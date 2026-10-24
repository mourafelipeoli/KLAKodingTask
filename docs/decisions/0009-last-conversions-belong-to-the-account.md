# 9. Last conversions belong to the account

Date: 2026-10-24 · Status: accepted

## Context

A visitor who converts several amounts wants to find them again. The converter itself is
anonymous and stateless; the database already holds accounts (signed in with fmID) and
removes old rows every day.

## Decision

- **Only for signed-in visitors.** Their conversions are kept against the account, so the
  list follows them to any device. Anonymous visitors leave nothing behind; the card offers
  to sign in and comes back to it.
- **Written by the conversion itself.** `POST /api/conversions` records its own result when
  a session is present. There is no endpoint to add an entry, so the list only ever holds
  words the server produced. `GET /api/conversions` never writes.
- **Short and distinct.** One entry per normalised amount and output language, moved to the
  top when converted again; ten per person; rows older than 90 days are purged.
- **Best effort.** A failure to record is logged and never fails the conversion.
- **The visitor can clear the list** at any time.

## Consequences

- One small write per conversion for signed-in visitors, bounded to ten rows per person.
- AI answers are not kept: the entry reopens the conversion, and the AI can be asked again.
- A copy without fmID has no accounts, so no list; the sign-in link hands over to the
  deployed version like every other account action.

## Alternatives considered

- **Per browser, without an account** (a random cookie id): no sign-in needed, but lost
  with the cookies, tied to one device, and a row per anonymous visitor to keep and purge.
- **A public feed of everyone's conversions**: livelier, but it would publish what strangers
  typed.
- **Browser storage only**: no database at all, but nothing follows the visitor to another
  device, and the request was to use the database.
