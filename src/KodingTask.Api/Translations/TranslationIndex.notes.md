# TranslationIndex

The per-language dictionaries the API serves, built from the database and cached in memory.

- **English lists every active term** (text → itself). That is how the server knows which
  texts are terms already (`KnowsTerm`), and how the client knows what not to report.
- **A regional variant is merged**: the parent's rows first, the variant's overrides on top,
  so `fr-CA` is complete although it stores only what differs.
- **Version**: the first 16 hex digits of a SHA-256 of the sorted entries. It becomes the
  ETag, so an unchanged dictionary costs the client a 304.
- **Five minutes** in memory. A save on the review page or new terms clear the cache of
  this instance at once; other instances pick the change up within five minutes, which is
  the trade-off for not needing a message bus.
- `Cached` never waits: a missing or old entry is reloaded in the background and the caller
  gets what there is (possibly nothing). `ServerTexts` uses it on every email and error
  message, so a database hiccup can never make a request slower or fail; the bundled
  dictionaries answer meanwhile. `KnowsTerm` answers "yes" until English is loaded, so a cold
  start does not report every text as new.
