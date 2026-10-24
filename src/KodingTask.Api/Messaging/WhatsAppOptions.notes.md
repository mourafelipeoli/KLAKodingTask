# WhatsAppOptions

Bound from `WhatsApp`. WhatsApp is on when the phone number id, the access token and the
result template are set.

- `PhoneNumberId`: the sending number's id in the WhatsApp Business account (Meta).
- `AccessToken`: a system-user token with `whatsapp_business_messaging`. A secret: store it
  in a secret store, never in a file of the repository.
- `ResultTemplate`: a UTILITY template with two body parameters, `{{1}}` the amount and
  `{{2}}` the words, approved in every language the interface offers (or at least in
  `FallbackLanguage`).
- `FallbackLanguage`: used when the template is not approved in the visitor's language.

Business-initiated WhatsApp messages can only be templates, so the content of every message
is fixed by Meta's approval; the application can only fill in the parameters. That is one
of the reasons this channel cannot be turned into a spam cannon.
