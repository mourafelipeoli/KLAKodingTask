# AccountServiceCollectionExtensions

Accounts on ASP.NET Core Identity, with the session in a cookie. Identity is the user store
and the session; signing in is fmID's (`FmIdServiceCollectionExtensions`).

- **Identity core without passwords**: no password rules, lockout or token providers are
  configured, because no account has a password. `FmIdAccounts` creates the users, each
  with a unique, already confirmed address.
- **Session cookie** `__Host-kodingtask`: host-only, secure, HTTP-only, `SameSite=Lax`
  (Strict would drop the session on the redirect back from fmID), 14 days sliding. API
  calls get 401/403 instead of redirects.
- **External cookie** `__Host-kodingtask-external`: Identity's short-lived cookie between the
  redirect back from fmID and the local session, renamed and secured like the others.
- **Antiforgery** for every state-changing call: the client fetches a token from
  `GET /api/account/antiforgery` and sends it in `X-XSRF-TOKEN` (JSON calls) or as a form
  field (the fmID form posts); its cookie is Strict.
