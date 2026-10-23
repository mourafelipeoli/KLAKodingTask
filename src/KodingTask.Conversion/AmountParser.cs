using System.Buffers;
using System.Globalization;

namespace KodingTask.Conversion;

public static class AmountParser
{
    public const int MaxInputLength = 40;

    private const string GroupSeparatorCharacters = "    ";

    private static readonly SearchValues<char> GroupSeparators = SearchValues.Create(GroupSeparatorCharacters);

    private static readonly char[] GroupSeparatorArray = GroupSeparatorCharacters.ToCharArray();

    public static bool TryParse(string? input, out MonetaryAmount amount, out ConversionError error)
    {
        amount = default;
        error = default;

        if (Parse((input ?? string.Empty).Trim(), out var dollars, out var cents) is { } failure)
        {
            error = failure;
            return false;
        }

        amount = new MonetaryAmount(dollars, cents);
        return true;
    }

    private static ConversionError? Parse(string text, out int dollars, out int cents)
    {
        dollars = 0;
        cents = 0;

        if (text.Length == 0)
            return ConversionError.Empty;
        if (text.Length > MaxInputLength)
            return ConversionError.TooLong;
        if (text[0] is '-' or '−')
            return ConversionError.NegativeAmount;

        foreach (var character in text)
        {
            if (char.IsAsciiDigit(character) || character == ',' || GroupSeparators.Contains(character))
                continue;
            return character == '.' ? ConversionError.DecimalPointNotAllowed : ConversionError.InvalidCharacter;
        }

        var comma = text.IndexOf(',');
        if (comma != text.LastIndexOf(','))
            return ConversionError.MultipleCommas;

        var integerPart = comma < 0 ? text : text[..comma];
        var fractionPart = comma < 0 ? null : text[(comma + 1)..];

        if (integerPart.Length == 0)
            return ConversionError.MissingDollars;

        if (fractionPart is not null)
        {
            if (fractionPart.Length == 0 || !fractionPart.All(char.IsAsciiDigit))
                return ConversionError.InvalidCents;
            if (fractionPart.Length > 2)
                return ConversionError.TooManyCentDigits;
        }

        if (integerPart.AsSpan().ContainsAny(GroupSeparators))
        {
            var groups = integerPart.Split(GroupSeparatorArray);
            if (groups[0].Length is < 1 or > 3 || groups.Skip(1).Any(group => group.Length != 3))
                return ConversionError.InvalidGrouping;
            integerPart = string.Concat(groups);
        }

        var significant = integerPart.TrimStart('0');
        if (significant.Length > 9)
            return ConversionError.DollarsOutOfRange;

        dollars = significant.Length == 0 ? 0 : int.Parse(significant, NumberStyles.None, CultureInfo.InvariantCulture);
        cents = fractionPart switch
        {
            null => 0,
            { Length: 1 } => (fractionPart[0] - '0') * 10,
            _ => int.Parse(fractionPart, NumberStyles.None, CultureInfo.InvariantCulture),
        };
        return null;
    }
}
