# 7. Sign-in with fmID, linked to local accounts

Date: 2026-10-24 · Status: accepted

## Context

Accounts exist only so a visitor can send results to themselves (see decision 6). A
password is one more thing to create for a visitor who already has an account on the
author's websites. fmID is the author's own OpenID Connect provider; this application is one
of its clients. The repository is public, and the provider also serves other applications.

## Decision

- **A confidential server-side client**: authorization code with PKCE, the code redeemed by
  the server with a client secret, the id_token validated by the ASP.NET Core OpenID Connect
  handler. No token reaches the browser and none is stored: the session is this
  application's own cookie.
- **Local accounts stay the source of truth.** The provider's subject is linked to a local
  ASP.NET Core Identity account through `AspNetUserLogins`; deliveries, phone verification
  and caps work the same whichever way the visitor signed in.
- **Linking needs a verified address.** A first sign-in links to the local account with the
  same address only when the provider says `email_verified`; an unconfirmed local account
  with that address is taken over (password removed, security stamp rotated). Otherwise a
  new account is created, already confirmed, within the registrations cap.
- **Navigations, not API calls**: the web client starts the sign-in and the sign-out with a
  form post (antiforgery token as a field), and the server answers with redirects. The
  provider's origin is allowed in `form-action`.
- **Sign-out ends both sessions** when the session started at fmID (RP-initiated logout); a
  password session signs out here only.
- **Optional like every other feature.** Without the three settings the scheme is not
  registered; the button stays and hands the visitor over to the deployed version, which
  starts the sign-in by itself.

## Consequences

- One account can be reached by password and by fmID once both prove the same address.
- Signing in with fmID honours the local lockout and the registrations cap.
- The provider's account pages (password change, address change) are the provider's; a
  changed address at the provider does not change the local one, which is where results
  are emailed.

## Alternatives considered

- **Use the provider's user database directly**: no linking step, but a container running
  public code would hold credentials to every user of the author's other systems.
- **Sign in from the browser (tokens in the single-page app)**: no server round trip, but
  tokens in JavaScript and a public client without a secret, for no benefit here.
- **Link by address without `email_verified`**: would let whoever typed an address at the
  provider take over the account that owns it here.
