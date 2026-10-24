# InterfaceLanguage

The list of interface languages, and the whole point of the translation system: **adding a
member is all it takes to offer a new language.**

```csharp
[Language("nl-NL", "Nederlands")]
DutchNetherlands,
```

After a deploy with that line:

1. `GET /api/i18n/languages` lists it, so the web client's language menu shows it. The
   client has no list of its own beyond a fallback copy for when the server cannot be
   reached.
2. The next run of `TranslationBatch` finds every active term without a Dutch translation
   and sends them to DeepL in groups of 50 (about 7,200 characters for the whole
   application, once).
3. `GET /api/i18n/nl-NL` serves the result; the client shows it, and the server's own texts
   (emails, error messages) answer in Dutch through `ServerTexts`.
4. An administrator reviews the machine translations on `/admin/translations` and may
   export them as `nl-NL.json` to bundle them with the code.

The idea comes from the author's earlier implementation in another application (a locale
enum, terms registered the first time the code shows them, a background batch that
translates the missing ones). This one keeps the idea and changes the details; decision 11
lists what changed and why.

## The attribute

- `Code` is a BCP 47 tag, the same code the converter uses for its output languages.
- `NativeName` is what the menu shows, in the language itself.
- `DeepL` is only needed when the rule in `InterfaceLanguageInfo` gives the wrong target:
  DeepL wants `EN-US`/`PT-BR` for English and Portuguese, `ZH-HANS` or `ZH-HANT` for
  Chinese, and the bare language for everything else (`DE`, `NL`, `SV`…). Traditional
  Chinese is the only override today.
- `VariantOf` marks a regional variant (`fr-CA` of `fr-FR`): it inherits every text of its
  parent and only stores what differs, so it is never machine-translated (DeepL has no
  Canadian French; it would only produce France's French again).

Reflection runs once (`TranslationLocales` caches the result), and a member without the
attribute fails at startup rather than at the first request.
