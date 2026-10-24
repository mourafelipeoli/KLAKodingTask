# TranslationAdminController

`/api/admin/translations`, for the addresses in `Admin:Emails` only (policy `Admin`), with
antiforgery on every change.

| Endpoint | What it does |
|---|---|
| `GET ?locale=` | Every active term with its translation in that language, where it came from, whether it is reviewed and, for a regional variant, the parent's text it inherits. |
| `PUT` | Saves one translation. A changed text becomes `Manual`, which the bundled import never overwrites; an empty text removes a regional override so the variant inherits again (an empty text for a full language is refused). |
| `POST run` | Wakes the batch now; 503 when DeepL is not configured. |
| `GET export/{locale}` | The language as JSON in the bundled dictionaries' format and order, ready to replace the file in the repository after a review. |

English is not editable: an English text is the term itself, and it changes in the code.

On a copy without DeepL, "Translate now" answers 503 with a message instead of handing over
to the deployed version: the review page is an operator's tool on their own instance, and
the deployed version's review page is only open to its own administrators.
