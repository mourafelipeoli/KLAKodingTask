namespace KodingTask.Conversion.Languages;

public sealed class KoreanSpeller : ICurrencySpeller
{
    private static readonly string[] Digits = ["영", "일", "이", "삼", "사", "오", "육", "칠", "팔", "구"];

    public LanguageInfo Language { get; } = new("ko-KR", "Korean (South Korea)", "한국어");

    public string Spell(MonetaryAmount amount)
    {
        var dollars = Cardinal(amount.Dollars) + " 달러";
        return amount.Cents == 0 ? dollars : $"{dollars} {Cardinal(amount.Cents)} 센트";
    }

    internal static string Cardinal(int number)
    {
        if (number == 0)
            return Digits[0];

        var (hundredMillions, tenThousands, units) = NumberGroups.SplitMyriads(number);
        var groups = new List<string>(3);
        if (hundredMillions > 0)
            groups.Add(Group(hundredMillions) + "억");
        if (tenThousands > 0)
            groups.Add(tenThousands == 1 ? "만" : Group(tenThousands) + "만");
        if (units > 0)
            groups.Add(Group(units));
        return string.Join(' ', groups);
    }

    private static string Group(int value)
    {
        var (thousands, hundreds, tens, ones) = NumberGroups.Digits(value);
        return string.Concat(
            Place(thousands, "천"),
            Place(hundreds, "백"),
            Place(tens, "십"),
            ones > 0 ? Digits[ones] : string.Empty);
    }

    private static string Place(int digit, string unit) => digit switch
    {
        0 => string.Empty,
        1 => unit,
        _ => Digits[digit] + unit,
    };
}
