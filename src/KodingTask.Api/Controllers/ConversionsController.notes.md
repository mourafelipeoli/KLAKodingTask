# ConversionsController

The server side of the task: every conversion happens here, never in the browser.

| Endpoint | Purpose |
|---|---|
| `GET /api/conversions/languages` | Supported output languages, in display order. |
| `POST /api/conversions` | `{ "amount": "45 100", "language": "de-DE" }` → words. Used by the UI. |
| `GET /api/conversions?amount=45%20100&language=de-DE` | Same, for a browser address bar or `curl`. |
| `POST /api/conversions/all-languages` | `{ "amount": "45 100" }` → one result per language, for the comparison table. |

- Success is 200 with `ConversionResponse`; any refusal is 400 with `ErrorResponse`
  (`{ "error": "InvalidGrouping", "message": "…" }`).
- A missing language falls back to English, the language of the task.
- **No antiforgery token on these endpoints.** They are stateless and anonymous: no cookie
  is read and nothing is stored, so there is nothing a cross-site request could abuse.
  Endpoints that act on behalf of a signed-in user require the token.
- Rate limited per client address (`RequestRateLimits.Conversion`); the language list is
  not, because it is static.
- The all-languages endpoint validates the amount once per language through the same
  converter. That repeats a microsecond of parsing eight times and keeps one code path for
  every conversion; any failure is the same failure in every language, so the first one is
  returned.
