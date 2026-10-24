# TranslationTests

The translation system end to end, on the real host with a fake machine translator.

The proof of the design is `A_language_without_any_translation_gets_every_text_from_the_machine`:
it deletes every Korean row, runs one batch and expects every active term translated,
in requests of at most 50, unreviewed, and served by `/api/i18n/ko-KR`. That is what adding
a member to `InterfaceLanguage` does in production.

The other tests cover the import (idempotent, hand edits kept), the caps and the kill
switch, the dictionary's ETag and the regional merge, the language resolution order, term
registration with its limits and antiforgery, a server text becoming a term, an error
message following the cookie's language, and the review API's access rules.
