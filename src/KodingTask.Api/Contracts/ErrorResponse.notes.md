# ErrorResponse

The one error shape of the API: `{ "error": "<code>", "message": "<English sentence>" }`.

- `error` is the contract. Clients branch on it; the web client uses it to pick the
  translated message in the user's language.
- `message` is for people reading the API directly (curl, the OpenAPI document, logs). It is
  English, the language of the API.

The English sentences are the same ones the web client uses as translation keys, so a
message reads identically whether it comes from the API or from the UI. A test checks that
every `ConversionError` has a message, so adding a new code without one fails the build.
