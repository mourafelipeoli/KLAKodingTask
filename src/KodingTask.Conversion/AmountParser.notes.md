# AmountParser

Turns the text a person typed into a `MonetaryAmount`, or into the precise reason it can't.

## The format

Taken from the task statement and its examples (`0`, `1`, `25,1`, `0,01`, `45 100`,
`999 999 999,99`):

- digits, optionally grouped in thousands with spaces;
- optionally a comma followed by one or two digits of cents.

## Decisions and assumptions

- **One cent digit is a tenth, not a cent.** `25,1` is twenty-five dollars and *ten* cents,
  exactly as the task's example says. `25,01` is one cent.
- **Grouping is checked.** When spaces are present, the first group has one to three digits
  and every following group exactly three. `45 100` and `1 000 000` pass; `4 5100` fails
  with `InvalidGrouping`, because there is no reading of it that is clearly right.
- **Accepted spaces:** the normal space, the no-break space (U+00A0), the narrow no-break
  space (U+202F, what French and Swiss number formatting produces) and the thin space
  (U+2009). Pasting a number formatted by the operating system just works.
- **Only ASCII digits.** `char.IsDigit` would also accept Arabic-Indic or full-width digits;
  `char.IsAsciiDigit` does not, so `int.Parse` never sees anything unexpected.
- **Leading zeros are fine** (`007` is seven dollars); the length check runs after they are
  removed, so `000 000 001 000` (twelve digits) is one thousand, not an out-of-range value.
- **A point is refused with its own error** (`DecimalPointNotAllowed`). Accepting it as a
  decimal separator would make `1.000` ambiguous (one thousand in German, one in English).
- **Surrounding whitespace is trimmed;** `string.Trim()` covers the no-break spaces too.
- **Input longer than 40 characters is refused** before any work. The longest valid amount
  is 14 characters; the margin allows generous spacing without letting a client send a
  megabyte of digits.

## Why a hand-written parser

`decimal.Parse` with a custom `NumberFormatInfo` accepts too much (exponents, signs,
currency symbols, inconsistent grouping) and reports a single generic failure. The task is
small enough that an explicit parser is shorter than the configuration needed to restrain
the framework one, and every refusal maps to a message the user can act on.
