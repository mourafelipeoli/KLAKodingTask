# 10. The WhatsApp number comes from fmID

Date: 2026-10-24 · Status: accepted

## Context

Decision 6 verified a WhatsApp number in this application with a one-time code sent through
a Meta authentication template: an extra template to get approved, a code flow, a table of
pending codes, a daily cap on codes, and paid messages that only prove ownership. Since
decision 8, fmID is the only sign-in, and it now manages a WhatsApp number per person,
confirmed there with its own WhatsApp code and emitted as the standard `phone_number` and
`phone_number_verified` claims.

## Decision

- **The number is fmID's.** The client asks for the `phone` scope; every sign-in stores
  `phone_number` when `phone_number_verified` is true and removes it otherwise. The number is
  added, changed and confirmed only on fmID's account page, which the send panel links to.
- **Fresh before sending.** A WhatsApp send needs a check of the number less than ten minutes
  old. Otherwise the server answers `PhoneCheckNeeded`, the web client signs in silently
  (`prompt=none`) and the send completes on the way back. When fmID cannot answer silently,
  a normal sign-in takes over.
- **Removed**: the code endpoints, the verification form, `PhoneChallenges`, the code
  template (`koding_task_code`) and the `PhoneCodesPerUserPerDay` cap. Migration
  `PhoneFromFmId` drops the table and adds the check time to `UserProfiles`.

## Consequences

- One Meta template on this side (the result); no paid code messages.
- A person manages one number for every application that uses fmID.
- A number removed or changed on fmID stops receiving results here within ten minutes.
- WhatsApp results depend on fmID's own verification being live; until it is, nobody has a
  verified number and the panel points to the fmID account page.

## Alternatives considered

- **Keep this application's code**: independent of the provider, but a second number to
  manage, a second template and paid messages for every verification.
- **Take the number only at sign-in**: simplest, but a removed number would keep receiving
  results for the whole 14-day session.
- **Both, the app's code as a fallback**: the most code, for a case fmID already covers.
