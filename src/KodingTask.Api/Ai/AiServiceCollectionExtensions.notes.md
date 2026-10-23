# AiServiceCollectionExtensions

Registers the AI engine and returns whether it is configured; `Program` turns that into the
`Ai` flag of `FeatureAvailability`.

Everything except the model client is registered either way, so controllers and services
can always be constructed. Without configuration the model client is a stub that throws,
and `AiConversionService` never reaches it because it refuses with `Unavailable` first.
No AWS client is created on a machine without AWS settings.

The Bedrock SDK client is a singleton (it is thread-safe and holds the HTTP connection
pool), with a request timeout from `AiOptions.TimeoutSeconds` and two SDK retries for
throttling and transient errors.
