namespace KodingTask.Conversion.Languages;

public sealed class FrenchSpeller(LanguageInfo language, FrenchNumbering numbering) : ICurrencySpeller
{
    private static readonly string[] Ones =
    [
        "zéro", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf",
        "dix", "onze", "douze", "treize", "quatorze", "quinze", "seize", "dix-sept", "dix-huit", "dix-neuf",
    ];

    private static readonly string[] Tens = ["", "", "vingt", "trente", "quarante", "cinquante", "soixante"];

    public LanguageInfo Language { get; } = language;

    public FrenchNumbering Numbering { get; } = numbering;

    public string Spell(MonetaryAmount amount)
    {
        var dollars = amount.Dollars;
        var dollarsText = dollars > 0 && dollars % 1_000_000 == 0
            ? $"{Cardinal(dollars)} de dollars"
            : $"{Cardinal(dollars)} {(dollars <= 1 ? "dollar" : "dollars")}";

        if (amount.Cents == 0)
            return dollarsText;

        return $"{dollarsText} et {Cardinal(amount.Cents)} {(amount.Cents == 1 ? "cent" : "cents")}";
    }

    internal string Cardinal(int number)
    {
        if (number == 0)
            return Ones[0];

        var (millions, thousands, units) = NumberGroups.Split(number);
        var words = new List<string>(3);
        if (millions > 0)
            words.Add(millions == 1 ? "un million" : $"{UpTo999(millions, pluralEndings: true)} millions");
        if (thousands > 0)
            words.Add(thousands == 1 ? "mille" : $"{UpTo999(thousands, pluralEndings: false)} mille");
        if (units > 0)
            words.Add(UpTo999(units, pluralEndings: true));
        return string.Join(' ', words);
    }

    private string UpTo999(int number, bool pluralEndings)
    {
        var hundreds = number / 100;
        var rest = number % 100;
        if (hundreds == 0)
            return UpTo99(rest, pluralEndings);

        var head = hundreds == 1
            ? "cent"
            : $"{Ones[hundreds]} {(rest == 0 && pluralEndings ? "cents" : "cent")}";
        return rest == 0 ? head : $"{head} {UpTo99(rest, pluralEndings)}";
    }

    private string UpTo99(int number, bool pluralEndings)
    {
        if (number < 20)
            return Ones[number];

        var unit = number % 10;
        return (number / 10) switch
        {
            <= 6 => Compose(Tens[number / 10], unit),
            7 => Numbering == FrenchNumbering.Standard ? SixtyPlusTeen(unit) : Compose("septante", unit),
            8 => Numbering == FrenchNumbering.Swiss ? Compose("huitante", unit) : FourTwenties(unit, pluralEndings),
            _ => Numbering == FrenchNumbering.Standard ? "quatre-vingt-" + Ones[10 + unit] : Compose("nonante", unit),
        };
    }

    private static string Compose(string tens, int unit) => unit switch
    {
        0 => tens,
        1 => tens + " et un",
        _ => tens + "-" + Ones[unit],
    };

    private static string SixtyPlusTeen(int unit) =>
        unit == 1 ? "soixante et onze" : "soixante-" + Ones[10 + unit];

    private static string FourTwenties(int unit, bool pluralEndings) => unit switch
    {
        0 => pluralEndings ? "quatre-vingts" : "quatre-vingt",
        _ => "quatre-vingt-" + Ones[unit],
    };
}
