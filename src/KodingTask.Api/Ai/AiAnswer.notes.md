# AiAnswer

Decides whether a model reply is usable, and whether it matches the deterministic speller.

- **Strict acceptance.** One non-empty line, no digits, at most 600 characters (the longest
  correct answer in any language is about 200). Surrounding quotes, backticks and a final
  period are tolerated and removed. Anything else (an explanation, a list, an apology, a
  refusal) is rejected as unexpected output rather than shown to the visitor.
- **Comparison is lenient only about typography**: case, whitespace, curly vs straight
  apostrophes and non-breaking hyphens or spaces. Different words, a missing hyphen or a
  different plural count as a disagreement, which is exactly what the UI reports.
- **Spaces do not count in Chinese, Japanese and Korean.** Chinese and Japanese write no
  spaces at all, and Korean spacing of numbers in words varies in practice (구천구백구십구만
  is often written without the space the spelling rules ask for after each myriad group).
  For those languages the comparison ignores whitespace; everywhere else a space is part of
  the spelling ("fünfundvierzig tausend" is wrong German).
