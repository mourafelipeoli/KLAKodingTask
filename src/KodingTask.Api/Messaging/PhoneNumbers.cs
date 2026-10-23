using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace KodingTask.Api.Messaging;

public static partial class PhoneNumbers
{
    public static bool TryNormalize(string? input, [NotNullWhen(true)] out string? phoneE164)
    {
        phoneE164 = null;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var compact = Separators().Replace(input.Trim(), string.Empty);
        if (compact.StartsWith("00", StringComparison.Ordinal))
            compact = "+" + compact[2..];
        if (!E164().IsMatch(compact))
            return false;

        phoneE164 = compact;
        return true;
    }

    [GeneratedRegex(@"[\s\-().]")]
    private static partial Regex Separators();

    [GeneratedRegex(@"^\+[1-9]\d{7,14}$")]
    private static partial Regex E164();
}
