# TermRegistry

Turns texts nobody translated yet into terms, which the batch then translates.

- **From pages** (`RegisterAsync`): the web client reports texts its dictionaries do not
  have. The endpoint is anonymous, so it is boxed in: at most 20 texts of 300 characters
  per request, no control characters, and every **new** term consumes one unit of two daily
  caps (`TermRegistrationsPerDay` for the instance, `TermRegistrationsPerClientPerDay` per
  anonymised client). Texts that are terms already cost nothing, and a deactivated
  `Visitor` term comes back to life. The kill switch stops new terms.
- **From the server** (`AddServerTermsAsync`): texts `ServerTexts` met without knowing them.
  They come from the code, not from visitors, so no caps.

A race between two instances inserting the same text ends in a unique-key violation on
`TextHash`; the loser drops its changes, and the term exists either way. New terms wake the
batch (`TranslationBatchSignal`) instead of waiting for its next round.
