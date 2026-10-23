namespace KodingTask.Conversion.Tests;

public sealed class ConverterContractTests
{
    private static readonly CurrencyConverter Converter = new(SpellerCatalog.Default);

    [Fact]
    public void Lists_the_supported_languages_in_display_order()
    {
        var codes = Converter.Languages.Select(language => language.Code);
        Assert.Equal(["en-US", "pt-BR", "de-DE", "fr-FR", "fr-CA", "fr-CH", "fr-BE", "it-IT", "es-ES", "pl-PL", "zh-CN", "zh-TW", "ja-JP", "ko-KR"], codes);
    }

    [Theory]
    [InlineData("EN-us", "en-US")]
    [InlineData("fr_be", "fr-BE")]
    [InlineData("zh-tw", "zh-TW")]
    public void Normalises_the_language_code(string requested, string canonical)
    {
        var result = Converter.Convert("5", requested);
        Assert.True(result.IsSuccess);
        Assert.Equal(canonical, result.LanguageCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sv-SE")]
    [InlineData("fr")]
    public void Refuses_unknown_languages_before_parsing(string? language)
    {
        var result = Converter.Convert("not a number", language);
        Assert.False(result.IsSuccess);
        Assert.Equal(ConversionError.UnsupportedLanguage, result.Error);
    }

    [Fact]
    public void Reports_parse_errors_with_the_language()
    {
        var result = Converter.Convert("abc", "de-DE");
        Assert.Equal(ConversionError.InvalidCharacter, result.Error);
        Assert.Equal("de-DE", result.LanguageCode);
        Assert.Null(result.Words);
    }

    public static TheoryData<string> LanguageCodes() => [.. SpellerCatalog.Default.Languages.Select(language => language.Code)];

    [Theory]
    [MemberData(nameof(LanguageCodes))]
    public void Produces_clean_words_across_the_whole_range(string language)
    {
        SpellerCatalog.Default.TryGet(language, out var speller);
        var random = new Random(20261023);
        var samples = Enumerable.Range(0, 2_100)
            .Select(number => new MonetaryAmount(number, number % 100))
            .Concat(Enumerable.Range(0, 3_000).Select(_ =>
                new MonetaryAmount(random.Next(0, MonetaryAmount.MaxDollars + 1), random.Next(0, 100))))
            .Append(new MonetaryAmount(MonetaryAmount.MaxDollars, MonetaryAmount.MaxCents));

        foreach (var amount in samples)
        {
            var words = speller!.Spell(amount);
            Assert.False(string.IsNullOrWhiteSpace(words));
            Assert.DoesNotContain(words, char.IsDigit);
            Assert.DoesNotContain("  ", words);
            Assert.DoesNotContain("--", words);
            Assert.Equal(words.Trim(), words);
            Assert.Equal(words, speller.Spell(amount));
        }
    }
}
