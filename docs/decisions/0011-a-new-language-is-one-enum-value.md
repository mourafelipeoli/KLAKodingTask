# 11. A new language is one enum value

Date: 2026-10-24 · Status: accepted

## Context

The interface was translated by JSON dictionaries bundled with the web client, and the
server's emails by a second set of JSON files. Adding a language meant writing two sets of
files by hand and touching the client's language list, and the server's error messages
were only in English.

The author designed a translation system in an earlier application that removes that
work: a locale enum, every text registered as a term the first time the code shows it, and
a background batch that machine-translates the missing ones. This project implements the
same idea as a proof of concept, across the TypeScript client and the C# server.

## Decision

- **The `InterfaceLanguage` enum is the list of languages.** Each member carries its code,
  its native name and, when needed, its DeepL target or the language it is a regional
  variant of. The web client builds its menu from `GET /api/i18n/languages`; its own list is
  only a fallback for when the server cannot be reached.
- **English text is the key**, in the code (`t("Sign in")`) and on the server
  (`ServerTexts.Translate("Your Koding Task result", …)`). Every text is a row in `Terms`,
  every translation a row in `Translations` with its source (`Baked`, `Machine`, `Manual`)
  and a `Reviewed` flag.
- **Three ways a text becomes a term**: the bundled dictionaries are imported on every
  start; the web client reports texts no dictionary knows; the server queues texts it used
  without knowing them. Both reports end in the same table.
- **A background batch translates** every active term missing in a machine-translated
  language, through DeepL, when a key is configured. Without a key, everything else works and
  the bundled dictionaries keep the application fully translated.
- **Dictionaries are served per language** with an ETag; the client merges the served one
  over the bundled one and keeps the last copy in the browser.
- **The server speaks the visitor's language** with the same terms: emails, and the message
  of every API error.
- **An administrator reviews** on `/admin/translations` (addresses in `Admin:Emails`):
  edit, mark reviewed, translate now, and export a language in the bundled files' format so
  the reviewed texts can be committed.

## What changed from the earlier implementation

The idea is unchanged; these are the refinements, each to a problem the earlier version
had or would have here:

| Earlier | Here | Why |
|---|---|---|
| The enum value plus a separate mapping to the provider's language code | The enum value only; the DeepL code follows a rule, with an override in the attribute | One edit adds a language. |
| One DeepL request per text, with a fixed pause | Up to 50 texts per request, backoff on 429 and 5xx honouring `Retry-After`, a clean stop on 456 (quota) | Fewer requests, and rate limits handled instead of avoided by sleeping. |
| Placeholders sent as plain text, source language detected | `{{name}}` wrapped in ignored XML tags, `source_lang=EN` | DeepL can neither translate nor drop a placeholder, and short texts are not misdetected. |
| A translation is just text | `Source` and `Reviewed` per translation; hand edits survive the bundled import | Machine output is visible as such until someone checks it. |
| Terms keyed by a shortened, normalised text | Keyed by the SHA-256 of the full text, unique | Two long texts with the same beginning stay two terms. |
| Dictionaries cached in each instance until restart | Five-minute cache with background refresh, ETag to the browser | Several instances converge without a restart or a message bus. |
| The registration endpoint open without limits | Size limits, antiforgery, a rate limit and two daily caps (instance, client); DeepL characters capped per day; kill switch | The endpoint is public and the characters are paid. |
| Regional variants translated separately | Variants inherit their parent and store only overrides; never machine-translated | DeepL has no regional French; it would only repeat France's. |
| Review page open to any signed-in user | Restricted to configured administrator addresses | Anyone can sign in here. |

## Consequences

- Adding `[Language("nl-NL", "Nederlands")] DutchNetherlands` and deploying is enough: the
  menu lists Dutch at once, and the next batch run (within two minutes, or on "Translate
  now") translates about 7,200 characters, the whole application.
- A language added only for the interface has no conversion engine; the converter then
  writes the closest language it has (English when there is none). Spellers are code, by
  decision 2.
- `npm run check:i18n` still requires every text of the code in the bundled English file, so
  the bundled dictionaries stay a complete offline fallback; runtime reports catch what slips
  through and texts that never pass through `t` literally.
- Machine translations go live before review. That is the point of the proof of concept;
  `Reviewed` and the review page make it visible, and the export makes the reviewed state
  part of the code.

## Alternatives considered

- **Keep hand-written JSON only**: no runtime cost, but every language is a manual project
  and the server stays English.
- **Translate at build time** with a script that calls DeepL: no database, but a deploy for
  every new text, and no review loop in the running application.
- **A third-party localisation platform**: mature review tools, but an external account and
  workflow for a proof of concept whose point is that the application does it itself.
