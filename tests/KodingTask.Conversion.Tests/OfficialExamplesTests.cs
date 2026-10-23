namespace KodingTask.Conversion.Tests;

public sealed class OfficialExamplesTests
{
    private static readonly CurrencyConverter Converter = new(SpellerCatalog.Default);

    [Theory]
    [InlineData("0", "zero dollars")]
    [InlineData("1", "one dollar")]
    [InlineData("25,1", "twenty-five dollars and ten cents")]
    [InlineData("0,01", "zero dollars and one cent")]
    [InlineData("45 100", "forty-five thousand one hundred dollars")]
    [InlineData("999 999 999,99", "nine hundred ninety-nine million nine hundred ninety-nine thousand nine hundred ninety-nine dollars and ninety-nine cents")]
    public void English_matches_the_task_statement(string input, string expected) => AssertWords(input, "en-US", expected);

    [Theory]
    [InlineData("0", "zero dólares")]
    [InlineData("1", "um dólar")]
    [InlineData("25,1", "vinte e cinco dólares e dez centavos")]
    [InlineData("0,01", "zero dólares e um centavo")]
    [InlineData("45 100", "quarenta e cinco mil e cem dólares")]
    [InlineData("999 999 999,99", "novecentos e noventa e nove milhões novecentos e noventa e nove mil novecentos e noventa e nove dólares e noventa e nove centavos")]
    public void Portuguese(string input, string expected) => AssertWords(input, "pt-BR", expected);

    [Theory]
    [InlineData("0", "null Dollar")]
    [InlineData("1", "ein Dollar")]
    [InlineData("25,1", "fünfundzwanzig Dollar und zehn Cent")]
    [InlineData("0,01", "null Dollar und ein Cent")]
    [InlineData("45 100", "fünfundvierzigtausendeinhundert Dollar")]
    [InlineData("999 999 999,99", "neunhundertneunundneunzig Millionen neunhundertneunundneunzigtausendneunhundertneunundneunzig Dollar und neunundneunzig Cent")]
    public void German(string input, string expected) => AssertWords(input, "de-DE", expected);

    [Theory]
    [InlineData("fr-FR")]
    [InlineData("fr-CA")]
    public void French_with_standard_numbering(string language)
    {
        AssertWords("0", language, "zéro dollar");
        AssertWords("1", language, "un dollar");
        AssertWords("25,1", language, "vingt-cinq dollars et dix cents");
        AssertWords("0,01", language, "zéro dollar et un cent");
        AssertWords("45 100", language, "quarante-cinq mille cent dollars");
        AssertWords("999 999 999,99", language, "neuf cent quatre-vingt-dix-neuf millions neuf cent quatre-vingt-dix-neuf mille neuf cent quatre-vingt-dix-neuf dollars et quatre-vingt-dix-neuf cents");
    }

    [Theory]
    [InlineData("fr-BE")]
    [InlineData("fr-CH")]
    public void French_with_nonante(string language)
    {
        AssertWords("0", language, "zéro dollar");
        AssertWords("1", language, "un dollar");
        AssertWords("25,1", language, "vingt-cinq dollars et dix cents");
        AssertWords("0,01", language, "zéro dollar et un cent");
        AssertWords("45 100", language, "quarante-cinq mille cent dollars");
        AssertWords("999 999 999,99", language, "neuf cent nonante-neuf millions neuf cent nonante-neuf mille neuf cent nonante-neuf dollars et nonante-neuf cents");
    }

    [Theory]
    [InlineData("0", "zero dollari")]
    [InlineData("1", "un dollaro")]
    [InlineData("25,1", "venticinque dollari e dieci centesimi")]
    [InlineData("0,01", "zero dollari e un centesimo")]
    [InlineData("45 100", "quarantacinquemilacento dollari")]
    [InlineData("999 999 999,99", "novecentonovantanove milioni novecentonovantanovemilanovecentonovantanove dollari e novantanove centesimi")]
    public void Italian(string input, string expected) => AssertWords(input, "it-IT", expected);

    [Theory]
    [InlineData("0", "cero dólares")]
    [InlineData("1", "un dólar")]
    [InlineData("25,1", "veinticinco dólares con diez centavos")]
    [InlineData("0,01", "cero dólares con un centavo")]
    [InlineData("45 100", "cuarenta y cinco mil cien dólares")]
    [InlineData("999 999 999,99", "novecientos noventa y nueve millones novecientos noventa y nueve mil novecientos noventa y nueve dólares con noventa y nueve centavos")]
    public void Spanish(string input, string expected) => AssertWords(input, "es-ES", expected);

    [Theory]
    [InlineData("0", "zero dolarów")]
    [InlineData("1", "jeden dolar")]
    [InlineData("25,1", "dwadzieścia pięć dolarów i dziesięć centów")]
    [InlineData("0,01", "zero dolarów i jeden cent")]
    [InlineData("45 100", "czterdzieści pięć tysięcy sto dolarów")]
    [InlineData("999 999 999,99", "dziewięćset dziewięćdziesiąt dziewięć milionów dziewięćset dziewięćdziesiąt dziewięć tysięcy dziewięćset dziewięćdziesiąt dziewięć dolarów i dziewięćdziesiąt dziewięć centów")]
    public void Polish(string input, string expected) => AssertWords(input, "pl-PL", expected);

    [Theory]
    [InlineData("0", "零美元")]
    [InlineData("1", "一美元")]
    [InlineData("25,1", "二十五美元十美分")]
    [InlineData("0,01", "零美元一美分")]
    [InlineData("45 100", "四万五千一百美元")]
    [InlineData("999 999 999,99", "九亿九千九百九十九万九千九百九十九美元九十九美分")]
    public void Chinese_simplified(string input, string expected) => AssertWords(input, "zh-CN", expected);

    [Theory]
    [InlineData("0", "零美元")]
    [InlineData("1", "一美元")]
    [InlineData("25,1", "二十五美元十美分")]
    [InlineData("0,01", "零美元一美分")]
    [InlineData("45 100", "四萬五千一百美元")]
    [InlineData("999 999 999,99", "九億九千九百九十九萬九千九百九十九美元九十九美分")]
    public void Chinese_traditional(string input, string expected) => AssertWords(input, "zh-TW", expected);

    [Theory]
    [InlineData("0", "零ドル")]
    [InlineData("1", "一ドル")]
    [InlineData("25,1", "二十五ドル十セント")]
    [InlineData("0,01", "零ドル一セント")]
    [InlineData("45 100", "四万五千百ドル")]
    [InlineData("999 999 999,99", "九億九千九百九十九万九千九百九十九ドル九十九セント")]
    public void Japanese(string input, string expected) => AssertWords(input, "ja-JP", expected);

    [Theory]
    [InlineData("0", "영 달러")]
    [InlineData("1", "일 달러")]
    [InlineData("25,1", "이십오 달러 십 센트")]
    [InlineData("0,01", "영 달러 일 센트")]
    [InlineData("45 100", "사만 오천백 달러")]
    [InlineData("999 999 999,99", "구억 구천구백구십구만 구천구백구십구 달러 구십구 센트")]
    public void Korean(string input, string expected) => AssertWords(input, "ko-KR", expected);

    internal static void AssertWords(string input, string language, string expected)
    {
        var result = Converter.Convert(input, language);
        Assert.True(result.IsSuccess, $"{input} in {language} failed with {result.Error}");
        Assert.Equal(expected, result.Words);
    }
}
