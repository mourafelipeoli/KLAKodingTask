# FmIdAccounts

Turns the identity fmID vouched for into the local account, the only kind there is. The
OpenID Connect principal is never the session: the local `IdentityUser` is, the anchor of
everything the application keeps per person (preferred language, verified number, pending
code, delivery history, per-account caps).

## Order of the lookup

1. **The subject first.** `AspNetUserLogins` (provider `fmID`, key `sub`) finds the account
   linked before, even if the address changed at the provider since. The local address is
   not synchronised: it is where results are emailed, and it was verified when the account
   was created.
2. **Then the address, only if verified.** `email_verified` must be `true`; otherwise the
   sign-in is refused (`no-access`). A verified address that already has an account (the
   person re-created their fmID account, so the subject is new) gets the new subject linked
   to that account instead of a second account.
3. **Then a new account**: the address as user name and email, confirmed, no password.

## No cap on new accounts

Creating one takes an fmID account with a confirmed address, which the provider controls. A
per-address cap here would stop a team behind one office network at its sixth person. The
kill switch and the delivery caps still stop everything that costs money.

## The WhatsApp number

Every sign-in (the silent one included) also takes the number from the id_token:
`phone_number` is stored only when `phone_number_verified` is `true`, and the stored number
is removed otherwise. The check time is kept on the profile; `DeliveryService` asks for a
new check when it is older than ten minutes, so a number removed or changed on fmID stops
receiving results within minutes, not at the end of a 14-day session.
