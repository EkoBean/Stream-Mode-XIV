using Dalamud.Game.Gui.NamePlate;

namespace StreamModeXIV.Replacers;

/// <summary>
/// Overhead nameplates of player characters.
///
/// Must run in OnNamePlateUpdate: Dalamud calls OnPostNamePlateUpdate only after the game
/// has already consumed the string array, so edits made there never reach the screen.
///
/// Cooperation with other plugins (e.g. PartyIcons): handler.Name reads the live string
/// array, so it already contains edits from plugins whose handler ran before ours. We only
/// swap the full player name for initials when it is still present:
///  - another plugin ran first and replaced the name (slot number, role letters, big job
///    icon spacer) -> the full name is gone, we leave it alone;
///  - another plugin runs after us -> it overwrites our value, its display wins;
///  - another plugin keeps the name (e.g. PartyIcons "small job icon") -> we abbreviate it.
/// </summary>
public sealed class NamePlateReplacer : IDisposable
{
    public NamePlateReplacer() => Plugin.NamePlateGui.OnNamePlateUpdate += OnUpdate;

    private static void OnUpdate(INamePlateUpdateContext context, IReadOnlyList<INamePlateUpdateHandler> handlers)
    {
        if (!Plugin.Config.StreamModeEnabled) return;
        foreach (var h in handlers)
        {
            if (h.NamePlateKind != NamePlateKind.PlayerCharacter) continue;

            var fullName = h.PlayerCharacter?.Name.TextValue;
            if (string.IsNullOrEmpty(fullName)) continue;
            NameAbbreviator.Learn(fullName);

            var shown = h.Name;
            if (shown.TextValue.Contains(fullName, StringComparison.Ordinal))
                h.Name = ChatReplacer.Rewrite(shown); // keeps other plugins' icons/colours

            var title = h.Title;
            if (NameAbbreviator.ReplaceKnown(title.TextValue) != title.TextValue)
                h.Title = ChatReplacer.Rewrite(title);
        }
    }

    public void Dispose() => Plugin.NamePlateGui.OnNamePlateUpdate -= OnUpdate;
}
