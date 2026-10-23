namespace KodingTask.Conversion;

public sealed class CurrencyConverter(SpellerCatalog catalog)
{
    public IReadOnlyList<LanguageInfo> Languages => catalog.Languages;

    public ConversionResult Convert(string? input, string? languageCode)
    {
        if (!catalog.TryGet(languageCode, out var speller))
            return ConversionResult.Failed(languageCode ?? string.Empty, ConversionError.UnsupportedLanguage);

        if (!AmountParser.TryParse(input, out var amount, out var error))
            return ConversionResult.Failed(speller.Language.Code, error);

        return ConversionResult.Succeeded(speller.Language.Code, amount, speller.Spell(amount));
    }
}
