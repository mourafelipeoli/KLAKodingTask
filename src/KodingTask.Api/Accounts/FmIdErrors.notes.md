# FmIdErrors

The `?error=` codes the sign-in page receives when a sign-in with fmID ends without a
session. The provider only returns standard OAuth errors; these codes belong to this app.

| Code | When |
|---|---|
| `refused-by-provider` | The provider answered `access_denied` (account locked there, or two-factor enabled, which the provider refuses for OpenID Connect). |
| `invalid-response` | Anything else that went wrong: forged or expired state, a failed code exchange, a token that does not validate, a missing antiforgery token, missing claims. |
| `no-access` | The provider says the address is not verified, so no account can be created or linked for it. |
| `unavailable` | fmID is not configured on this instance (the web client hands over before posting, so this only answers hand-made requests). |

They are kebab-case strings rather than the PascalCase codes of `ApiErrors` because they
travel in a URL, not in a JSON body.
