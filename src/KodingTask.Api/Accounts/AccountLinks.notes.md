# AccountLinks

The links the server builds: the conversion link in result emails, and the sign-in page a
failed fmID sign-in returns to.

**The base address of emailed links never comes from the request outside development.**
Building it from the `Host` header would let a forged host put a link to someone else's
site into an email sent to a real address (host-header poisoning). Hosted, links use
`PublicSite:Url`; in development they use the local address, so a developer's link opens
their own instance.

The conversion link carries the interface language (`lang`) the visitor used, so the page it
opens speaks the same language as the email. `SignInPath` is a path on this site with the
error code and the sanitised return path; `SignInAndStartPath` opens the sign-in page so
that it starts a sign-in by itself, with `retry=1` for the one automatic retry after a
callback this browser could not match (see `FmIdServiceCollectionExtensions`).
