# Languages

Twelve output languages, eight spellers, five scripts. The code of each speller has a note
with the full rule set; this page is the overview.

| Code | Speller | 1 | 25,1 | 45 100 |
|---|---|---|---|---|
| en-US | [English](../src/KodingTask.Conversion/Languages/EnglishSpeller.notes.md) | one dollar | twenty-five dollars and ten cents | forty-five thousand one hundred dollars |
| pt-BR | [Portuguese](../src/KodingTask.Conversion/Languages/PortugueseSpeller.notes.md) | um dólar | vinte e cinco dólares e dez centavos | quarenta e cinco mil e cem dólares |
| de-DE | [German](../src/KodingTask.Conversion/Languages/GermanSpeller.notes.md) | ein Dollar | fünfundzwanzig Dollar und zehn Cent | fünfundvierzigtausendeinhundert Dollar |
| fr-FR | [French](../src/KodingTask.Conversion/Languages/FrenchSpeller.notes.md) | un dollar | vingt-cinq dollars et dix cents | quarante-cinq mille cent dollars |
| fr-CA | French, standard numbering | un dollar | vingt-cinq dollars et dix cents | quarante-cinq mille cent dollars |
| fr-CH | French, Swiss numbering | un dollar | vingt-cinq dollars et dix cents | quarante-cinq mille cent dollars |
| fr-BE | French, Belgian numbering | un dollar | vingt-cinq dollars et dix cents | quarante-cinq mille cent dollars |
| it-IT | [Italian](../src/KodingTask.Conversion/Languages/ItalianSpeller.notes.md) | un dollaro | venticinque dollari e dieci centesimi | quarantacinquemilacento dollari |
| zh-CN | [Chinese](../src/KodingTask.Conversion/Languages/ChineseSpeller.notes.md), simplified | 一美元 | 二十五美元十美分 | 四万五千一百美元 |
| zh-TW | Chinese, traditional | 一美元 | 二十五美元十美分 | 四萬五千一百美元 |
| ja-JP | [Japanese](../src/KodingTask.Conversion/Languages/JapaneseSpeller.notes.md) | 一ドル | 二十五ドル十セント | 四万五千百ドル |
| ko-KR | [Korean](../src/KodingTask.Conversion/Languages/KoreanSpeller.notes.md) | 일 달러 | 이십오 달러 십 센트 | 사만 오천백 달러 |

## Where the French variants differ

| | 70 | 71 | 80 | 81 | 90 | 99 |
|---|---|---|---|---|---|---|
| fr-FR, fr-CA | soixante-dix | soixante et onze | quatre-vingts | quatre-vingt-un | quatre-vingt-dix | quatre-vingt-dix-neuf |
| fr-BE | septante | septante et un | quatre-vingts | quatre-vingt-un | nonante | nonante-neuf |
| fr-CH | septante | septante et un | huitante | huitante et un | nonante | nonante-neuf |

Switzerland is not uniform: *huitante* is used in Vaud, Valais and Fribourg, while Geneva
and Neuchâtel mostly say *quatre-vingts*. `fr-CH` follows the cantons with the distinct
Swiss form.

## Where Chinese, Japanese and Korean differ

All three count in myriads: 万 (zh-CN, ja-JP), 萬 (zh-TW) or 만 (ko-KR) is ten thousand, and
亿, 億 or 억 a hundred million.

| Amount | zh-CN | ja-JP | ko-KR |
|---|---|---|---|
| 2 | 两美元 | 二ドル | 이 달러 |
| 10 | 十美元 | 十ドル | 십 달러 |
| 1 001 | 一千零一美元 | 千一ドル | 천일 달러 |
| 11 000 | 一万一千美元 | 一万一千ドル | 만 천 달러 |
| 100 100 | 十万零一百美元 | 十万百ドル | 십만 백 달러 |
| 100 000 001 | 一亿零一美元 | 一億一ドル | 일억 일 달러 |

## Rules that are easy to get wrong

- **English**: no "and" inside the number (American style); hyphens for 21-99.
- **Portuguese**: "e" joins hundreds, tens and units, and joins the last group to the rest
  only when that group is below one hundred or a round hundred ("mil e cem", but "mil
  duzentos e cinquenta"); "cem" vs "cento"; "de" after an exact million.
- **German**: everything below a million is one word; "eins", "ein" or "eine" depending on
  what follows; "einhundert" and "eintausend" as on cheques.
- **French**: "et un" for 21-71 but not 81 and 91 in standard numbering; the plural "s" of
  "cents" and "quatre-vingts" only at the end of the numeral and before "millions", never
  before "mille"; singular for zero and one.
- **Italian**: one word below a million; elision before "uno" and "otto" ("ventuno",
  "ventotto"); "centottanta"; accent on a final "tre" ("ventitré").
- **Chinese**: one 零 for each gap followed by a digit; 十 without 一 only at the start;
  两/兩 for two before the currency word and before a leading 千, 万 or 亿; ordinary
  numerals, not the financial capitals used on bank cheques.
- **Japanese**: no zero inside a number; 一 is silent before 十, 百 and 千 but said before 万
  and 億, and before 千 once a larger unit is present (一万一千).
- **Korean**: Sino-Korean numerals in Hangul; 일 is silent before 십, 백, 천 and a lone 만,
  but not before 억; a space after each myriad group and around the currency words.

All of these have a test in
[`LanguageRulesTests`](../tests/KodingTask.Conversion.Tests/LanguageRulesTests.cs).

## Interface translations

The interface texts live in `src/kodingtask-web/src/i18n/dictionaries/`. The English
sentence is the key. `pt-BR`, `de-DE`, `fr-FR`, `it-IT`, `zh-CN`, `zh-TW`, `ja-JP` and
`ko-KR` translate every key; `fr-CA`,
`fr-CH` and `fr-BE` only hold what differs from France (Canada writes "courriel" for
e-mail) and fall back to `fr-FR` for the rest. `npm run check:i18n` verifies completeness,
placeholders and that every key in use exists.

A browser asking for Chinese from Taiwan, Hong Kong or Macau, or for `zh-Hant`, gets
traditional characters; any other Chinese gets simplified ones.
