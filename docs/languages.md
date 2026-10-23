# Languages

Eight output languages, five spellers. The code of each speller has a note with the full
rule set; this page is the overview.

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

## Where the French variants differ

| | 70 | 71 | 80 | 81 | 90 | 99 |
|---|---|---|---|---|---|---|
| fr-FR, fr-CA | soixante-dix | soixante et onze | quatre-vingts | quatre-vingt-un | quatre-vingt-dix | quatre-vingt-dix-neuf |
| fr-BE | septante | septante et un | quatre-vingts | quatre-vingt-un | nonante | nonante-neuf |
| fr-CH | septante | septante et un | huitante | huitante et un | nonante | nonante-neuf |

Switzerland is not uniform: *huitante* is used in Vaud, Valais and Fribourg, while Geneva
and Neuchâtel mostly say *quatre-vingts*. `fr-CH` follows the cantons with the distinct
Swiss form.

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

All of these have a test in
[`LanguageRulesTests`](../tests/KodingTask.Conversion.Tests/LanguageRulesTests.cs).

## Interface translations

The interface texts live in `src/kodingtask-web/src/i18n/dictionaries/`. The English
sentence is the key. `pt-BR`, `de-DE`, `fr-FR` and `it-IT` translate every key; `fr-CA`,
`fr-CH` and `fr-BE` only hold what differs from France (Canada writes "courriel" for
e-mail) and fall back to `fr-FR` for the rest. `npm run check:i18n` verifies completeness,
placeholders and that every key in use exists.
