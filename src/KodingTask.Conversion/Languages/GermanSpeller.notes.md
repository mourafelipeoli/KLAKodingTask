# GermanSpeller (de-DE)

Written the way amounts are written out on German cheques and invoices ("in Worten").

| Amount | Output |
|---|---|
| 1 | ein Dollar |
| 25,1 | fünfundzwanzig Dollar und zehn Cent |
| 0,01 | null Dollar und ein Cent |
| 45 100 | fünfundvierzigtausendeinhundert Dollar |
| 999 999 999,99 | neunhundertneunundneunzig Millionen neunhundertneunundneunzigtausendneunhundertneunundneunzig Dollar und neunundneunzig Cent |

## Rules

- **Everything below a million is one word**: "fünfundvierzigtausendeinhundert".
  Millions are separate words with a capital letter: "eine Million", "zwei Millionen".
- **Units before tens, joined by "und"**: 25 is "fünfundzwanzig".
- **"einhundert", "eintausend"** rather than the shorter "hundert", "tausend": the long form
  is the cheque convention, because nobody can squeeze a digit in front of it.
- **Currency nouns do not inflect after numbers**: "zehn Dollar", "zehn Cent".

## The three forms of "one"

German "one" changes with what follows it, so the cardinal tracks where a final 1 sits:

- **Compound** "ein" inside a word: "einundzwanzig", "einhunderteintausend".
- **Feminine** "eine" before "Million", a feminine noun: "eine Million",
  "einhunderteine Millionen".
- **Standalone** "eins" at the end of the number: "einhunderteins".

Exactly one dollar or one cent is the article form "ein Dollar", "ein Cent". For larger
amounts ending in one (101, 1 001, …) the numeral keeps its uninflected written form,
"einhunderteins Dollar", as the "in Worten" line of a cheque would. Duden also accepts the
inflected "hundertundein Dollar"; the uninflected form was chosen because it is the one
used when amounts are written out formally.
