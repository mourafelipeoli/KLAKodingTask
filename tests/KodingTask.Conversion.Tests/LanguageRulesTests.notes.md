# LanguageRulesTests

One case per grammatical rule documented in the speller notes: the places where a naive
"number to words" implementation goes wrong in each language.

- English: "and" only between dollars and cents.
- Portuguese: "cem" vs "cento", where "e" joins groups, "de" after an exact million.
- German: the three forms of "one" (eins / ein / eine) and compound spelling.
- French: 70/80/90 per region, "et un", the plural "s" of "cent" and "quatre-vingts" before
  "mille" (no s) and before "millions" (s), singular for zero.
- Italian: elision before "uno" and "otto", "centottanta", the accent on a final "tre".

When a rule changes, the speller's note and this test change together.
