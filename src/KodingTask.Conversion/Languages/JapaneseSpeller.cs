namespace KodingTask.Conversion.Languages;

public sealed class JapaneseSpeller : ICurrencySpeller
{
    private const string Digits = "零一二三四五六七八九";

    public LanguageInfo Language { get; } = new("ja-JP", "Japanese (Japan)", "日本語");

    public string Spell(MonetaryAmount amount)
    {
        var dollars = Cardinal(amount.Dollars) + "ドル";
        return amount.Cents == 0 ? dollars : dollars + Cardinal(amount.Cents) + "セント";
    }

    internal static string Cardinal(int number)
    {
        if (number == 0)
            return Digits[0].ToString();

        var (hundredMillions, tenThousands, units) = NumberGroups.SplitMyriads(number);
        var words = new System.Text.StringBuilder();
        var hasLargeUnit = number >= 10_000;
        if (hundredMillions > 0)
            words.Append(Group(hundredMillions, sayOneThousand: true)).Append('億');
        if (tenThousands > 0)
            words.Append(Group(tenThousands, sayOneThousand: true)).Append('万');
        if (units > 0)
            words.Append(Group(units, sayOneThousand: hasLargeUnit));
        return words.ToString();
    }

    private static string Group(int value, bool sayOneThousand)
    {
        var (thousands, hundreds, tens, ones) = NumberGroups.Digits(value);
        var words = new System.Text.StringBuilder();
        if (thousands > 0)
            words.Append(thousands == 1 && !sayOneThousand ? "千" : Digits[thousands] + "千");
        if (hundreds > 0)
            words.Append(hundreds == 1 ? "百" : Digits[hundreds] + "百");
        if (tens > 0)
            words.Append(tens == 1 ? "十" : Digits[tens] + "十");
        if (ones > 0)
            words.Append(Digits[ones]);
        return words.ToString();
    }
}
