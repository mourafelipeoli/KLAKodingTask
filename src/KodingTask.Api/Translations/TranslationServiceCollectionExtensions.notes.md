# TranslationServiceCollectionExtensions

Registers the translation system. Everything except the DeepL calls works without
configuration: the dictionaries, the review page, term registration and the server's
translated texts. The return value (`DeepL:ApiKey` set or not) becomes
`FeatureAvailability.MachineTranslation`, which the batch loop and the "Translate now"
action check.

Singletons hold the caches and queues (`TranslationIndex`, `PendingTerms`,
`TranslationBatchSignal`) so every request and the background loop share them; the classes
that use the database are scoped.
