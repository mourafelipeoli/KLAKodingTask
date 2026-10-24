# Koding Task · Dollars in words

A web application that converts US dollar amounts from digits into words, written for the
KLA Senior Software Engineer coding task (web variant).

- **Live:** https://klakodingtask.mourafelipe.com
- **Languages:** English, Portuguese (Brazil), German, French (France, Canada, Switzerland,
  Belgium), Italian, Spanish, Polish, Chinese (Simplified and Traditional), Japanese and
  Korean, for both the interface and the converted words. The Asian languages are the ones on KLA's own website.
- **Stack:** ASP.NET Core 10 Web API (conversion on the server) + React 19 / TypeScript /
  Vite single-page app, served from the same origin.
- **Two engines:** the rules engine (deterministic, the reference) and an optional AI engine
  (Claude on Amazon Bedrock) whose answer is shown next to the rules engine's, with an
  "agrees / differs" badge.
- **Send it to yourself:** after signing in, a result can be emailed to the account's
  confirmed address or sent by WhatsApp to the number verified on fmID, within daily caps
  that live outside the code.
- **Your last conversions:** signed in, the converter lists your last ten conversions on any
  device; choose one to convert it again.
- **Sign in with fmID:** the author's own OpenID Connect provider (code flow with PKCE) is
  the only sign-in; this site stores no passwords.
- **Where it runs:** the hosted version has a live
  [infrastructure page](https://klakodingtask.mourafelipe.com/infrastructure) (container,
  traffic, database, certificate, guardrails) that never shows an identifier.

```
0                → zero dollars
1                → one dollar
25,1             → twenty-five dollars and ten cents
0,01             → zero dollars and one cent
45 100           → forty-five thousand one hundred dollars
999 999 999,99   → nine hundred ninety-nine million nine hundred ninety-nine thousand
                   nine hundred ninety-nine dollars and ninety-nine cents
```

## Run it

Prerequisites: the [.NET 10 SDK](https://dotnet.microsoft.com/download) and
[Node.js 22 LTS](https://nodejs.org) or newer. Nothing else: no database server, no cloud
account, no API key.

```sh
git clone <this repository>
cd KLAKodingTask
dotnet dev-certs https --trust          # once per machine, for https://localhost
dotnet run --project src/KodingTask.Api
```

Open **https://localhost:7162**. The first run installs the web client's npm packages
(about a minute), creates a local SQLite database in `src/KodingTask.Api/App_Data/`, and
starts the Vite dev server behind the API host. Conversion in every language, the
comparison across languages, the light theme (the default, as on kla.com) and the dark one
all work offline.

The optional features that need credentials (sending the result by e-mail or WhatsApp,
the AI conversion engine, sign-in with an external identity provider) still respond when
used: a dialog in the selected language explains that the result is on
the deployed version and opens the same page there, with the same amount, language and
engine, scrolled to the section that asked for it, where the deployed version completes
the action. This hand-over is the project's standard for every optional feature (see
`AGENTS.md`). To enable them on your own instance, see
[docs/configuration.md](docs/configuration.md).

### Tests and checks

```sh
dotnet test KodingTask.slnx              # converter golden tables + API tests (in memory)

cd src/kodingtask-web
npm run check:i18n                       # every dictionary is complete and consistent
npm run lint
npm run build
```

### The API on its own

```sh
curl -X POST https://localhost:7162/api/conversions \
     -H "Content-Type: application/json" \
     -d '{ "amount": "999 999 999,99", "language": "fr-BE" }'
```

```json
{
  "amount": "999 999 999,99",
  "normalizedAmount": "999 999 999,99",
  "dollars": 999999999,
  "cents": 99,
  "language": "fr-BE",
  "words": "neuf cent nonante-neuf millions neuf cent nonante-neuf mille neuf cent nonante-neuf dollars et nonante-neuf cents",
  "engine": "Deterministic"
}
```

All endpoints are described in the OpenAPI document at `/openapi/v1.json`, and
[`src/KodingTask.Api/KodingTask.Api.http`](src/KodingTask.Api/KodingTask.Api.http) has
ready-made requests.

## How the requirements are met

| Requirement | Where |
|---|---|
| Convert dollars from numbers into words, max 999 999 999 dollars and 99 cents, comma separator | [`AmountParser`](src/KodingTask.Conversion/AmountParser.cs), one speller per language in [`Languages/`](src/KodingTask.Conversion/Languages) |
| The six examples of the statement | [`OfficialExamplesTests`](tests/KodingTask.Conversion.Tests/OfficialExamplesTests.cs), in every language |
| At least two languages, selectable, output follows the selection | Fourteen output languages in five scripts; the output language follows the interface language and can be changed independently |
| .NET 8 or newer, ASP.NET backend | .NET 10, ASP.NET Core Web API |
| Client-server, conversion on the server | The browser only sends text and shows the answer; see [`ConversionsController`](src/KodingTask.Api/Controllers/ConversionsController.cs) |
| Modern frontend framework | React 19 + TypeScript + Vite |
| README, design decisions, assumptions, limitations | This file, [docs/](docs) and the `.notes.md` files next to the code |
| Prompt history of AI use | [docs/ai/prompt-history.md](docs/ai/prompt-history.md) |

## Architecture

```
browser ──HTTPS──► KodingTask.Api (ASP.NET Core 10)
                    ├─ /api/conversions …  ─► KodingTask.Conversion (pure C#, no I/O)
                    ├─ /api/app-info          instance info + optional feature flags
                    ├─ /api/infrastructure    read-only facts for the "Where this runs" page
                    ├─ /api/account …         the signed-in visitor (Identity)
                    ├─ /signin-fmid …         sign-in with fmID (OpenID Connect, code + PKCE)
                    ├─ /api/deliveries        a result by email (SES) or WhatsApp (Meta)
                    ├─ /api/account/conversions  the signed-in visitor's last conversions
                    ├─ /health                version and commit, never touches the DB
                    └─ everything else        the React app (static files; Vite in development)
                         │
                         └─ KodingTask.Data (EF Core: SQLite locally, MySQL when hosted)
```

| Project | Responsibility |
|---|---|
| [`src/KodingTask.Conversion`](src/KodingTask.Conversion) | The domain: parsing and spelling. No framework, no I/O, fully unit tested. |
| [`src/KodingTask.Data`](src/KodingTask.Data) | EF Core context for the optional features; SQLite or MySQL by configuration. |
| [`src/KodingTask.Api`](src/KodingTask.Api) | HTTP API, hosting concerns (health, security headers, rate limits) and the SPA host. |
| [`src/kodingtask-web`](src/kodingtask-web) | React client: UI, translations, themes. |
| [`tests/`](tests) | xUnit: converter golden tables and API tests through the real pipeline. |

Each C# file has a sibling `*.notes.md` that explains its decisions (Visual Studio nests it
under the file). That is where to look for the "why" of any class.

