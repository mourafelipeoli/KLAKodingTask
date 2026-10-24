# FmIdOptions

Settings of the sign-in with fmID, the author's OpenID Connect provider (section `FmId`).

- **All three or nothing.** `Authority`, `ClientId` and `ClientSecret` must all be set;
  otherwise the scheme is not registered at all and `FeatureAvailability.ExternalLogin` stays
  false. fmID being the only sign-in, a fresh clone then has no accounts: the converter works
  as always, and everything that needs an account hands over to the deployed version.
- **The secret is configuration, never code**: user-secrets in development, a secret store
  when hosted. The provider stores only its hash; a leaked secret is rotated there.
- **Fixed paths.** `/signin-fmid` and `/signout-fmid` are the redirect and post-logout URIs
  registered at the provider, which matches them exactly (scheme, host, port, path). They
  are constants because changing one means registering it again on the provider side.
- `AuthorityOrigin` feeds the `form-action` directive of the content security policy (see
  `SecurityHeaders`).

Any OpenID Connect provider that issues `sub`, `email` and `email_verified` in the id_token
works with these settings; only the button text names fmID.

`AccountUrl` (optional) is the provider's account page, where people add and confirm their
WhatsApp number; `/api/app-info` publishes it as `fmIdAccountUrl` when fmID is configured,
and the send panel links to it.
