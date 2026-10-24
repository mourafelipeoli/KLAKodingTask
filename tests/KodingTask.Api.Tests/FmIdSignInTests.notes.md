# FmIdSignInTests

The sign-in with fmID, the only one, end to end through the real OpenID Connect handler (see
`FakeFmIdProvider`): the authorize request (client, PKCE, scopes, callback, interface
language), the code exchange, the return to the page that started it, and the account rules
of `FmIdAccounts` (a new account without a password, the same account by subject, a new
subject joining the account of its verified address, refusal of an unverified address).
Also the failure paths (refusal by the provider, a forged callback, a missing antiforgery
token, the same callback requested twice, a callback in a browser that did not start the
sign-in and restarts it once, and a retry that fails again), RP-initiated logout, the `form-action` directive and the instance without fmID,
which has no accounts. The WhatsApp number follows fmID's verified claim on every sign-in;
the silent refresh asks with `prompt=none` and falls back to a normal sign-in when fmID
cannot answer silently.

Every client keeps cookies and has `AllowAutoRedirect = false`, so every redirect of the
round trip is asserted instead of followed. The other API tests sign in the same way through
`MessagingTestHost.SignedInAccountAsync`.
