# LanguageInfo

Identity of an output language.

`Code` is a BCP 47 tag with region (`fr-BE`, not `fr`), because the region changes the
result: Belgian and Swiss French say *nonante*, France and Canada say *quatre-vingt-dix*.
The same tags are used by the UI for its own translations, so one code means one thing in
the whole application.

`NativeName` is what a speaker of that language expects to see in a language menu
("Français (Belgique)"); `EnglishName` serves logs, documentation and the API listing.
