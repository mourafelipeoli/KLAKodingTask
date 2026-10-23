# FrenchSpeller (fr-FR, fr-CA, fr-CH, fr-BE)

One class for the four French variants; `FrenchNumbering` decides 70, 80 and 90.

| Amount | fr-FR / fr-CA | fr-BE | fr-CH |
|---|---|---|---|
| 71 | soixante et onze dollars | septante et un dollars | septante et un dollars |
| 80 | quatre-vingts dollars | quatre-vingts dollars | huitante dollars |
| 81 | quatre-vingt-un dollars | quatre-vingt-un dollars | huitante et un dollars |
| 99 | quatre-vingt-dix-neuf dollars | nonante-neuf dollars | nonante-neuf dollars |

Shared examples: "un dollar", "zéro dollar et un cent", "vingt-cinq dollars et dix cents",
"quarante-cinq mille cent dollars", "un million de dollars".

## Rules

- **Traditional spelling** (before the 1990 reform): hyphens between tens and units below
  one hundred ("vingt-cinq"), "et" for 21, 31 … 71 ("vingt et un", "soixante et onze"), and
  spaces around "cent", "mille", "million". The 1990 spelling ("vingt-et-un",
  "deux-cents") is also correct; the traditional one is still the norm on cheques and in
  legal texts in all four regions.
- **No "et" in 81 and 91 of the standard numbering**: "quatre-vingt-un", "quatre-vingt-onze".
- **The plural "s" of "cent" and "quatre-vingts"** appears only when the word ends the
  numeral: "deux cents dollars", "quatre-vingts dollars", but "deux cent un", "deux cent
  mille", "quatre-vingt mille" ("mille" is an adjective and does not count as an end).
  "Million" is a noun, so the "s" stays before it: "deux cents millions". The
  `pluralEndings` flag carries that decision down to the group being spelled.
- **"mille" is invariable and never preceded by "un"**: "mille", "deux mille".
- **"million" is a noun**: "un million", "deux millions", and "de" before the currency when
  the amount is an exact number of millions: "deux millions de dollars".
- **Zero and one take the singular**: "zéro dollar", "un dollar", "un cent". French uses the
  plural from two upwards.
- **The cent is "cent"**: that is what francophone Canada calls a hundredth of a dollar, and
  it is the usual French name for the US cent. "centime" is reserved for the euro and the
  Swiss franc.

## Why variants and not a parameter on the UI

The variant changes the output, so it is part of the language code (`fr-BE`) rather than a
UI option. A Belgian user who picks "Français (Belgique)" gets *nonante* without hunting
for a setting.
