# FeatureAvailability

Which optional capabilities this instance can actually perform. The single-page app reads
it from `GET /api/app-info`. On a local copy, using a feature that is off opens a dialog in
the user's language that continues on the hosted version, same page and same parameters,
instead of a broken button.

## Why it exists

The task asks for a converter; this project adds features that depend on paid or
credentialed services (email, WhatsApp, an AI model, machine translation, an external
identity provider). Someone who clones the repository has none of those credentials, and
must still get a fully working converter on the first `dotnet run`. Each feature is
therefore optional and reports whether it is configured.

## How a flag becomes true

A flag is set by the feature's own registration method when, and only when, its
configuration is complete (for example, the email feature needs a sender address and a
region). `Program` builds the record from those results and registers it as a singleton,
so the flags reflect what was actually wired into the container, never a guess.

The deterministic converter is not listed: it is always available.

`Accounts` follows `Email`: an account with a password is only useful once its address is
confirmed, and confirming needs email. Without email, registering and signing in with a
password are handed over to the deployed version like every other optional feature.
`ExternalLogin` is independent: fmID vouches for the address itself, so it works with or
without email.

