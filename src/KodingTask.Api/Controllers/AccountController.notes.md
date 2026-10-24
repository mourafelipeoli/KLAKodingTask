# AccountController

`/api/account`: local accounts for the web client.

| Endpoint | What it does |
|---|---|
| `GET antiforgery` | Issues the antiforgery token the client sends in `X-XSRF-TOKEN`. Fetch it again after signing in or out: tokens are bound to the identity. |
| `GET me` | Who the visitor is (or `isSignedIn: false`), and whether the session started at fmID. |
| `POST register` | Creates the account, emails the confirmation link, signs in. |
| `POST confirm-email` | Confirms the address from the emailed link and signs in. |
| `POST resend-confirmation` | Sends the confirmation again (capped per account per day). |
| `POST sign-in` / `POST sign-out` | Password sign-in with lockout; sign-out. |
| `POST forgot-password` | Emails a reset link when the account exists. Always 204, so the endpoint does not reveal which addresses have accounts. |
| `POST reset-password` | Sets the new password from the emailed link; the link also proves the address, so it confirms it. |
| `POST phone/code` / `POST phone/verify` | WhatsApp number verification (see `PhoneVerification`). |

Every endpoint is behind the `account` rate limit and validates antiforgery on unsafe
methods. Registration is unavailable when email is not configured (an account could never
be confirmed), and capped per client address per day (`RegistrationsPerClientPerDay`).

**Registration says when an address is taken.** Hiding it would need a "check your email"
flow for every attempt; for an app whose accounts only let you email yourself, the clearer
message is the better trade, and the per-address rate limit keeps enumeration slow.

Sign-in with fmID lives in `FmIdController` (same route prefix, same rate limit).
