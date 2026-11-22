using Dalamud.Game.Gui.NamePlate;

namespace StreamModeXIV.Replacers;

/// <summary>Overhead nameplates of player characters (name, FC tag is left alone).</summary>
public sealed class NamePlateReplacer : IDisposable
{
    public NamePlateReplacer() => Plugin.NamePlateGui.OnNamePlateUpdate += OnUpdate;

    private static void OnUpdate(INamePlateUpdateContext context, IReadOnlyList<INamePlateUpdateHandler> handlers)
    {
        if (!Plugin.Config.StreamModeEnabled) return;
        foreach (var h in handlers)
        {
            if (h.NamePlateKind != NamePlateKind.PlayerCharacter) continue;
            var name = h.Name.TextValue;
            if (string.IsNullOrEmpty(name)) continue;
            NameAbbreviator.Learn(name);
            h.Name = NameAbbreviator.Abbreviate(name);

            // Titles can embed names too (rare); only touch known names.
            var title = h.Title.TextValue;
            var newTitle = NameAbbreviator.ReplaceKnown(title);
            if (newTitle != title) h.Title = newTitle;
        }
    }

    public void Dispose() => Plugin.NamePlateGui.OnNamePlateUpdate -= OnUpdate;
}
