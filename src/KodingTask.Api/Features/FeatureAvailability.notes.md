# FeatureAvailability

Which optional capabilities this instance can actually perform. The single-page app reads
it from `GET /api/app-info` and, for anything that is off, shows a message in the user's
language pointing to the hosted version instead of a broken button.

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
