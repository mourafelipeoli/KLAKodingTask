# MonetaryAmount

The value object behind every conversion: whole dollars and cents, already validated.

- The limits come straight from the task: at most 999 999 999 dollars and 99 cents. The
  constructor enforces them, so a speller never has to defend against an impossible value.
- `int` is enough for both parts (the maximum fits comfortably below `int.MaxValue`), and
  keeping dollars and cents apart avoids any floating-point rounding: `25,1` is exactly
  25 dollars and 10 cents, never 25.0999…
- `default(MonetaryAmount)` is zero dollars and zero cents, which is a valid amount.
- `ToCanonicalString()` writes the amount the way the task writes it (space-grouped
  thousands, comma, two cent digits: `45 100,00`). The API returns it so the client can show
  how its input was understood.
