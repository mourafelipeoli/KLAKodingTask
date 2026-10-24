# FmIdController

`/api/account/fmid`: the three steps of a sign-in with fmID that are not the handler's own.
Not part of the OpenAPI document: these are browser navigations, not JSON endpoints.

| Endpoint | What it does |
|---|---|
| `POST sign-in` | Form post from the sign-in or register page (antiforgery token as a form field, `returnUrl`, `uiLocale`). Challenges the `fmID` scheme, which answers with the redirect to the provider. |
| `GET complete` | Where the handler lands after `/signin-fmid` validated the answer. Reads and deletes the external cookie, resolves the local account (`FmIdAccounts`), remembers the interface language, signs in with authentication method `fmID` and returns to `returnUrl`. |
| `POST sign-out` | Form post from the account menu for a session that started at fmID: ends the local session, then the provider's (RP-initiated logout), which comes back through `/signout-fmid` to `returnUrl`. A password session only signs out here. |

- **POST, with antiforgery, to start.** A GET that starts a sign-in could be triggered by any
  page; a form post with a token can only come from this site. An invalid token returns to
  the sign-in page with `invalid-response` instead of a JSON error, because the visitor is
  navigating, not calling an API.
- **Navigations, not fetch calls.** Leaving for the provider needs a top-level navigation,
  so the web client submits a form and the server answers with redirects.
- **Return paths are sanitised** (`ReturnUrls`) and redirects are local (`LocalRedirect`):
  the provider round trip cannot be turned into an open redirect.
- **The authentication method is a claim** of the local session (`amr` = `fmID`), which is
  how `GET /api/account/me` knows to report `signedInWithFmId` and how `sign-out` knows to
  end the provider session too.
