# ChineseSpeller (zh-CN, zh-TW)

One class for Simplified (Mainland China) and Traditional (Taiwan) Chinese; `ChineseScript`
picks the characters.

| Amount | zh-CN | zh-TW |
|---|---|---|
| 0 | 零美元 | 零美元 |
| 1 | 一美元 | 一美元 |
| 2 | 两美元 | 兩美元 |
| 25,1 | 二十五美元十美分 | 二十五美元十美分 |
| 0,01 | 零美元一美分 | 零美元一美分 |
| 45 100 | 四万五千一百美元 | 四萬五千一百美元 |
| 100 100 | 十万零一百美元 | 十萬零一百美元 |
| 999 999 999,99 | 九亿九千九百九十九万九千九百九十九美元九十九美分 | 九億九千九百九十九萬九千九百九十九美元九十九美分 |

## Rules

- **Counting in myriads**: 万/萬 (10⁴) and 亿/億 (10⁸) name the groups, so 45 100 is
  四万五千一百 ("four myriads, five thousand one hundred").
- **零 marks a gap, once**: one or more missing places followed by a digit are read as a
  single 零: 一千零一 (1 001), 十万零一百 (100 100), 一亿零一 (100 000 001). Trailing zeros
  are silent: 一千 (1 000), 一亿 (100 000 000).
- **十, not 一十, at the start**: 10 to 19 and 100 000 to 199 999 begin with 十 (十五, 十五万),
  but inside a number the 一 stays: 一百一十 (110), 一千零一十 (1 010).
- **两/兩 for two in front of a measure**: "two dollars" is 两美元, "two cents" 两美分, and a
  number that starts with two thousand, two myriads or two hundred million starts with 两
  (两千, 两万, 两亿). Everywhere else two is 二 (二十二, 二百, 一万二千), which is how
  amounts are read aloud and written in words.
- **No conjunction between dollars and cents**: 二十五美元十美分. "Zero dollars" is
  written (零美元一美分), as the task requires for every language.
- **Currency words**: 美元 (US dollar) and 美分 (US cent), the same characters in both
  scripts. Chinese has no plural, so one and many look alike.
- **Ordinary numerals, not the financial ones**: banks use the anti-fraud capitals
  (壹贰叁…, 圆, 整) on cheques; the task asks for an amount "in words", which in everyday
  Chinese means the ordinary numerals.
