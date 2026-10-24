# FmIdAccounts

Turns the identity fmID vouched for into a local account. The OpenID Connect principal is
never the session: the local `IdentityUser` is, so everything else (deliveries, phone
verification, caps) works the same whichever way the visitor signed in.

## Order of the lookup

1. **The subject first.** `AspNetUserLogins` (provider `fmID`, key `sub`) finds an account
   linked earlier, even if the address changed at the provider since. The local address is
   not synchronised: it is where results are emailed, and it was confirmed here.
2. **Then the address, only if verified.** `email_verified` must be `true`; an unverified
   address is refused (`no-access`) instead of linked, because linking by an address nobody
   proved would hand an existing account to whoever typed it at the provider.
3. **Then a new account**, with `EmailConfirmed = true` and no password, counted against the
   registrations cap of the client address and refused while the kill switch is on, exactly
   like a registration with a password.

## Linking to an account that already has the address

- **Confirmed here**: the same person proved the address twice. The fmID login is added and
  the password keeps working.
- **Not confirmed here**: whoever registered it with a password never proved they own the
  address, and the visitor at fmID just did. The account is taken over: the password is
  removed and the security stamp rotated, so the earlier registrant can neither sign in
  again nor keep a session past the next stamp check. An unconfirmed account holds nothing
  worth keeping (no verified phone, no deliveries), so nothing is lost.

## Lockout

A locked account stays locked for fmID too (`locked-out`), as ASP.NET Core Identity does for
external logins: the lockout protects the account, not only the password.
