# AccountController

`/api/account`: the signed-in visitor. Signing in and out is `FmIdController` (same route
prefix, same rate limit); there are no passwords and no phone codes here.

| Endpoint | What it does |
|---|---|
| `GET antiforgery` | Issues the antiforgery token the client sends in `X-XSRF-TOKEN` (or as a form field in the fmID form posts). Fetch it again after signing in or out: tokens are bound to the identity. |
| `GET me` | Who the visitor is (or `isSignedIn: false`), with the WhatsApp number fmID verified, masked, and whether `Admin:Emails` lists them (`isAdmin`). |

Behind the `account` rate limit; antiforgery on unsafe methods.
