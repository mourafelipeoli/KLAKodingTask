# AppInfoResponse

Everything the web client needs to know about this instance, in one request at startup:
version and commit for the footer, the public links (hosted site, repository, author's
portfolio), and which optional features are configured (see `FeatureAvailability`).
Nothing here is secret: it is the same information the header, the footer and the UI
reveal anyway. `Author` is null unless both its name and URL are configured.

`FmIdAccountUrl` is fmID's account page (where the WhatsApp number is managed), or null when
fmID or the setting is missing.
