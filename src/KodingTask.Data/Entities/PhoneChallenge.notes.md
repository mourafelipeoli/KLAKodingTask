# PhoneChallenge

The pending one-time code for an account that is verifying a WhatsApp number. One per
account; asking for a new code replaces it.

- **Only a hash is stored** (`CodeHash`, HMAC-SHA256 of the code with the account id), so
  a database reader cannot use a pending code.
- `SentAtUtc` enforces the resend cooldown; `ExpiresAtUtc` the code lifetime;
  `FailedAttempts` locks the challenge after too many wrong guesses. The daily number of
  codes per account is a separate cap, outside the code (see `UsageLimits`).
