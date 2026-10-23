# UsageLimitsSource

Answers "what are the limits right now?" from configuration or from AWS Systems Manager
Parameter Store (see `LimitsOptions`).

- **Fail closed, everywhere.** A missing parameter means 0 (or kill switch on); an
  unreadable store, missing credentials or a value that does not parse closes every paid
  feature for ten seconds, then tries again. A typo in a parameter can only switch features
  off, never raise a cap to infinity. Every exception is caught for that reason (except a
  cancellation of the request itself): closing is always the safe answer.
- **Cached** for `CacheSeconds` so a burst of requests costs one store read per minute. A
  semaphore makes concurrent requests share one refresh instead of stampeding the store.
- `WithDecryption` lets the kill switch or a limit be a SecureString if someone prefers; it
  costs nothing for plain strings.
- The SSM client is optional in the constructor: without `SsmPath` it is never registered,
  so a local run needs no AWS credentials.
