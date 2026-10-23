using System.Globalization;
using KodingTask.Conversion;

namespace KodingTask.Api.Messaging;

public static class AmountText
{
    public static string Format(MonetaryAmount amount, string locale)
    {
        var culture = CultureFor(locale);
        var value = amount.Dollars + amount.Cents / 100m;
        return $"US$ {value.ToString("N2", culture)}";
    }

    private static CultureInfo CultureFor(string locale)
    {
        try
        {
            return CultureInfo.GetCultureInfo(locale);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }
}
