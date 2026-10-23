namespace KodingTask.Conversion.Tests;

public sealed class AmountParserTests
{
    [Theory]
    [InlineData("0", 0, 0)]
    [InlineData("1", 1, 0)]
    [InlineData("25,1", 25, 10)]
    [InlineData("0,01", 0, 1)]
    [InlineData("45 100", 45_100, 0)]
    [InlineData("999 999 999,99", 999_999_999, 99)]
    [InlineData("999999999", 999_999_999, 0)]
    [InlineData("  7  ", 7, 0)]
    [InlineData("007", 7, 0)]
    [InlineData("12,05", 12, 5)]
    [InlineData("1 000", 1_000, 0)]
    [InlineData("1 000,5", 1_000, 50)]
    [InlineData("1 234 567", 1_234_567, 0)]
    [InlineData("000 000 001 000", 1_000, 0)]
    public void Accepts(string input, int dollars, int cents)
    {
        Assert.True(AmountParser.TryParse(input, out var amount, out var error), $"refused with {error}");
        Assert.Equal(new MonetaryAmount(dollars, cents), amount);
    }

    [Theory]
    [InlineData(null, ConversionError.Empty)]
    [InlineData("", ConversionError.Empty)]
    [InlineData("   ", ConversionError.Empty)]
    [InlineData("1 000 000 000", ConversionError.DollarsOutOfRange)]
    [InlineData("1000000000", ConversionError.DollarsOutOfRange)]
    [InlineData("-5", ConversionError.NegativeAmount)]
    [InlineData("−5", ConversionError.NegativeAmount)]
    [InlineData("5.00", ConversionError.DecimalPointNotAllowed)]
    [InlineData("1.000,50", ConversionError.DecimalPointNotAllowed)]
    [InlineData("1,2,3", ConversionError.MultipleCommas)]
    [InlineData(",5", ConversionError.MissingDollars)]
    [InlineData("5,", ConversionError.InvalidCents)]
    [InlineData("5, 10", ConversionError.InvalidCents)]
    [InlineData("5,123", ConversionError.TooManyCentDigits)]
    [InlineData("4 5100", ConversionError.InvalidGrouping)]
    [InlineData("45  100", ConversionError.InvalidGrouping)]
    [InlineData("1000 000", ConversionError.InvalidGrouping)]
    [InlineData("25 ,1", ConversionError.InvalidGrouping)]
    [InlineData("abc", ConversionError.InvalidCharacter)]
    [InlineData("$5", ConversionError.InvalidCharacter)]
    [InlineData("1e5", ConversionError.InvalidCharacter)]
    [InlineData("١٢", ConversionError.InvalidCharacter)]
    [InlineData("12345678901234567890123456789012345678901", ConversionError.TooLong)]
    public void Refuses(string? input, ConversionError expected)
    {
        Assert.False(AmountParser.TryParse(input, out _, out var error));
        Assert.Equal(expected, error);
    }

    [Theory]
    [InlineData(45_100, 0, "45 100,00")]
    [InlineData(0, 1, "0,01")]
    [InlineData(999_999_999, 99, "999 999 999,99")]
    public void Writes_the_canonical_form(int dollars, int cents, string expected) =>
        Assert.Equal(expected, new MonetaryAmount(dollars, cents).ToCanonicalString());

    [Fact]
    public void Rejects_out_of_range_values_at_construction()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MonetaryAmount(MonetaryAmount.MaxDollars + 1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new MonetaryAmount(0, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => new MonetaryAmount(-1, 0));
    }
}
