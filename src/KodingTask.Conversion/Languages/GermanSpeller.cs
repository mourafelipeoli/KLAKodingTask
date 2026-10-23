namespace KodingTask.Conversion.Languages;

public sealed class GermanSpeller : ICurrencySpeller
{
    private static readonly string[] Ones =
    [
        "null", "eins", "zwei", "drei", "vier", "fünf", "sechs", "sieben", "acht", "neun",
        "zehn", "elf", "zwölf", "dreizehn", "vierzehn", "fünfzehn", "sechzehn", "siebzehn", "achtzehn", "neunzehn",
    ];

    private static readonly string[] Tens =
        ["", "", "zwanzig", "dreißig", "vierzig", "fünfzig", "sechzig", "siebzig", "achtzig", "neunzig"];

    private enum TrailingOne
    {
        Standalone,
        Compound,
        Feminine,
    }

    public LanguageInfo Language { get; } = new("de-DE", "German (Germany)", "Deutsch (Deutschland)");

    public string Spell(MonetaryAmount amount)
    {
        var dollars = $"{Counted(amount.Dollars)} Dollar";
        return amount.Cents == 0 ? dollars : $"{dollars} und {Counted(amount.Cents)} Cent";
    }

    internal static string Cardinal(int number)
    {
        if (number == 0)
            return Ones[0];

        var (millions, thousands, units) = NumberGroups.Split(number);
        var words = new List<string>(2);
        if (millions > 0)
            words.Add(millions == 1 ? "eine Million" : $"{UpTo999(millions, TrailingOne.Feminine)} Millionen");

        var belowMillion = string.Concat(
            thousands > 0 ? UpTo999(thousands, TrailingOne.Compound) + "tausend" : string.Empty,
            units > 0 ? UpTo999(units, TrailingOne.Standalone) : string.Empty);
        if (belowMillion.Length > 0)
            words.Add(belowMillion);

        return string.Join(' ', words);
    }

    private static string Counted(int number) => number == 1 ? "ein" : Cardinal(number);

    private static string UpTo999(int number, TrailingOne trailingOne)
    {
        var hundreds = number / 100;
        var rest = number % 100;
        var head = hundreds switch
        {
            0 => string.Empty,
            1 => "einhundert",
            _ => Ones[hundreds] + "hundert",
        };
        return rest == 0 ? head : head + UpTo99(rest, trailingOne);
    }

    private static string UpTo99(int number, TrailingOne trailingOne)
    {
        if (number == 1)
        {
            return trailingOne switch
            {
                TrailingOne.Compound => "ein",
                TrailingOne.Feminine => "eine",
                _ => "eins",
            };
        }

        if (number < 20)
            return Ones[number];

        var unit = number % 10;
        var tens = Tens[number / 10];
        if (unit == 0)
            return tens;

        return (unit == 1 ? "ein" : Ones[unit]) + "und" + tens;
    }
}
