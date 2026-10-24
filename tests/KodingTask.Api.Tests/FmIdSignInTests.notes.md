# FmIdSignInTests

The sign-in with fmID end to end through the real OpenID Connect handler (see
`FakeFmIdProvider`): the authorize request (client, PKCE, scopes, callback, interface
language), the code exchange, the return to the page that started it, and the account rules
of `FmIdAccounts` (new account, link by subject, link by verified address, takeover of an
unconfirmed account, refusal of an unverified address, registration cap). Also the failure
paths (refusal by the provider, a forged callback, a missing antiforgery token),
RP-initiated logout, the `form-action` directive and the instance without fmID settings.

Each test uses its own client with `AllowAutoRedirect = false`, so every redirect of the
round trip is asserted instead of followed.
