# SpellerCatalog

The list of supported output languages and the lookup by code.

- `Default` is the single place where languages are registered. Adding a language is one
  new speller class plus one line here; the API listing, the validation of incoming codes
  and the UI menu all follow from it.
- The order of `Default` is the order shown to users: English first (the task's language and
  the code's default), then the languages in the order the product owner listed them.
- The four French entries share one class. Only the numbering differs (see
  `FrenchSpeller.notes.md`), so a variant is a constructor argument, not a copy.
- Lookup is case-insensitive and accepts `fr_BE` as well as `fr-BE`, because both spellings
  circulate (Java and gettext use the underscore).
