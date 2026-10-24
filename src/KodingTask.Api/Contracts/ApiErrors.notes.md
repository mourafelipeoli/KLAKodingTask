# ApiErrors

The refusals of the account, delivery and translation endpoints, in the same shape as
every other API error (`{ "error": "<code>", "message": "<sentence>" }`). The web client
translates by code. The message is written in English here and translated into the
request's language on the way out (`TranslateErrorMessages`), so API users read it in their
language too; a new message becomes a term the first time it is used.

`StatusOf` keeps the HTTP status consistent with the meaning: 429 for every "limit or
pause" (the client may try again later), 403 for "you must do something first", 503 when the
instance lacks the feature, 502 when the provider failed, 409 for an existing account, 404
for an unknown language or term on the translation endpoints and 400 for the rest.
