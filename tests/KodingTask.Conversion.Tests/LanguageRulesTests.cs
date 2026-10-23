using static KodingTask.Conversion.Tests.OfficialExamplesTests;

namespace KodingTask.Conversion.Tests;

public sealed class LanguageRulesTests
{
    [Theory]
    [InlineData("12", "twelve dollars")]
    [InlineData("80", "eighty dollars")]
    [InlineData("101", "one hundred one dollars")]
    [InlineData("100 000", "one hundred thousand dollars")]
    [InlineData("1 000 000", "one million dollars")]
    [InlineData("1 001 001,01", "one million one thousand one dollars and one cent")]
    [InlineData("2,5", "two dollars and fifty cents")]
    public void English(string input, string expected) => AssertWords(input, "en-US", expected);

    [Theory]
    [InlineData("14", "catorze dólares")]
    [InlineData("100", "cem dólares")]
    [InlineData("101", "cento e um dólares")]
    [InlineData("1 001", "mil e um dólares")]
    [InlineData("1 250", "mil duzentos e cinquenta dólares")]
    [InlineData("1 000 000", "um milhão de dólares")]
    [InlineData("2 000 000", "dois milhões de dólares")]
    [InlineData("1 000 100", "um milhão e cem dólares")]
    [InlineData("1 001 000", "um milhão e mil dólares")]
    [InlineData("1 100 000", "um milhão e cem mil dólares")]
    [InlineData("2 500 000", "dois milhões e quinhentos mil dólares")]
    [InlineData("1 234 567", "um milhão duzentos e trinta e quatro mil quinhentos e sessenta e sete dólares")]
    public void Portuguese(string input, string expected) => AssertWords(input, "pt-BR", expected);

    [Theory]
    [InlineData("16", "sechzehn Dollar")]
    [InlineData("17", "siebzehn Dollar")]
    [InlineData("21", "einundzwanzig Dollar")]
    [InlineData("30", "dreißig Dollar")]
    [InlineData("101", "einhunderteins Dollar")]
    [InlineData("1 000", "eintausend Dollar")]
    [InlineData("100 000", "einhunderttausend Dollar")]
    [InlineData("101 000", "einhunderteintausend Dollar")]
    [InlineData("1 000 000", "eine Million Dollar")]
    [InlineData("2 000 001", "zwei Millionen eins Dollar")]
    [InlineData("101 000 000", "einhunderteine Millionen Dollar")]
    [InlineData("0,21", "null Dollar und einundzwanzig Cent")]
    public void German(string input, string expected) => AssertWords(input, "de-DE", expected);

    [Theory]
    [InlineData("21", "vingt et un dollars")]
    [InlineData("70", "soixante-dix dollars")]
    [InlineData("71", "soixante et onze dollars")]
    [InlineData("77", "soixante-dix-sept dollars")]
    [InlineData("80", "quatre-vingts dollars")]
    [InlineData("81", "quatre-vingt-un dollars")]
    [InlineData("90", "quatre-vingt-dix dollars")]
    [InlineData("91", "quatre-vingt-onze dollars")]
    [InlineData("200", "deux cents dollars")]
    [InlineData("201", "deux cent un dollars")]
    [InlineData("1 000", "mille dollars")]
    [InlineData("21 000", "vingt et un mille dollars")]
    [InlineData("80 000", "quatre-vingt mille dollars")]
    [InlineData("200 000", "deux cent mille dollars")]
    [InlineData("1 000 000", "un million de dollars")]
    [InlineData("80 000 000", "quatre-vingts millions de dollars")]
    [InlineData("200 000 000", "deux cents millions de dollars")]
    [InlineData("0,8", "zéro dollar et quatre-vingts cents")]
    public void French_France_and_Canada(string input, string expected)
    {
        AssertWords(input, "fr-FR", expected);
        AssertWords(input, "fr-CA", expected);
    }

    [Theory]
    [InlineData("70", "septante dollars")]
    [InlineData("71", "septante et un dollars")]
    [InlineData("80", "quatre-vingts dollars")]
    [InlineData("81", "quatre-vingt-un dollars")]
    [InlineData("90", "nonante dollars")]
    [InlineData("91", "nonante et un dollars")]
    [InlineData("99", "nonante-neuf dollars")]
    public void French_Belgium(string input, string expected) => AssertWords(input, "fr-BE", expected);

    [Theory]
    [InlineData("70", "septante dollars")]
    [InlineData("80", "huitante dollars")]
    [InlineData("81", "huitante et un dollars")]
    [InlineData("88", "huitante-huit dollars")]
    [InlineData("90", "nonante dollars")]
    [InlineData("80 000", "huitante mille dollars")]
    public void French_Switzerland(string input, string expected) => AssertWords(input, "fr-CH", expected);

    [Theory]
    [InlineData("3", "tre dollari")]
    [InlineData("21", "ventuno dollari")]
    [InlineData("23", "ventitré dollari")]
    [InlineData("28", "ventotto dollari")]
    [InlineData("101", "centouno dollari")]
    [InlineData("108", "centootto dollari")]
    [InlineData("180", "centottanta dollari")]
    [InlineData("288", "duecentottantotto dollari")]
    [InlineData("1 000", "mille dollari")]
    [InlineData("1 003", "milletré dollari")]
    [InlineData("2 000", "duemila dollari")]
    [InlineData("23 000", "ventitremila dollari")]
    [InlineData("1 000 000", "un milione di dollari")]
    [InlineData("23 000 000", "ventitré milioni di dollari")]
    [InlineData("0,23", "zero dollari e ventitré centesimi")]
    public void Italian(string input, string expected) => AssertWords(input, "it-IT", expected);
}
