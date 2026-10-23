# UserProfiles

Remembers the interface language each account last used, and answers it. The interface
locales are the same codes as the output languages, so `SpellerCatalog` validates them
(and turns `fr_be` into `fr-BE`); anything unknown keeps the previous choice, or English.
