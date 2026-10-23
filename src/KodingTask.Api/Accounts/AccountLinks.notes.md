# AccountLinks

The links the server puts in emails: confirm the email, reset the password, open a
conversion. They point at pages of the web client, which call the API.

**The base address never comes from the request outside development.** Building it from the
`Host` header would let anyone ask for a password reset with a forged host and have the
victim receive a reset link to an attacker's site (host-header poisoning). Hosted, links use
`PublicSite:Url`; in development they use the local address, so a developer's confirmation
link opens their own instance.

Every link carries the interface language (`lang`) the visitor used when it was requested, so the
page it opens speaks the same language as the email.
