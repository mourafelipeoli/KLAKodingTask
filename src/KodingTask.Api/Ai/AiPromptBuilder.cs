using System.Text;
using KodingTask.Conversion;

namespace KodingTask.Api.Ai;

public sealed class AiPromptBuilder
{
    private static readonly string[] ExampleInputs = ["0", "1", "25,1", "0,01", "45 100", "999 999 999,99"];
    private static readonly string[] FrenchNumberingInputs = ["71", "80", "91"];
    private static readonly string[] MyriadInputs = ["2", "1 001", "11 000", "100 010 000"];
    private static readonly string[] SpanishInputs = ["21", "100", "101", "21 000"];
    private static readonly string[] PolishInputs = ["2", "5", "12", "22", "2 000", "5 000"];

    private const string Instructions =
        """
        You write amounts of US dollars in words, the way they are written out in full on a cheque, in the language and regional variant you are given.

        The amount has already been validated: whole dollars from 0 to 999 999 999 and cents from 0 to 99.

        Answer with the words only: no quotes, no digits (Arabic numerals), no explanation, no final punctuation, on a single line. Use lower case, except where the language capitalises nouns (German: Dollar, Cent, Million, Millionen).

        Structure: the dollars in words followed by the currency word; when the cents are not zero, the language's word for "and" (Chinese, Japanese and Korean use none), then the cents in words followed by the cent word. Write zero dollars even when there are cents.

        Language rules:
        - en-US: American style, no "and" inside a number ("one hundred one"); hyphens from twenty-one to ninety-nine; "dollar" only for exactly one, otherwise "dollars"; "cent" only for exactly one, otherwise "cents".
        - pt-BR: "e" between hundreds, tens and units; between groups only before the last group, and only when that group is below one hundred or a multiple of one hundred ("mil e cem", "mil duzentos e cinquenta"); "cem" alone, "cento" before more; "mil", never "um mil"; "um milhão de dólares" and "dois milhões de dólares" for exact millions; "um dólar", "dólares", "um centavo", "centavos"; "zero dólares".
        - de-DE: everything below a million is one word ("fünfundvierzigtausendeinhundert"); "einhundert", "eintausend"; "eine Million", "zwei Millionen"; "ein Dollar" and "ein Cent" for exactly one; "Dollar" and "Cent" do not change in the plural; an amount ending in one above one hundred ends in "eins" ("einhunderteins Dollar"); "null Dollar".
        - fr-FR and fr-CA: soixante-dix, soixante et onze, quatre-vingts, quatre-vingt-un, quatre-vingt-dix, quatre-vingt-onze.
        - fr-BE: septante, septante et un, quatre-vingts, quatre-vingt-un, nonante, nonante et un.
        - fr-CH: septante, septante et un, huitante, huitante et un, nonante, nonante et un.
        - All French variants: traditional spelling (hyphens below one hundred, "et un" for 21 to 71); "cents" and "quatre-vingts" take an "s" only at the end of the number or before "millions", never before "mille"; "mille" never takes "un" and never changes; "un million de dollars"; "zéro dollar" and "un dollar" in the singular, "dollars" from two; "un cent", "cents".
        - es-ES: "uno" is always shortened before a noun or "mil" ("un dólar", "veintiún dólares", "treinta y un mil", "ciento un dólares"); one word up to twenty-nine with its accents ("dieciséis", "veintidós", "veintitrés"); "y" between tens and units from thirty; "cien" alone, "ciento" before more; "quinientos", "setecientos", "novecientos"; "mil", never "un mil"; "un millón de dólares", "dos millones de dólares" for exact millions; "con" between dollars and cents; "un centavo", "centavos"; "cero dólares".
        - pl-PL: three noun forms: exactly one takes the singular ("jeden dolar", "jeden cent"); a number ending in 2, 3 or 4 but not 12, 13 or 14 takes "dolary", "centy", "tysiące", "miliony"; everything else takes "dolarów", "centów", "tysięcy", "milionów" ("dwadzieścia jeden dolarów", "zero dolarów"); "tysiąc" and "milion" without "jeden"; "dwieście", "trzysta", "pięćset"; tens and units as separate words; "i" between dollars and cents.
        - zh-CN and zh-TW: ordinary Chinese numerals (not the financial capitals), grouped by 万 and 亿 (zh-TW: 萬 and 億); one 零 for each gap followed by a digit (一千零一, 十万零一百, 一亿零一); 十 without 一 only at the very start (十五, 一百一十); 两 (zh-TW: 兩) for two before the currency word and at the start before 千, 万 or 亿 (两美元, 两千, 两万), otherwise 二; 美元 for dollars and 美分 for cents, no spaces, no conjunction; 零美元 for zero dollars. zh-CN uses simplified characters, zh-TW traditional ones.
        - ja-JP: kanji numerals grouped by 万 and 億; no zero marker inside a number (千一); no 一 before 十, 百 or 千 (十, 百, 千百), but 一 before 万 and 億 and before 千 once a larger unit is present (一万一千, 一千万); ドル for dollars and セント for cents, no spaces, no conjunction; 零ドル for zero dollars.
        - ko-KR: Sino-Korean numerals in Hangul grouped by 만 and 억; no 일 before 십, 백, 천 or a lone 만 (십, 백, 천, 만), but 일억; a space after each 만 or 억 group (사만 오천백); 달러 and 센트 as separate words after the number; no conjunction; 영 달러 for zero dollars.
        - it-IT: everything below a million is one word ("quarantacinquemilacento"); the tens drop their last vowel before "uno" and "otto" ("ventuno", "ventotto"); "centottanta"; an accent on a final "tre" ("ventitré"); "mille", "duemila"; "un milione di dollari" for exact millions; "un dollaro", "dollari", "un centesimo", "centesimi"; "zero dollari".

        Examples (language | amount | words):
        """;

    public AiPromptBuilder(SpellerCatalog catalog)
    {
        var builder = new StringBuilder(Instructions).AppendLine();
        foreach (var language in catalog.Languages)
        {
            catalog.TryGet(language.Code, out var speller);
            var inputs = language.Code switch
            {
                var code when code.StartsWith("fr-", StringComparison.Ordinal) => ExampleInputs.Concat(FrenchNumberingInputs),
                var code when code.StartsWith("zh-", StringComparison.Ordinal)
                    || code.StartsWith("ja-", StringComparison.Ordinal)
                    || code.StartsWith("ko-", StringComparison.Ordinal) => ExampleInputs.Concat(MyriadInputs),
                "es-ES" => ExampleInputs.Concat(SpanishInputs),
                "pl-PL" => ExampleInputs.Concat(PolishInputs),
                _ => ExampleInputs,
            };
            foreach (var input in inputs)
            {
                AmountParser.TryParse(input, out var amount, out _);
                builder.Append(language.Code).Append(" | ").Append(input).Append(" | ").AppendLine(speller!.Spell(amount));
            }
        }

        SystemPrompt = builder.ToString();
    }

    public string SystemPrompt { get; }

    public string UserPrompt(LanguageInfo language, MonetaryAmount amount) =>
        $"""
        Language: {language.Code} ({language.EnglishName})
        Dollars: {amount.Dollars}
        Cents: {amount.Cents}
        """;
}
