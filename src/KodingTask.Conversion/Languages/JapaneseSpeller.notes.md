# JapaneseSpeller (ja-JP)

| Amount | Output |
|---|---|
| 0 | 零ドル |
| 1 | 一ドル |
| 25,1 | 二十五ドル十セント |
| 0,01 | 零ドル一セント |
| 1 001 | 千一ドル |
| 11 000 | 一万一千ドル |
| 45 100 | 四万五千百ドル |
| 999 999 999,99 | 九億九千九百九十九万九千九百九十九ドル九十九セント |

## Rules

- **Counting in myriads**: 万 (10⁴) and 億 (10⁸) name the groups, as in Chinese.
- **No zero marker**: missing places are simply skipped, 千一 (1 001), 一万百 (10 100).
  Japanese has no equivalent of the Chinese 零 inside a number.
- **One is silent before 十, 百 and 千**: 十, 百, 千, 千百 (1 100). It is said before 万 and
  億 (一万, 一億), and before 千 once a larger unit is involved: 一万一千 (11 000), 一千万
  (10 000 000), which is how amounts are written in Japanese finance.
- **Kanji numerals**, the form used when an amount is written out in words. 零 is used for
  zero ("zero dollars" is required by the task even when there are cents).
- **Currency words**: ドル (dollar) and セント (cent), written right after the number without
  a space; Japanese has no plural. No conjunction between dollars and cents.
