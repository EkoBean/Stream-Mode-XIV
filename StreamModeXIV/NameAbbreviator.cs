using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace StreamModeXIV;

/// <summary>
/// Turns "Bean Annyx" into "B.A" and rewrites strings that contain player names.
/// </summary>
public static partial class NameAbbreviator
{
    // FFXIV (global) names: two words, each starting upper-case, letters plus ' and -, total ≤ 21 chars.
    [GeneratedRegex(@"(?<![\p{L}'\-.])([A-Z][a-z'\-]{1,14}) ([A-Z][a-z'\-]{1,14})(?![\p{L}'\-])")]
    private static partial Regex NamePattern();

    /// <summary>Full names we have seen on real player characters / social lists.</summary>
    public static readonly ConcurrentDictionary<string, byte> KnownNames = new();

    public static string Abbreviate(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return fullName;
        if (parts.Length == 1) return char.ToUpperInvariant(parts[0][0]) + "."; // CN/KR single-token names
        return string.Join(".", parts.Select(p => char.ToUpperInvariant(p[0])));
    }

    public static void Learn(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return;
        fullName = fullName.Trim();
        if (fullName.Length < 3) return;
        KnownNames.TryAdd(fullName, 0);
    }

    /// <summary>Replace only names we know belong to players. Safe for chat and generic text.</summary>
    public static string ReplaceKnown(string text)
    {
        if (string.IsNullOrEmpty(text) || KnownNames.IsEmpty) return text;
        foreach (var name in KnownNames.Keys.OrderByDescending(n => n.Length))
        {
            if (text.Contains(name, StringComparison.Ordinal))
                text = text.Replace(name, Abbreviate(name), StringComparison.Ordinal);
        }
        return text;
    }

    /// <summary>
    /// Game-authored strings (UI labels, tab names, duties, places, titles). These are never
    /// treated as player names even if they look like "Friend List".
    /// </summary>
    public static volatile HashSet<string> GameText = new(StringComparer.Ordinal);

    /// <summary>
    /// For name-list windows: abbreviate a text piece only if the WHOLE piece is a name
    /// (e.g. the recruiter field). Anything else (descriptions, tags, duty names) is left alone
    /// completely. Matches are learned.
    /// </summary>
    public static string ReplaceWholeName(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var trimmed = text.Trim();
        var m = NamePattern().Match(trimmed);
        if (m.Success && m.Length == trimmed.Length && !GameText.Contains(trimmed))
        {
            Learn(trimmed);
            return text.Replace(trimmed, Abbreviate(trimmed), StringComparison.Ordinal);
        }
        return text; // descriptions, duty names, tags: untouched
    }
}
