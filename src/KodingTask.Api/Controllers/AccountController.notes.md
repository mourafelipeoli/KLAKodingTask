# AccountController

`/api/account`: the signed-in visitor and their WhatsApp number. Signing in and out is
`FmIdController` (same route prefix, same rate limit); there are no passwords here.

| Endpoint | What it does |
|---|---|
| `GET antiforgery` | Issues the antiforgery token the client sends in `X-XSRF-TOKEN` (or as a form field in the fmID form posts). Fetch it again after signing in or out: tokens are bound to the identity. |
| `GET me` | Who the visitor is (or `isSignedIn: false`). |
| `POST phone/code` / `POST phone/verify` | WhatsApp number verification (see `PhoneVerification`). |

Every endpoint is behind the `account` rate limit and validates antiforgery on unsafe
methods. A phone code also needs a confirmed address: every account fmID creates has one,
and the check keeps the rule explicit should another way in ever appear.
