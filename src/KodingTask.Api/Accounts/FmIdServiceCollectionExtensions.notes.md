# FmIdServiceCollectionExtensions

Registers the OpenID Connect client for fmID, only when `FmIdOptions` is complete.

## Contract with the provider

| Setting | Value | Why |
|---|---|---|
| Flow | Authorization code with PKCE (S256) | The only flow the provider accepts; PKCE binds the code to this browser. |
| `response_mode` | query (the default of the code flow, so the parameter is left out) | The provider only offers query. The callback is a GET, which also lets the `SameSite=Lax` cookies through. |
| Client authentication | Client secret at the token endpoint | Confidential client: the server, not the browser, redeems the code. |
| Scopes | `openid profile email phone` | `email` and `email_verified` link an fmID identity to a local account; `phone_number` and `phone_number_verified` are the only source of the WhatsApp number. |
| Claims | From the validated id_token only | The access token is encrypted and opaque to clients, and the id_token already carries `sub`, `name`, `email` and `email_verified`; no UserInfo call. |
| Tokens | Not saved | There are no refresh tokens and nothing calls the provider on the visitor's behalf afterwards; the local session cookie is the session. |
| Claim names | Not remapped (`MapInboundClaims = false`) | `sub` stays `sub` instead of becoming a long XML-schema claim type. |

`ui_locales` carries the interface language of the page that started the sign-in, so the
provider can show its pages in the same language when it supports it. `client_id` is added
to the end-session request because no id_token is kept to send as `id_token_hint`.

## Cookies on the way

The handler signs into Identity's external scheme (`__Host-kodingtask-external`, five
minutes); `FmIdController.Complete` reads it once, deletes it and creates the real session.
Correlation and nonce cookies are the handler's own.

## Failures

`OnAccessDenied` and `OnRemoteFailure` send the browser back to `/sign-in` with an
`FmIdErrors` code and the original return path (read from the authentication properties
when the state could be decoded, `/` otherwise). Without them the handler would end the
request with an exception page.

## Silent refresh

A sign-in started with the `prompt` item `none` (`FmIdController.Refresh`) asks the provider
without showing anything when its session is still open. When it is not (`login_required`,
`interaction_required` and the like), the browser goes to `/sign-in?…&with=fmid`, which
starts a normal sign-in by itself, and comes back to the same place.

## A sign-in that finishes in another browser

Creating an fmID account takes the visitor through a registration, an email confirmation
and a WhatsApp confirmation, and the confirmation link may open in another browser (the
phone's mail app, for instance). The provider then completes the authorization there, and
our callback arrives with a valid state but without the correlation cookie, which lives in
the browser that started. The first real sign-up ended exactly like that, on the error page.

- **One automatic restart.** When the callback fails locally (no `error` from the provider:
  missing correlation or nonce cookie, an expired attempt) and the state is ours, the browser
  goes to `/sign-in?…&with=fmid&retry=1`, which starts a sign-in by itself. That browser now
  has an fmID session, so the provider answers at once and the visitor lands signed in where
  they started. The retry carries the `retry` item; a second local failure shows
  `invalid-response` instead, so it can never loop. A callback whose state cannot be decoded
  (forged, or from another instance) is never retried.
- **A one-hour window** (`RemoteAuthenticationTimeout`, 15 minutes by default) for the
  correlation cookie and the state: a sign-up with two confirmations can take longer than a
  quarter of an hour.
