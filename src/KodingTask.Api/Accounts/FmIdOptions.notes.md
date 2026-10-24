# FmIdOptions

Settings of the sign-in with fmID, the author's OpenID Connect provider (section `FmId`).

- **All three or nothing.** `Authority`, `ClientId` and `ClientSecret` must all be set;
  otherwise the scheme is not registered at all and `FeatureAvailability.ExternalLogin` stays
  false, so a fresh clone starts without any provider and hands the button over to the
  deployed version.
- **The secret is configuration, never code**: user-secrets in development, a secret store
  when hosted. The provider stores only its hash; a leaked secret is rotated there.
- **Fixed paths.** `/signin-fmid` and `/signout-fmid` are the redirect and post-logout URIs
  registered at the provider, which matches them exactly (scheme, host, port, path). They
  are constants because changing one means registering it again on the provider side.
- `AuthorityOrigin` feeds the `form-action` directive of the content security policy (see
  `SecurityHeaders`).

Any OpenID Connect provider that issues `sub`, `email` and `email_verified` in the id_token
works with these settings; only the button text names fmID.
