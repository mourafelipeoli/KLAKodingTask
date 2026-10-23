# AiConversionResponse

The answer of `POST /api/conversions/ai`: always the deterministic result, plus either the
AI's words or the reason the AI did not answer.

A refusal by the AI side (`AiDailyLimitReached`, `AiPaused`, `AiUnavailable`, …) is part of
a 200 response, not an HTTP error: the visitor still asked a valid question and still gets
the correct words. Only an invalid amount is a 400, exactly as on the deterministic
endpoint.
