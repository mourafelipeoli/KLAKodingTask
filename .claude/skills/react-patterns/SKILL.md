---
name: react-patterns
description: Conventions of the web client (src/kodingtask-web, React + TypeScript + Vite) - naming, folders, the single HTTP client, translations through useText, design-system components and the checks that must pass. Use when creating or editing any .ts, .tsx or .css file of the web client.
---

# React patterns

## Naming

| Element | Convention | Example |
|---|---|---|
| Component (file **and** function) | `PascalCase`; file name = component name | `ResultCard.tsx` → `function ResultCard()` |
| Custom hook | `camelCase` starting with `use` | `useTheme()` |
| Function / handler | `camelCase`; event handlers start with `handle` | `compare()`, `handleExample()` |
| Variable / parameter | `camelCase` | `requestedAmount` |
| Boolean | `is` / `has` prefix | `isComparisonOpen` |
| `useState` setter | `set` + state name | `[isOpen, setIsOpen]` |
| Interface / type | `PascalCase`, no `I` prefix, in `types/` | `ConversionResult` |
| Interface field | `camelCase`, matching the API JSON | `normalizedAmount` |
| Module constant | `UPPER_SNAKE_CASE` (or `PascalCase` for an `as const` object) | `ROUTES` |
| Service file | `camelCase` + `Service` | `conversionService.ts` |
| CSS class | BEM in `kebab-case` | `.result-card__words` |

ESLint enforces most of this (`npm run lint`).

## Folders

```
src/
  app/          application-wide state (instance info and feature flags)
  assets/       images and brand files
  components/   reusable UI; page-specific pieces in a subfolder (components/converter/)
  config/       static configuration (supported locales)
  hooks/        custom hooks
  i18n/         dictionaries, provider, useText, RichText
  pages/        one screen per file
  routes/       route paths and the route table
  services/     httpClient.ts + one file per service
  types/        interfaces of API data
  utils/        pure helpers
```

One component per file. Interfaces in `types/`, not inside services.

## API access

- **One HTTP client**: `services/httpClient.ts`. It owns `fetch`, headers, status checks,
  JSON parsing and error mapping (`ApiError` with the server's error code).
- **No `fetch` anywhere else**, never inside a component.
- Services return typed data, never a raw `Response`.
- Same origin only: the API host serves the client, so there are no base URLs or CORS.
- No secret ever reaches the client bundle.

## Text and translations

- Every visible string goes through `const { t } = useText()` and `t("English sentence")`.
  The English sentence is the key; dictionaries live in `src/i18n/dictionaries/`.
- Text chosen in one place and translated in another is marked with `textKey("…")` so
  `npm run check:i18n` sees it.
- A sentence with an element inside (a link) uses `<RichText text={t("… {{link}} …")} slots={{ link: … }} />`
  so each language keeps its own word order.
- Numbers and money are formatted with `Intl` for the UI locale.

## Components

- Use the shared pieces before writing new markup: `Notice`, `DeployedVersionDialog`,
  `CopyButton`, the `.button`, `.card`, `.field`, `.chip` classes.
- **Hand-over standard** (see `AGENTS.md`): an optional feature that is not configured stays
  usable. On a local copy the action opens `DeployedVersionDialog`, which continues on the
  deployed version with the same page and state (`useDeployedVersion`, `converterPath`),
  scrolled to the section that triggered it (`section` becomes the URL hash and must be the
  id of that section) and with the pending action in the query (`send=email`), so the
  deployed version can complete it. Never a passive notice, a greyed-out or a silent button.
- Declared state is used state. `useEffect` lists all dependencies. No synchronous
  `setState` inside an effect body: use event handlers, promise callbacks or transitions.
- No `any`, no `console.log` (`console.error` only for genuine failures), no `alert()` or
  `window.confirm`.
- Accessibility: labels for every control, `aria-live` for results, `role="alert"` for
  errors, visible focus, no information carried by colour alone.

## Before committing

- [ ] `npm run check:i18n`, `npm run lint`, `npm run format:check`, `npm run build` pass.
- [ ] New strings exist in every complete dictionary.
- [ ] Styles follow `styles-without-modules`.
