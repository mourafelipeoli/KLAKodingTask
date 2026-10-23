# NumberGroups

Splits a number into the groups the languages name separately.

- `Split`: millions, thousands and the last three digits, for the European languages,
  which count in thousands.
- `SplitMyriads`: hundred-millions, ten-thousands and the last four digits, for Chinese,
  Japanese and Korean, which count in myriads: 万/萬/만 is ten thousand and 亿/億/억 is a
  hundred million, so 45 100 is "4 万 5100" and 999 999 999 is "9 亿 9999 万 9999".
- `Digits`: the four digits of one myriad group (thousands, hundreds, tens, ones).

With a maximum of 999 999 999 there is never a billions group in the first split nor a
group above 亿 in the second.

This is the only arithmetic the languages share. Everything after the split (joining
words, agreement, conjunctions, zero markers) is language-specific and lives in each
speller.
