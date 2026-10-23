---
name: styles-without-modules
description: CSS conventions of the web client. CSS Modules (*.module.css) are forbidden; styles are global CSS with BEM names, colours and sizes only through the semantic tokens in index.css. Use when creating or editing any component, page or stylesheet of the web client, and whenever tempted to create a Name.module.css.
---

# Styles without modules

## The rule

**No `*.module.css` file enters this repository.** Styles are global CSS with BEM class
names, in `src/index.css` or in a stylesheet co-located with a large component
(`SiteHeader.css`, `ConvertPage.css`), imported with `import "./SiteHeader.css"`.

## Why

- Leaks between screens come from **bare element selectors** (`form {}`, `input {}`), not
  from class names that happen to repeat. A module does not stop `input {}` from leaking;
  a BEM block does, because the block name is the scope.
- Modules hide duplication: two near-identical buttons in two files never show up in the
  same search. With global BEM, `grep -rn "result-card" src` finds the CSS and the JSX.
- Hashed class names (`_button_1x9k4_12`) tell nothing in the browser's inspector;
  `.result-card__words` tells which rule to edit.
- Theming is already solved by tokens on `:root`; modules would add a second layer of
  scoping on top of one that works.

## Where a style goes

| Situation | Where |
|---|---|
| Tokens, reset, layout, anything used by more than one screen | `src/index.css` |
| A component with a large style block of its own | `Component.css` next to it |
| A few rules for one screen | the screen's stylesheet (`ConvertPage.css`) |

**Search before writing a new class:** `grep -rn "block-name" src --include=*.css`.

## BEM

`block__element--modifier`, in kebab-case. The block name comes from the component name.

```css
.result-card { }
.result-card__words { }
.button--primary { }
```

Non-negotiable:

- **One block per component or screen**; two blocks with the same name in different
  screens are exactly the leak this avoids.
- **No bare element selectors** outside the reset section of `index.css`.
- **No `!important`.** If specificity does not work out, the selector is wrong or a
  modifier is missing. Reduced motion is handled by zeroing the duration tokens, not by
  overriding with `!important`.
- **Colours only through semantic tokens**: `var(--color-action)`, never `#0b6fb3`, and
  never a primitive (`var(--blue-600)`) outside the semantic layer of `:root`. Space,
  radius, shadow, type and motion also come from tokens. A hex value outside `:root` is
  the start of a second theme.
- **Every theme-dependent value is a token** redefined under `:root[data-theme="dark"]`,
  so a component never needs its own dark-mode rule.
- **Grids use `minmax(0, 1fr)` tracks**, so a wide control (an input, a select) cannot push
  the page sideways on a phone.

## Found a .module.css?

Convert it: rename classes to BEM, move shared rules to `index.css` (often they already
exist there under another name, in which case delete instead of moving), replace literal
colours with tokens, delete the module, and run `npm run lint` and `npm run build`.
Preserve the values that were tuned against a real screen; change only the scoping
mechanism.
