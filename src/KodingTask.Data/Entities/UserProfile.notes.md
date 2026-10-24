# UserProfile

What this application keeps about an account beyond what Identity stores: the interface
language its emails and WhatsApp messages are written in, when the account first appeared
here, and when fmID last said which WhatsApp number it has (`PhoneCheckedAtUtc`, set on
every sign-in, including the silent one before a WhatsApp send).

## Why a separate table and not columns on the user

The user itself is a plain `IdentityUser`. Accounts may later come from an external
identity provider whose user store is not this database; keeping every application-specific
fact in tables keyed by the user id (`UserProfile`, `VerifiedPhone`, `OutboundMessage`,
`ConversionRecord`) means the user store can change without touching them.

`PreferredLocale` is updated whenever the visitor acts in a given interface language
(signing in, a delivery), so the next message speaks the language
they last used.
