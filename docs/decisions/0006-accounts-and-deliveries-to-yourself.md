# 6. Accounts and deliveries that can only reach their own owner

Date: 2026-10-23 · Status: accepted

## Context

The hosted version can send a conversion result by email or WhatsApp. Both channels cost
money and reach third parties, and the code is public: whoever runs it must not be able to
turn the site into a way to message strangers, and the people who can read the code must
not be able to raise its allowance.

## Decision

- **Only to yourself.** A delivery has no recipient field. Email goes to the account's own
  address, after the address is confirmed; WhatsApp goes to a number the account verified
  with a one-time code. The words are converted again on the server from the amount, so the
  content is never supplied by the client; WhatsApp content is further fixed by Meta's
  template approval.
- **Local accounts on ASP.NET Core Identity**, in the application's own database, with
  password lockout, email confirmation and password reset. Application-specific data
  (preferred language, verified number, pending code, delivery history) lives in separate
  tables keyed by the user id, so the user store can later federate with an external
  OpenID Connect provider without migrating it.
- **Caps outside the code**: instance-wide per channel per day, per account per day, phone
  codes and account emails per account per day, registrations per client per day, and the
  kill switch, all read from the parameter store and enforced with atomic counters.
- **IAM scoped to the sender**: the task role may only call `ses:SendEmail` from one address
  through one configuration set.
- **Hand-over**: a copy without a channel keeps the buttons usable and continues on the
  deployed version, which lands on the send panel and completes the delivery after any
  sign-in on the way.

## Consequences

- A visitor needs an account to receive anything; the converter itself stays anonymous.
- Every message is recorded with a masked destination and the provider's id; rows older than
  90 days are purged.
- Development can exercise the whole flow with `Mode=Log` senders, which refuse to start
  outside Development.

## Alternatives considered

- **Send to any address typed in the form**: simplest, and exactly the spam cannon the
  guardrails exist to prevent.
- **Magic-link sign-in without passwords**: fewer fields, but every sign-in costs an email
  against the daily cap.
