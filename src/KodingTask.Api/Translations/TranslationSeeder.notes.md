# TranslationSeeder

Imports the bundled dictionaries into `Terms` and `Translations` on every start (and in the
tests, explicitly). It is idempotent: a second run changes nothing.

- **Terms**: every key of the English files becomes an active `Code` term. A `Code` term
  whose key left the files is deactivated, not deleted, so its translations survive a key
  that comes back.
- **Translations**: a bundled text is stored as `Baked` and reviewed. An existing `Machine`
  or `Baked` row is replaced by the bundled text, because the files are the reviewed
  source. A `Manual` row (edited on the review page) is never overwritten: the edit wins
  until someone exports it into the files.
- A `Baked` row whose text left the files is removed, so a translation deleted from a file
  does not linger in the database.
