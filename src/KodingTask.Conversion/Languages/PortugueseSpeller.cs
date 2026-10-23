namespace KodingTask.Conversion.Languages;

public sealed class PortugueseSpeller : ICurrencySpeller
{
    private static readonly string[] Ones =
    [
        "zero", "um", "dois", "três", "quatro", "cinco", "seis", "sete", "oito", "nove",
        "dez", "onze", "doze", "treze", "catorze", "quinze", "dezesseis", "dezessete", "dezoito", "dezenove",
    ];

    private static readonly string[] Tens =
        ["", "", "vinte", "trinta", "quarenta", "cinquenta", "sessenta", "setenta", "oitenta", "noventa"];

    private static readonly string[] Hundreds =
    [
        "", "cento", "duzentos", "trezentos", "quatrocentos",
        "quinhentos", "seiscentos", "setecentos", "oitocentos", "novecentos",
    ];

    public LanguageInfo Language { get; } = new("pt-BR", "Portuguese (Brazil)", "Português (Brasil)");

    public string Spell(MonetaryAmount amount)
    {
        var dollars = amount.Dollars switch
        {
            1 => "um dólar",
            var exactMillions when exactMillions % 1_000_000 == 0 && exactMillions > 0 => $"{Cardinal(exactMillions)} de dólares",
            var other => $"{Cardinal(other)} dólares",
        };

        if (amount.Cents == 0)
            return dollars;

        var cents = amount.Cents == 1 ? "um centavo" : $"{Cardinal(amount.Cents)} centavos";
        return $"{dollars} e {cents}";
    }

    internal static string Cardinal(int number)
    {
        if (number == 0)
            return Ones[0];

        var (millions, thousands, units) = NumberGroups.Split(number);
        var groups = new List<(string Words, int Value)>(3);
        if (millions > 0)
            groups.Add((millions == 1 ? "um milhão" : $"{UpTo999(millions)} milhões", millions));
        if (thousands > 0)
            groups.Add((thousands == 1 ? "mil" : $"{UpTo999(thousands)} mil", thousands));
        if (units > 0)
            groups.Add((UpTo999(units), units));

        var text = groups[0].Words;
        for (var index = 1; index < groups.Count; index++)
        {
            var (words, value) = groups[index];
            var isLast = index == groups.Count - 1;
            var joinsWithE = isLast && (value < 100 || value % 100 == 0);
            text += (joinsWithE ? " e " : " ") + words;
        }

        return text;
    }

    private static string UpTo999(int number)
    {
        if (number == 100)
            return "cem";

        var hundreds = number / 100;
        var rest = number % 100;
        if (hundreds == 0)
            return UpTo99(rest);
        return rest == 0 ? Hundreds[hundreds] : $"{Hundreds[hundreds]} e {UpTo99(rest)}";
    }

    private static string UpTo99(int number)
    {
        if (number < 20)
            return Ones[number];

        var unit = number % 10;
        return unit == 0 ? Tens[number / 10] : $"{Tens[number / 10]} e {Ones[unit]}";
    }
}
