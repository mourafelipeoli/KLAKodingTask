# DeepLTranslator

The DeepL client (`POST /v2/translate`), behind `IMachineTranslator` so tests and other
providers can replace it.

- **Up to 50 texts per request**, DeepL's limit, instead of one request per text: fewer
  round trips and fewer rate-limit answers.
- **English is declared as the source** (`source_lang=EN`). Detection guesses wrong on short
  texts such as "Show" or "Missing".
- **Placeholders are protected**: `{{amount}}` travels as `<x>{{amount}}</x>` with
  `tag_handling=xml` and `ignore_tags=x`, so DeepL neither translates nor drops it (see
  `Placeholders`).
- **Retries**: 429 and 5xx wait 1, 3, 8 and 20 s (`DeepL:RetryDelaysSeconds`), or what
  `Retry-After` asks for; a wait longer than a minute gives up until the next run instead
  of holding the loop. 456 (quota used up) and other 4xx stop at once.
- **Free or Pro** is chosen from the key: free keys end in `:fx` and use
  `api-free.deepl.com`. `DeepL:BaseUrl` overrides both.
- The key travels only in the `Authorization` header; it is never logged.
