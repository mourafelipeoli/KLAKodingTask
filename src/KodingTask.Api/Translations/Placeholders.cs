using System.Text.RegularExpressions;

namespace KodingTask.Api.Translations;

public static partial class Placeholders
{
    public static string Protect(string text) =>
        Placeholder().Replace(Escape(text), match => $"<x>{match.Value}</x>");

    public static string Restore(string text) =>
        Unescape(Wrapped().Replace(text, "$1"));

    private static string Escape(string text) =>
        text.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);

    private static string Unescape(string text) =>
        text.Replace("&lt;", "<", StringComparison.Ordinal)
            .Replace("&gt;", ">", StringComparison.Ordinal)
            .Replace("&quot;", "\"", StringComparison.Ordinal)
            .Replace("&apos;", "'", StringComparison.Ordinal)
            .Replace("&amp;", "&", StringComparison.Ordinal);

    [GeneratedRegex(@"\{\{\w+\}\}")]
    private static partial Regex Placeholder();

    [GeneratedRegex(@"<x>\s*(\{\{\w+\}\})\s*</x>")]
    private static partial Regex Wrapped();
}
