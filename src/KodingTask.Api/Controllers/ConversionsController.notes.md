# ConversionsController

The server side of the task: every conversion happens here, never in the browser.

| Endpoint | Purpose |
|---|---|
| `GET /api/conversions/languages` | Supported output languages, in display order. |
| `POST /api/conversions` | `{ "amount": "45 100", "language": "de-DE" }` → words. Used by the UI; for a signed-in visitor it also records the conversion in their last conversions. |
| `GET /api/conversions?amount=45%20100&language=de-DE` | Same, for a browser address bar or `curl`. |
| `POST /api/conversions/all-languages` | `{ "amount": "45 100" }` → one result per language, for the comparison table. |

- Success is 200 with `ConversionResponse`; any refusal is 400 with `ErrorResponse`
  (`{ "error": "InvalidGrouping", "message": "…" }`).
- A missing language falls back to English, the language of the task.
- **No antiforgery token on these endpoints.** They answer anyone, and the only thing they
  store is the signed-in visitor's own last conversions (`POST` only). A cross-site page
  cannot make that write: the session cookie is `SameSite=Lax`, so it does not travel on a
  cross-site `POST`, and a JSON body needs a CORS preflight that this API never grants.
- **`GET` stores nothing**, signed in or not: a link or a crawler must never write.
- Rate limited per client address (`RequestRateLimits.Conversion`); the language list is
  not, because it is static.
- The all-languages endpoint validates the amount once per language through the same
  converter. That repeats a microsecond of parsing for each language and keeps one code path for
  every conversion; any failure is the same failure in every language, so the first one is
  returned.
