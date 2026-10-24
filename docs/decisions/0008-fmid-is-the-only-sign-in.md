# 8. fmID is the only sign-in

Date: 2026-10-24 · Status: accepted

## Context

Decision 6 created accounts with passwords; decision 7 added sign-in with fmID next to
them. Two ways into the same account cost a registration flow, email confirmation, password
reset, lockout, the confirmation and reset emails with their own cap, a cap on new accounts
per client address, the takeover of unconfirmed accounts when fmID proved the address, and
password hashes in a database that public code runs against. fmID already does all of that
for the author's other websites.

## Decision

- **fmID is the only way to sign in.** No password is stored here. Creating an account,
  confirming the address, resetting the password and refusing two-factor accounts are the
  provider's; its sign-in page offers sign-up and comes back to this site signed in.
- **Removed**: registration, password sign-in, email confirmation, forgot and reset
  password (endpoints, pages and emails), the lockout, the caps on account emails
  (`AccountEmailsPerUserPerDay`) and on new accounts per client address
  (`RegistrationsPerClientPerDay`), and the takeover of unconfirmed accounts.
- **Kept**: ASP.NET Core Identity as the user store and the session cookie, the link by
  subject in `AspNetUserLogins`, the `email_verified` gate, phone verification, deliveries
  and every delivery cap and the kill switch.
- **No schema change.** Identity's standard user table keeps its password and lockout
  columns, unused. Dropping them would take a custom user type and a migration for no gain.
- **No cap on new accounts.** Each needs a confirmed fmID account, and a per-address cap
  would stop a team behind one office network at its sixth person.

## Consequences

- A copy without an OpenID Connect provider has no accounts: the converter works as always,
  and the send buttons and the sign-in hand over to the deployed version. Trying the
  delivery flow locally needs a provider with a development client.
- Visitors need an fmID account, created on the provider's page in about a minute.
- Less to attack and to maintain: no password guessing, no reset links, no address
  enumeration, about a thousand lines and thirty interface texts fewer.

## Alternatives considered

- **Keep both**: the most choice for visitors, and every cost listed above.
- **Magic links instead of passwords**: no password either, but every sign-in costs an email
  against the daily cap, and the confirmation machinery stays.
