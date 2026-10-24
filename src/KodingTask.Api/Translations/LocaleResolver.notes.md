# LocaleResolver

The language of a request, in the same order as the web client: `?lang=` in the address,
then the `kodingtask-lang` cookie (written by the client when the visitor picks a
language, or once it detects one), then `Accept-Language` by quality, then English. Every
candidate goes through `TranslationLocales.Match`, so `de-AT` answers in German.
