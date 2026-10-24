# TranslationBatchService

The background loop of the translation system.

1. On start, imports the bundled dictionaries (`Translations:SeedOnStartup`, on by
   default; the tests switch it off and import explicitly).
2. Each round stores the texts the server queued (`PendingTerms`) and, when DeepL is
   configured, runs `TranslationBatch`.
3. Then it sleeps until a signal (new terms, or "Translate now" on the review page) or the
   interval (`DeepL:IntervalSeconds`, 120 s by default, at least 10 s).

Without DeepL the loop still runs: terms are stored, so the review page shows them and a
later key translates them. Every failure is logged by type only and the loop goes on;
translation must never take the application down.

With several instances, each runs its own loop. Two runs may translate the same text at
the same moment; the unique index on (term, language) keeps one row, and the cost is a few
duplicated characters.
