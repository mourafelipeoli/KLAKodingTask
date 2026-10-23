namespace KodingTask.Conversion.Languages;

public sealed class SpanishSpeller : ICurrencySpeller
{
    // Every number in an amount is followed by a noun ("dólares", "mil", "millones"), so
    // "uno" always takes its shortened form: un, veintiún, treinta y un, ciento un.
    private static readonly string[] UpToTwentyNine =
    [
        "cero", "un", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve",
        "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve",
        "veinte", "veintiún", "veintidós", "veintitrés", "veinticuatro", "veinticinco", "veintiséis", "veintisiete", "veintiocho", "veintinueve",
    ];

    private static readonly string[] Tens =
        ["", "", "", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa"];

    private static readonly string[] Hundreds =
        ["", "ciento", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos"];

    public LanguageInfo Language { get; } = new("es-ES", "Spanish (Spain)", "Español (España)");

    public string Spell(MonetaryAmount amount)
    {
        var dollars = amount.Dollars switch
        {
            1 => "un dólar",
            var exactMillions when exactMillions % 1_000_000 == 0 && exactMillions > 0 => $"{Cardinal(exactMillions)} de dólares",
            var other => $"{Cardinal(other)} dólares",
        };

        if (amount.Cents == 0)
            return dollars;

        var cents = amount.Cents == 1 ? "un centavo" : $"{Cardinal(amount.Cents)} centavos";
        return $"{dollars} con {cents}";
    }

    internal static string Cardinal(int number)
    {
        if (number == 0)
            return UpToTwentyNine[0];

        var (millions, thousands, units) = NumberGroups.Split(number);
        var words = new List<string>(3);
        if (millions > 0)
            words.Add(millions == 1 ? "un millón" : $"{UpTo999(millions)} millones");
        if (thousands > 0)
            words.Add(thousands == 1 ? "mil" : $"{UpTo999(thousands)} mil");
        if (units > 0)
            words.Add(UpTo999(units));
        return string.Join(' ', words);
    }

    private static string UpTo999(int number)
    {
        if (number == 100)
            return "cien";

        var hundreds = number / 100;
        var rest = number % 100;
        if (hundreds == 0)
            return UpTo99(rest);
        return rest == 0 ? Hundreds[hundreds] : $"{Hundreds[hundreds]} {UpTo99(rest)}";
    }

    private static string UpTo99(int number)
    {
        if (number < 30)
            return UpToTwentyNine[number];

        var unit = number % 10;
        return unit == 0 ? Tens[number / 10] : $"{Tens[number / 10]} y {UpToTwentyNine[unit]}";
    }
}
