using System.Text.RegularExpressions;

namespace KodingTask.Api.Ai;

public static partial class AiAnswer
{
    private const int MaxLength = 600;

    public static string? Clean(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length != 1)
            return null;

        var answer = Whitespace().Replace(lines[0], " ").Trim().Trim('"', '\'', '`', '“', '”', '«', '»').Trim().TrimEnd('.').Trim();
        if (answer.Length is 0 or > MaxLength || answer.Any(char.IsDigit))
            return null;

        return answer;
    }

    public static bool SameWords(string first, string second) =>
        string.Equals(Normalize(first), Normalize(second), StringComparison.Ordinal);

    private static string Normalize(string text) =>
        Whitespace().Replace(text.Replace('’', '\'').Replace('‑', '-').Replace(' ', ' '), " ").Trim().ToLowerInvariant();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
