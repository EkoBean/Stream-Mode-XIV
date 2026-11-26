using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace StreamModeXIV;

/// <summary>
/// Reads every party member name straight from the game's party data, so members are
/// known even when they are in another zone, not loaded nearby, or in a cross-world party
/// (Dalamud's IPartyList is empty for cross-world parties).
/// </summary>
public static unsafe class PartySource
{
    public static void Learn()
    {
        foreach (var m in Plugin.PartyList)
            NameAbbreviator.Learn(m.Name.TextValue);

        // Party list HUD: what _PartyList actually shows (normal + cross-world).
        var hud = AgentHUD.Instance();
        if (hud != null)
        {
            for (var i = 0; i < hud->PartyMemberCount && i < hud->PartyMembers.Length; i++)
            {
                var ptr = hud->PartyMembers[i].Name.Value;
                if (ptr == null) continue;
                var bytes = System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpanFromNullTerminated(ptr);
                NameAbbreviator.Learn(Dalamud.Game.Text.SeStringHandling.SeString.Parse(bytes).TextValue);
            }
        }

        // Cross-world party / alliance groups.
        var cr = InfoProxyCrossRealm.Instance();
        if (cr != null)
        {
            foreach (ref var group in cr->CrossRealmGroups)
                for (var i = 0; i < group.GroupMemberCount && i < group.GroupMembers.Length; i++)
                    NameAbbreviator.Learn(group.GroupMembers[i].NameString);
        }
    }
}
