# ServerTexts

Translates what the server itself writes (emails, error messages) with the same terms as
the web client, so one review covers both.

Order: the database dictionary of the language (cached, never awaited), the bundled
dictionary, the bundled dictionary of the parent language for a regional variant, then the
English text. `{{name}}` placeholders are filled after translating, so a translation can
move them.

A text that is not a term yet is queued (`PendingTerms`) and becomes one on the next round
of the batch: the first email with a new sentence goes out in English, the next ones in the
visitor's language once machine translation has run. That is the same "the code shows it,
the system learns it" flow as on the web client.