## Key design decisions

- **A hand-written parser with precise errors.** The input format is the task's own:
  digits grouped by spaces, a comma before one or two cent digits. Every refusal has a code
  (`InvalidGrouping`, `DecimalPointNotAllowed`, …) that the UI translates into an
  actionable message. See [`AmountParser.notes.md`](src/KodingTask.Conversion/AmountParser.notes.md).
- **One speller class per language, whole sentence included.** Where the number meets the
  currency each language has its own grammar (German "ein Dollar" vs "einhunderteins
  Dollar", Portuguese "um milhão de dólares", French "zéro dollar", Italian elisions), so a
  speller owns the complete sentence. Adding a language is one class and one line in
  [`SpellerCatalog`](src/KodingTask.Conversion/SpellerCatalog.cs).
- **Counting in myriads where the language does.** Chinese, Japanese and Korean name
  ten-thousands (万/萬/만) and hundred-millions (亿/億/억), so 45 100 is "four myriads, five
  thousand one hundred". They share the split, not the rules: Chinese marks gaps with 零 and
  says 两 for two before a measure, Japanese and Korean skip gaps silently, Korean puts a
  space after each myriad group.
- **Regional French as data, not copies.** France and Canada say *quatre-vingt-dix*,
  Belgium and Switzerland *nonante*, and Switzerland also *huitante*; one `FrenchSpeller`
  takes the numbering as a constructor argument.
- **Refusals are values, not exceptions**, and the API has one error shape:
  `400 { "error": "<code>", "message": "<English sentence>" }`.
- **The server is the single source of truth for languages**: the client builds its menu
  from `GET /api/conversions/languages`.
- **One origin in development and production.** The API host serves the React app; in
  development it starts Vite and forwards to it. No CORS, and cookies, redirects and the
  content security policy behave the same in both.
- **Optional features never block the core.** Each one reports whether it is configured;
  a fresh clone has all of them off and still converts in every language.
- **A new language is one enum value.** `InterfaceLanguage` on the server is the list of
  languages; the client's menu, the dictionaries it loads and the server's own emails and
  error messages follow it. Every text is a term, machine-translated by DeepL in the
  background and reviewed on `/admin/translations`. The dictionaries for the fourteen
  languages also ship with the app (checked by `npm run check:i18n`), so the UI is fully
  translated offline. See [decision 11](docs/decisions/0011-a-new-language-is-one-enum-value.md).
- **The AI engine never replaces the rules engine.** It answers the same validated amount
  (nothing the visitor types reaches the model), its few-shot examples are generated by the
  rules engine, and every call passes a kill switch, a monthly budget, a daily cap, a
  per-client cap and a concurrency gate before it is made. The caps live in a parameter
  store, outside the code. See
  [`AiConversionService.notes.md`](src/KodingTask.Api/Ai/AiConversionService.notes.md).

- **The infrastructure page reads, never reveals.** Facts come from the ECS task metadata
  and a few read-only AWS calls, are cached for five minutes and are copied field by field
  into a whitelisted report; a test proves that no ARN, account id or resource name leaks.
  See [decision 5](docs/decisions/0005-infrastructure-page-from-read-only-facts.md).

- **Messages only reach their own owner.** No recipient field: email goes to the account's
  confirmed address, WhatsApp to the number the identity provider verified (checked again
  within ten minutes of every send), and the words are converted again on the server.
  Daily caps per channel, per account and per client live in the parameter store. See
  [decision 6](docs/decisions/0006-accounts-and-deliveries-to-yourself.md) and
  [decision 10](docs/decisions/0010-the-whatsapp-number-comes-from-fmid.md).

- **An external identity, a local account, no passwords.** fmID is the only sign-in: a
  confidential server-side OpenID Connect client whose tokens never reach the browser. The
  provider's subject is linked to a local account only through a verified address, and
  sign-up, address confirmation and password resets are the provider's. See
  [decision 7](docs/decisions/0007-sign-in-with-fmid-linked-to-local-accounts.md) and
  [decision 8](docs/decisions/0008-fmid-is-the-only-sign-in.md).

- **Last conversions belong to the account.** Only a signed-in visitor's conversions are
  kept, recorded by the conversion endpoint itself from its own result, ten per person at
  most and never older than 90 days. See
  [decision 9](docs/decisions/0009-last-conversions-belong-to-the-account.md).

More decisions, with their alternatives, are recorded in [docs/decisions](docs/decisions).

## Assumptions

- **`,1` means ten cents.** The task's example `25,1 → … and ten cents` fixes it: one cent
  digit is a tenth.
- **Spaces group thousands, in blocks of three.** `45 100` and `1 000 000` are valid;
  `4 5100` is refused rather than guessed. Non-breaking and thin spaces count as spaces.
- **A point is never a decimal separator.** `25.10` is refused with a hint to use a comma,
  because `1.000` means one thousand in German and one in English.
- **American English style**: no "and" inside the number ("one hundred one"), hyphens for
  21-99, as in the task's examples.
- **Zero is plural in English, Portuguese and Italian, singular in French** ("zero
  dollars", "zéro dollar"), following each language's grammar.
- **The cent is "cent" in English and French, "Cent" in German, "centavo" in Portuguese and
  "centesimo" in Italian.**
- The full rules per language, with examples, are in [docs/languages.md](docs/languages.md).

## Limitations and known issues

- Only US dollars, as the task specifies; the currency is not a parameter.
- Amounts must be typed in the task's format; locale-specific input such as `1.000,50` or
  `1,000.50` is refused with a precise hint instead of being interpreted.
- German amounts ending in one above one hundred use the uninflected written form
  ("einhunderteins Dollar"); the inflected "hundertundein Dollar" is also correct German.
- The local SQLite database is recreated automatically when the data model changes; it
  holds nothing worth keeping on a development machine.
- A language added to `InterfaceLanguage` without a speller translates the interface but
  not the amounts: the converter writes the closest language it has, English when there is
  none. Machine translations are live before anyone reviews them.

## Repository layout

```
src/            application code (see Architecture)
tests/          xUnit test projects
docs/           configuration, languages, decisions, AI prompt history
scripts/checks/ encoding and private-terms checks used by the git hooks and CI
.githooks/      pre-commit and commit-msg hooks (git config core.hooksPath .githooks)
.claude/skills/ conventions for AI coding agents (also readable by humans)
design/         the Koding Task logos, icons and fonts
versions.json   the application version, shown by /health and in the footer
```
