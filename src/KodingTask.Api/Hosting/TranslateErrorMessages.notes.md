# TranslateErrorMessages

A result filter that translates the `message` of every `ErrorResponse` a controller
returns into the request's language (`LocaleResolver`), through `ServerTexts`. The `error`
code is untouched, so the web client keeps mapping codes to its own texts; API users,
logs aside, now read the message in their language. Refusals written outside MVC (the rate
limiter's 429) call `Translate` directly.

It is an **always-run** result filter (`IAsyncAlwaysRunResultFilter`). An ordinary result
filter is skipped when an authorization filter ends the request, so the antiforgery refusal
(`ValidateAntiforgeryHeader`) would stay in English and never become a term. The first production probe of the translation batch showed exactly that.
