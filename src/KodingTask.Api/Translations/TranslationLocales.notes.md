# TranslationLocales

`InterfaceLanguage` read once into plain lists: every code, the DeepL target of each
machine-translated language, and the parent of each regional variant.

`Match` is the server's twin of the web client's `matchLocale`, so the page and the server
agree on a browser's language: exact code, then traditional Chinese for Taiwan, Hong Kong,
Macau and the `Hant` script, then the first language with the same prefix (`de-AT` →
`de-DE`). Keep the two in step when a rule changes.
