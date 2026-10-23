# AiConversionOutcome

Either the AI's words (with whether they match the deterministic speller and how long the
model took) or the reason there are none.

`ErrorCode` prefixes the reason with `Ai` (`AiDailyLimitReached`, `AiPaused`, …) so the
codes never collide with conversion errors in the client's message table.
