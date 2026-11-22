using Dalamud.Game.Gui.NamePlate;

namespace StreamModeXIV.Replacers;

/// <summary>
/// Overhead nameplates of player characters.
/// Runs in OnPostNamePlateUpdate, after other plugins (e.g. PartyIcons) have applied their
/// changes, and only abbreviates when the full player name is still visible. If another
/// plugin already replaced the name (party slot number, job icon, ...), it is left alone.
/// </summary>
public sealed class NamePlateReplacer : IDisposable
{
    public NamePlateReplacer() => Plugin.NamePlateGui.OnPostNamePlateUpdate += OnPostUpdate;

    private static void OnPostUpdate(INamePlateUpdateContext context, IReadOnlyList<INamePlateUpdateHandler> handlers)
    {
        if (!Plugin.Config.StreamModeEnabled) return;
        foreach (var h in handlers)
        {
            if (h.NamePlateKind != NamePlateKind.PlayerCharacter) continue;

            var fullName = h.PlayerCharacter?.Name.TextValue;
            if (string.IsNullOrEmpty(fullName)) continue;
            NameAbbreviator.Learn(fullName);

            // Name as it will actually be shown, including other plugins' edits.
            var shown = h.Name;
            if (shown.TextValue.Contains(fullName, StringComparison.Ordinal))
                h.Name = ChatReplacer.Rewrite(shown); // keeps colours/icons, swaps only the name text

            var title = h.Title;
            if (NameAbbreviator.ReplaceKnown(title.TextValue) != title.TextValue)
                h.Title = ChatReplacer.Rewrite(title);
        }
    }

    public void Dispose() => Plugin.NamePlateGui.OnPostNamePlateUpdate -= OnPostUpdate;
}
