namespace KodingTask.Conversion.Languages;

public sealed class PolishSpeller : ICurrencySpeller
{
    private static readonly string[] Ones =
    [
        "zero", "jeden", "dwa", "trzy", "cztery", "pięć", "sześć", "siedem", "osiem", "dziewięć",
        "dziesięć", "jedenaście", "dwanaście", "trzynaście", "czternaście", "piętnaście", "szesnaście", "siedemnaście", "osiemnaście", "dziewiętnaście",
    ];

    private static readonly string[] Tens =
        ["", "", "dwadzieścia", "trzydzieści", "czterdzieści", "pięćdziesiąt", "sześćdziesiąt", "siedemdziesiąt", "osiemdziesiąt", "dziewięćdziesiąt"];

    private static readonly string[] Hundreds =
        ["", "sto", "dwieście", "trzysta", "czterysta", "pięćset", "sześćset", "siedemset", "osiemset", "dziewięćset"];

    private static readonly NounForms Dollar = new("dolar", "dolary", "dolarów");
    private static readonly NounForms Cent = new("cent", "centy", "centów");
    private static readonly NounForms Thousand = new("tysiąc", "tysiące", "tysięcy");
    private static readonly NounForms Million = new("milion", "miliony", "milionów");

    public LanguageInfo Language { get; } = new("pl-PL", "Polish (Poland)", "Polski (Polska)");

    public string Spell(MonetaryAmount amount)
    {
        var dollars = $"{Cardinal(amount.Dollars)} {Dollar.For(amount.Dollars)}";
        return amount.Cents == 0
            ? dollars
            : $"{dollars} i {Cardinal(amount.Cents)} {Cent.For(amount.Cents)}";
    }

    internal static string Cardinal(int number)
    {
        if (number == 0)
            return Ones[0];

        var (millions, thousands, units) = NumberGroups.Split(number);
        var words = new List<string>(3);
        if (millions > 0)
            words.Add(Group(millions, Million));
        if (thousands > 0)
            words.Add(Group(thousands, Thousand));
        if (units > 0)
            words.Add(UpTo999(units));
        return string.Join(' ', words);
    }

    // "tysiąc", "dwa tysiące", "pięć tysięcy": a lone one is not said before the noun.
    private static string Group(int count, NounForms noun) =>
        count == 1 ? noun.One : $"{UpTo999(count)} {noun.For(count)}";

    private static string UpTo999(int number)
    {
        var words = new List<string>(3);
        if (number >= 100)
            words.Add(Hundreds[number / 100]);

        var rest = number % 100;
        if (rest >= 20)
        {
            words.Add(Tens[rest / 10]);
            if (rest % 10 > 0)
                words.Add(Ones[rest % 10]);
        }
        else if (rest > 0)
        {
            words.Add(Ones[rest]);
        }

        return string.Join(' ', words);
    }

    private sealed record NounForms(string One, string Few, string Many)
    {
        // Polish counts nouns three ways: exactly one; 2-4 in the last digit except 12-14;
        // everything else (0, 5-21, 25-31 …) takes the genitive plural.
        public string For(int count)
        {
            if (count == 1)
                return One;
            var lastDigit = count % 10;
            var lastTwoDigits = count % 100;
            return lastDigit is >= 2 and <= 4 && lastTwoDigits is not (>= 12 and <= 14) ? Few : Many;
        }
    }
}
