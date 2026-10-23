namespace KodingTask.Conversion.Languages;

public sealed class ChineseSpeller(LanguageInfo language, ChineseScript script) : ICurrencySpeller
{
    private const string Digits = "零一二三四五六七八九";
    private const char Zero = '零';
    private const char Two = '二';

    private readonly string _tenThousand = script == ChineseScript.Simplified ? "万" : "萬";
    private readonly string _hundredMillion = script == ChineseScript.Simplified ? "亿" : "億";
    private readonly char _twoBeforeMeasure = script == ChineseScript.Simplified ? '两' : '兩';

    public LanguageInfo Language { get; } = language;

    public ChineseScript Script { get; } = script;

    public string Spell(MonetaryAmount amount)
    {
        var dollars = Cardinal(amount.Dollars) + "美元";
        return amount.Cents == 0 ? dollars : dollars + Cardinal(amount.Cents) + "美分";
    }

    internal string Cardinal(int number)
    {
        if (number == 0)
            return Zero.ToString();
        if (number == 2)
            return _twoBeforeMeasure.ToString();

        var (hundredMillions, tenThousands, units) = NumberGroups.SplitMyriads(number);
        var words = new System.Text.StringBuilder();
        var gap = false;
        foreach (var (value, unit) in new[] { (hundredMillions, _hundredMillion), (tenThousands, _tenThousand), (units, string.Empty) })
        {
            if (value == 0)
            {
                gap |= words.Length > 0;
                continue;
            }

            if (words.Length > 0 && (gap || value < 1_000))
                words.Append(Zero);
            words.Append(Group(value, isFirst: words.Length == 0)).Append(unit);
            gap = false;
        }

        return UseTwoBeforeLargeUnit(words.ToString());
    }

    private static string Group(int value, bool isFirst)
    {
        var (thousands, hundreds, tens, ones) = NumberGroups.Digits(value);
        var words = new System.Text.StringBuilder();
        var pendingZero = false;
        foreach (var (digit, unit) in new[] { (thousands, "千"), (hundreds, "百"), (tens, "十"), (ones, string.Empty) })
        {
            if (digit == 0)
            {
                pendingZero |= words.Length > 0;
                continue;
            }

            if (pendingZero)
                words.Append(Zero);
            pendingZero = false;

            var omitOne = digit == 1 && unit == "十" && isFirst && words.Length == 0;
            if (!omitOne)
                words.Append(Digits[digit]);
            words.Append(unit);
        }

        return words.ToString();
    }

    private string UseTwoBeforeLargeUnit(string words) =>
        words.Length > 1 && words[0] == Two && (words[1] == '千' || words[1] == _tenThousand[0] || words[1] == _hundredMillion[0])
            ? _twoBeforeMeasure + words[1..]
            : words;
}
