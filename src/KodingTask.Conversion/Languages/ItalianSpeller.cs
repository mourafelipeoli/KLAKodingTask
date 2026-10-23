namespace KodingTask.Conversion.Languages;

public sealed class ItalianSpeller : ICurrencySpeller
{
    private static readonly string[] Ones =
    [
        "zero", "uno", "due", "tre", "quattro", "cinque", "sei", "sette", "otto", "nove",
        "dieci", "undici", "dodici", "tredici", "quattordici", "quindici", "sedici", "diciassette", "diciotto", "diciannove",
    ];

    private static readonly string[] Tens =
        ["", "", "venti", "trenta", "quaranta", "cinquanta", "sessanta", "settanta", "ottanta", "novanta"];

    public LanguageInfo Language { get; } = new("it-IT", "Italian (Italy)", "Italiano (Italia)");

    public string Spell(MonetaryAmount amount)
    {
        var dollars = amount.Dollars switch
        {
            1 => "un dollaro",
            var exactMillions when exactMillions % 1_000_000 == 0 && exactMillions > 0 => $"{Cardinal(exactMillions)} di dollari",
            var other => $"{Cardinal(other)} dollari",
        };

        if (amount.Cents == 0)
            return dollars;

        var cents = amount.Cents == 1 ? "un centesimo" : $"{Cardinal(amount.Cents)} centesimi";
        return $"{dollars} e {cents}";
    }

    internal static string Cardinal(int number)
    {
        if (number == 0)
            return Ones[0];

        var (millions, thousands, units) = NumberGroups.Split(number);
        var words = new List<string>(2);
        if (millions > 0)
            words.Add(millions == 1 ? "un milione" : $"{WithFinalAccent(UpTo999(millions))} milioni");

        var belowMillion = string.Concat(
            thousands switch
            {
                0 => string.Empty,
                1 => "mille",
                _ => UpTo999(thousands) + "mila",
            },
            units > 0 ? UpTo999(units) : string.Empty);
        if (belowMillion.Length > 0)
            words.Add(WithFinalAccent(belowMillion));

        return string.Join(' ', words);
    }

    private static string WithFinalAccent(string word) =>
        word.Length > 3 && word.EndsWith("tre", StringComparison.Ordinal) ? word[..^3] + "tré" : word;

    private static string UpTo999(int number)
    {
        var hundreds = number / 100;
        var rest = number % 100;
        var head = hundreds switch
        {
            0 => string.Empty,
            1 => "cento",
            _ => Ones[hundreds] + "cento",
        };

        if (rest == 0)
            return head;

        var tail = UpTo99(rest);
        var elidesBeforeEighty = head.Length > 0 && rest is >= 80 and <= 89;
        return elidesBeforeEighty ? head[..^1] + tail : head + tail;
    }

    private static string UpTo99(int number)
    {
        if (number < 20)
            return Ones[number];

        var unit = number % 10;
        var tens = Tens[number / 10];
        return unit switch
        {
            0 => tens,
            1 or 8 => tens[..^1] + Ones[unit],
            _ => tens + Ones[unit],
        };
    }
}
