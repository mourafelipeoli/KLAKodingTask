namespace KodingTask.Conversion;

public interface ICurrencySpeller
{
    LanguageInfo Language { get; }

    string Spell(MonetaryAmount amount);
}
