# HostingTests

The hosting contract that does not belong to a single feature:

- `/health`: 200 JSON with status and version, `no-store`, `HEAD` without a body, 405 for
  other methods.
- `/api/app-info` on a bare configuration: every optional feature is off, which is exactly
  the state of a fresh clone, and the public links are present.
- Security headers, including the content security policy outside Development.
- The local SQLite database is created at startup without any setup.
- Every `ConversionError` has an API message (a new error code without one fails here).
- The OpenAPI document is served and describes the conversion endpoints.
