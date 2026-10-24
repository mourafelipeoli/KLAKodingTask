# BakedDictionaries

The dictionaries that ship with the code, embedded in the assembly:

- the web client's (`src/kodingtask-web/src/i18n/dictionaries/*.json`, linked into this
  project as embedded resources), so the server and the client import the very same files;
- the server's own (`Messaging/Texts/*.json`: the email texts).

They play three roles: the source of the `Code` terms (`SourceKeys`, the English files),
reviewed translations imported by `TranslationSeeder`, and the fallback of `ServerTexts`
when the database has nothing yet (a fresh clone, or the first seconds after a start).

`ByLocale` merges both kinds per language; the interface entry wins when both define the
same key, which never happens today. Entries keep the files' order so an export can keep it.
