# MessageTexts

The texts of the messages the server writes itself (emails), in every interface language.

- **Same convention as the web client**: the English sentence is the key; `{{name}}` marks a
  parameter; `fr-CA`, `fr-CH` and `fr-BE` hold only what differs from `fr-FR` and fall back
  to it. An unknown locale or key falls back to the English key itself, so a message is
  never empty.
- **Embedded in the assembly** from `Messaging/Texts/<locale>.json`, so there is nothing to
  deploy next to the binary and nothing to configure.
- **Separate from the web dictionaries** on purpose: these sentences only ever leave the
  server inside an email, and the web client's check (`npm run check:i18n`) would flag them
  as unused there. `MessageTextsTests` enforces the same rules for them: every complete
  locale translates every key, with the same placeholders.

WhatsApp texts are not here: WhatsApp business messages must use templates approved by
Meta, so their wording lives in the Meta account (see `WhatsAppOptions.notes.md`); the
application only fills in the parameters.
