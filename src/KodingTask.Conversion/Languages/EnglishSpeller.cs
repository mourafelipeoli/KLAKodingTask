namespace KodingTask.Conversion.Languages;

public sealed class EnglishSpeller : ICurrencySpeller
{
    private static readonly string[] Ones =
    [
        "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
        "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen",
    ];

    private static readonly string[] Tens =
        ["", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"];

    public LanguageInfo Language { get; } = new("en-US", "English (United States)", "English (US)");

    public string Spell(MonetaryAmount amount)
    {
        var dollars = $"{Cardinal(amount.Dollars)} {(amount.Dollars == 1 ? "dollar" : "dollars")}";
        return amount.Cents == 0
            ? dollars
            : $"{dollars} and {Cardinal(amount.Cents)} {(amount.Cents == 1 ? "cent" : "cents")}";
    }

    internal static string Cardinal(int number)
    {
        if (number == 0)
            return Ones[0];

        var (millions, thousands, units) = NumberGroups.Split(number);
        var words = new List<string>(3);
        if (millions > 0)
            words.Add($"{UpTo999(millions)} million");
        if (thousands > 0)
            words.Add($"{UpTo999(thousands)} thousand");
        if (units > 0)
            words.Add(UpTo999(units));
        return string.Join(' ', words);
    }

    private static string UpTo999(int number)
    {
        var hundreds = number / 100;
        var rest = number % 100;
        if (hundreds == 0)
            return UpTo99(rest);

        var head = $"{Ones[hundreds]} hundred";
        return rest == 0 ? head : $"{head} {UpTo99(rest)}";
    }

    private static string UpTo99(int number)
    {
        if (number < 20)
            return Ones[number];

        var unit = number % 10;
        return unit == 0 ? Tens[number / 10] : $"{Tens[number / 10]}-{Ones[unit]}";
    }
}
