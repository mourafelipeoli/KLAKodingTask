# AccountServiceCollectionExtensions

Local accounts on ASP.NET Core Identity, with the session in a cookie.

- **Identity core, not the default UI**: the web client has its own pages and talks to
  `/api/account`; Identity provides the user store, password hashing, lockout and the email
  confirmation and password reset tokens.
- **Password**: at least 10 characters with 4 different ones, no composition rules (length
  beats symbols). **Lockout**: 5 failures, 15 minutes.
- **Email confirmation is not required to sign in**, only to receive anything: a new account
  can sign in at once, resend the confirmation, and is refused deliveries until confirmed.
- **Session cookie** `__Host-kodingtask`: host-only, secure, HTTP-only, `SameSite=Lax`
  (Strict would drop the session on the redirect back from an external sign-in), 14 days
  sliding. API calls get 401/403 instead of redirects.
- **Antiforgery** for every state-changing account and delivery call: the client fetches a
  token from `GET /api/account/antiforgery` and sends it in `X-XSRF-TOKEN`; its cookie is
  Strict. With the Lax session cookie and JSON bodies a cross-site request would already
  fail; the token closes the remaining gaps.
