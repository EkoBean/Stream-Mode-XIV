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
    /// Replace anything shaped like "Firstname Lastname". Only use inside windows whose
    /// text is (almost) purely player names, e.g. the friend list. Matches are learned.
    /// </summary>
    public static string ReplacePattern(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        text = NamePattern().Replace(text, m =>
        {
            Learn(m.Value);
            return Abbreviate(m.Value);
        });
        return ReplaceKnown(text);
    }
}
