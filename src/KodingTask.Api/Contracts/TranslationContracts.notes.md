# TranslationContracts

Requests and answers of `/api/i18n` and `/api/admin/translations`. The dictionary is a flat
map from the English text to its translation, the same shape as the bundled JSON files, so
the client merges a served dictionary over a bundled one without converting anything.
