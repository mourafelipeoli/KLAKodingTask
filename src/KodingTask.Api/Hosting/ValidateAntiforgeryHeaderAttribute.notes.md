# ValidateAntiforgeryHeaderAttribute

Validates the antiforgery token on every state-changing request of a controller (anything
but GET, HEAD and OPTIONS). It does what MVC's `AutoValidateAntiforgeryToken` does, without
requiring the view services this API-only host does not register, and answers a failure
with the API's usual JSON error (`InvalidAntiforgeryToken`, 400) instead of an empty 400.

The token travels in the `X-XSRF-TOKEN` header (see `AccountServiceCollectionExtensions`).
