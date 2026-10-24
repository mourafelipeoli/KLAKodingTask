# UserProfiles

Remembers the interface language each account last used, and answers it. The language
must be one `InterfaceLanguage` declares (`TranslationLocales.Canonical`, which also fixes
its case); anything unknown keeps the previous choice, or English. Interface languages are
not tied to conversion engines any more: a language added for the interface may have no
speller, and its emails are still written in it.
