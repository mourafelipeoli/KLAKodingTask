# AiConversionsController

`POST /api/conversions/ai` with the same body as `POST /api/conversions`.

The amount is parsed and converted deterministically first; an invalid amount is a 400
before any AI guard or model call runs. Then `AiConversionService` decides whether the AI
may answer and, if so, asks it. The response always contains the deterministic result
(see `AiConversionResponse`).

It has its own, stricter rate-limit policy (`RequestRateLimits.Ai`, ten per minute per
client by default) on top of the daily and monthly limits, because every accepted request
may cost money.
