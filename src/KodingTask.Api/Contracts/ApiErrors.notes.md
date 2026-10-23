# ApiErrors

The refusals of the account, phone and delivery endpoints, in the same shape as every other
API error (`{ "error": "<code>", "message": "<English sentence>" }`). The web client
translates by code; the English message is for API users and logs.

`StatusOf` keeps the HTTP status consistent with the meaning: 429 for every "limit or
pause" (the client may try again later), 403 for "you must do something first", 503 when the
instance lacks the feature, 502 when the provider failed, 409 for an existing account and
400 for the rest.
