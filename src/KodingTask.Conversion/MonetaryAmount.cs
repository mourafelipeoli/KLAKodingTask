using System.Globalization;

namespace KodingTask.Conversion;

public readonly record struct MonetaryAmount
{
    public const int MaxDollars = 999_999_999;
    public const int MaxCents = 99;

    public MonetaryAmount(int dollars, int cents)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(dollars);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dollars, MaxDollars);
        ArgumentOutOfRangeException.ThrowIfNegative(cents);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(cents, MaxCents);
        Dollars = dollars;
        Cents = cents;
    }

    public int Dollars { get; }

    public int Cents { get; }

    public string ToCanonicalString()
    {
        var grouped = Dollars.ToString("#,0", CultureInfo.InvariantCulture).Replace(',', ' ');
        return $"{grouped},{Cents:00}";
    }
}
