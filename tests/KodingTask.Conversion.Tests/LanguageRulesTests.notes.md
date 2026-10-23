# LanguageRulesTests

One case per grammatical rule documented in the speller notes: the places where a naive
"number to words" implementation goes wrong in each language.

- English: "and" only between dollars and cents.
- Portuguese: "cem" vs "cento", where "e" joins groups, "de" after an exact million.
- German: the three forms of "one" (eins / ein / eine) and compound spelling.
- French: 70/80/90 per region, "et un", the plural "s" of "cent" and "quatre-vingts" before
  "mille" (no s) and before "millions" (s), singular for zero.
- Italian: elision before "uno" and "otto", "centottanta", the accent on a final "tre".
- Spanish: the shortened "un" (veintiún, treinta y un, ciento un, veintiún mil), "cien" vs
  "ciento", the irregular hundreds, "de" after an exact million.
- Polish: the three noun forms (dolar / dolary / dolarów) including 12-14 and 21, the same
  rule for "tysiąc" and "milion", the silent one before them.
- Chinese: one 零 per gap, 十 without 一 only at the start, 两/兩 for two before a measure,
  the 万/亿 and 萬/億 groups.
- Japanese: no zero marker, one silent before 十, 百 and 千 but said before 万 and 億 and
  before 千 once a larger unit appears.
- Korean: one silent before 십, 백, 천 and a lone 만, but 일억; a space after each myriad
  group and around the currency words.

When a rule changes, the speller's note and this test change together.
