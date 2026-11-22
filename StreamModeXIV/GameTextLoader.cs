using Lumina.Excel.Sheets;

namespace StreamModeXIV;

/// <summary>Collects every game-authored string that could look like "Firstname Lastname".</summary>
public static class GameTextLoader
{
    public static void LoadAsync() => Task.Run(() =>
    {
        try
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            void Add(string s) { s = s.Trim(); if (s.Length > 0) set.Add(s); }

            foreach (var r in Plugin.DataManager.GetExcelSheet<Addon>()) Add(r.Text.ExtractText());
            foreach (var r in Plugin.DataManager.GetExcelSheet<ContentFinderCondition>()) Add(r.Name.ExtractText());
            foreach (var r in Plugin.DataManager.GetExcelSheet<PlaceName>()) Add(r.Name.ExtractText());
            foreach (var r in Plugin.DataManager.GetExcelSheet<Title>()) { Add(r.Masculine.ExtractText()); Add(r.Feminine.ExtractText()); }

            NameAbbreviator.GameText = set;
            Plugin.Log.Debug($"Loaded {set.Count} game strings to exclude from name matching.");
        }
        catch (Exception e) { Plugin.Log.Error(e, "Failed to load game text"); }
    });
}
