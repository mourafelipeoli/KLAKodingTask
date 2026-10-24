# TranslationsController

`/api/i18n`, public.

| Endpoint | What it does |
|---|---|
| `GET /api/i18n` | The dictionary of the visitor's language (`LocaleResolver`). |
| `GET /api/i18n/languages` | Every interface language: code, native name, parent of a regional variant. |
| `GET /api/i18n/{locale}` | One dictionary, with `ETag` and `Cache-Control: no-cache`: the browser keeps it and revalidates it, so an unchanged dictionary costs a 304. 404 for a language the enum does not declare. |
| `POST /api/i18n/terms` | Texts a page shows that no dictionary knows; 202 with how many became terms. Antiforgery like every state-changing call, its own rate limit (`RateLimits:TranslationsPerMinute`), and the daily caps in `TermRegistry`. |

The dictionary endpoints share the conversions' rate limit: a page loads one or two per
visit.
