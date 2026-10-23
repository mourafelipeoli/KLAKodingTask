# AiConcurrencyGate

At most `MaxConcurrency` model calls at a time in this container (two by default). A request
that cannot get a slot within five seconds is refused with `AiBusy` instead of piling up
connections, threads and spend behind a slow model.

It is a per-container bound; the daily and monthly limits are the global ones.
