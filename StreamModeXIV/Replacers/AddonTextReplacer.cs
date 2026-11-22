using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace StreamModeXIV.Replacers;

/// <summary>
/// Rewrites text nodes of game windows right before they are drawn.
/// "Name-list" windows abbreviate text pieces that are entirely a name (labels/descriptions untouched); "mixed" windows
/// only get names we already know belong to players (so boss / item names survive).
/// </summary>
public sealed unsafe class AddonTextReplacer : IDisposable
{
    /// <summary>Windows whose text is essentially a list of player names.</summary>
    public static readonly string[] NameListAddons =
    [
        "Social",                // Social window frame
        "PartyMemberList",       // Social > Party Members
        "FriendList",            // Social > Friend List
        "SocialList",            // Social > Player Search results
        "BlackList",             // Blacklist
        "MuteList",              // Mute list
        "ContactList",           // Contacts / recent contacts
        "FreeCompanyMember",     // Free Company > Members
        "FreeCompanyMemberList",
        "LinkShell",             // Linkshells
        "CrossWorldLinkshell",   // Cross-world Linkshells
        "CircleList",            // Fellowships member list
        "CircleBook",
        "ContentMemberList",
        "LookingForGroup",       // Party Finder list: only the recruiter field (whole-node name)
        "LookingForGroupDetail", // Party Finder detail
    ];

    /// <summary>Windows that mix player names with other text: only known player names are replaced.</summary>
    public static readonly string[] MixedAddons =
    [
        "_PartyList",                  // names come from IPartyList, text also holds level etc.
        "_AllianceList1",
        "_AllianceList2",
        "_TargetInfo",                 // target bar + target-of-target
        "_TargetInfoMainTarget",       // split target bar
        "_TargetInfoCastBar",
        "_FocusTargetInfo",
        "_EnemyList",                  // enemy list (shows who boss is targeting via marks)
        "CharaCard",                   // Adventurer Plate
        "Character",                   // Character window (name + title)
        "CharacterInspect",            // Examine
        "FreeCompany",                 // FC main window (master name)
        "FreeCompanyProfile",
        "LookingForGroupCondition",
        "ReadyCheck",
        "Trade",
        "ContextMenu",
        "Talk",                        // NPC dialogue that addresses you by name
        "SelectString",
        "SelectYesno",
        "_NaviMap",                    // minimap tooltip
        "Tooltip",
        "MKSRecord",                   // Crystalline Conflict scoreboard
        "PvPMKSIntroduction",
        "FrontlineRecord",
        "MiragePrismMiragePlate",
        "JournalResult",
        "ItemDetail",                  // crafted-by signature
        "HousingSignBoard",            // estate owner
        "MarketBoardItemSearch",
        "ItemSearchResult",            // retainer/seller lists
        "RecipeNote",
        "_DTR",
    ];

    private readonly HashSet<string> nameList;
    private readonly string[] all;
    // text-node address -> (original bytes, bytes we wrote)
    private readonly Dictionary<nint, (byte[] Original, byte[] Written)> touched = new();

    public AddonTextReplacer()
    {
        nameList = new HashSet<string>(NameListAddons.Concat(Plugin.Config.ExtraNameAddons));
        all = nameList.Concat(MixedAddons).Distinct().ToArray();
        Plugin.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, all, OnPreDraw);
        Plugin.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, all, OnFinalize);
    }

    private void OnPreDraw(AddonEvent type, AddonArgs args)
    {
        if (!Plugin.Config.StreamModeEnabled) return;
        var addon = (AtkUnitBase*)args.Addon.Address;
        if (addon == null || !addon->IsVisible) return;
        var pattern = nameList.Contains(args.AddonName);
        Walk(&addon->UldManager, node => Rewrite(node, pattern), 0);
    }

    private void OnFinalize(AddonEvent type, AddonArgs args)
    {
        // Nodes are about to be freed; forget them so we never write to dead memory.
        var addon = (AtkUnitBase*)args.Addon.Address;
        if (addon == null) return;
        Walk(&addon->UldManager, node => touched.Remove((nint)node), 0);
    }

    private delegate void NodeAction(AtkTextNode* node);

    private static void Walk(AtkUldManager* uld, NodeAction action, int depth)
    {
        if (uld == null || depth > 8) return;
        for (var i = 0; i < uld->NodeListCount; i++)
        {
            var node = uld->NodeList[i];
            if (node == null) continue;
            if (node->Type == NodeType.Text)
            {
                action((AtkTextNode*)node);
            }
            else if ((ushort)node->Type >= 1000)
            {
                var comp = ((AtkComponentNode*)node)->Component;
                if (comp != null) Walk(&comp->UldManager, action, depth + 1);
            }
        }
    }

    private void Rewrite(AtkTextNode* node, bool pattern)
    {
        var current = ReadBytes(node);
        if (current.Length == 0) return;
        if (touched.TryGetValue((nint)node, out var t) && current.AsSpan().SequenceEqual(t.Written)) return;

        var se = SeString.Parse(current);
        var changed = false;
        foreach (var p in se.Payloads)
        {
            if (p is not TextPayload tp || string.IsNullOrEmpty(tp.Text)) continue;
            var replaced = pattern ? NameAbbreviator.ReplaceWholeName(tp.Text) : NameAbbreviator.ReplaceKnown(tp.Text);
            if (replaced == tp.Text) continue;
            tp.Text = replaced;
            changed = true;
        }
        if (!changed) return;

        var written = se.Encode();
        touched[(nint)node] = (current, written);
        WriteBytes(node, written);
    }

    /// <summary>Put original text back into any still-visible window.</summary>
    public void RestoreAll()
    {
        if (touched.Count == 0) return;
        foreach (var name in all)
        {
            var addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName(name).Address;
            if (addon == null) continue;
            Walk(&addon->UldManager, node =>
            {
                if (!touched.TryGetValue((nint)node, out var t)) return;
                if (ReadBytes(node).AsSpan().SequenceEqual(t.Written)) WriteBytes(node, t.Original);
            }, 0);
        }
        touched.Clear();
    }

    private static byte[] ReadBytes(AtkTextNode* node)
    {
        var ptr = node->NodeText.StringPtr.Value;
        if (ptr == null) return [];
        var len = 0;
        while (ptr[len] != 0) len++;
        return new ReadOnlySpan<byte>(ptr, len).ToArray();
    }

    private static void WriteBytes(AtkTextNode* node, byte[] bytes)
    {
        var buf = new byte[bytes.Length + 1];
        bytes.CopyTo(buf, 0);
        fixed (byte* p = buf) node->SetText(p);
    }

    public void Dispose()
    {
        Plugin.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, all, OnPreDraw);
        Plugin.AddonLifecycle.UnregisterListener(AddonEvent.PreFinalize, all, OnFinalize);
    }
}
