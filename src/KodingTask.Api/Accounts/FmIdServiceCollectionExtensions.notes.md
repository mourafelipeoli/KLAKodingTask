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
