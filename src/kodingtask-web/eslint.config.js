import js from "@eslint/js";
import globals from "globals";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import tseslint from "typescript-eslint";
import { defineConfig, globalIgnores } from "eslint/config";

export default defineConfig([
  globalIgnores(["dist", "scripts"]),
  {
    files: ["**/*.{ts,tsx}"],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommendedTypeChecked,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      ecmaVersion: 2022,
      globals: globals.browser,
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
    rules: {
      // Naming conventions (see .claude/skills/react-patterns).
      "@typescript-eslint/naming-convention": [
        "error",
        { selector: "default", format: ["camelCase"], leadingUnderscore: "allow" },
        { selector: "variable", format: ["camelCase", "UPPER_CASE"] },
        // Module constants may also be PascalCase: the `as const` object that replaces a
        // TypeScript enum, and React components assigned to constants.
        {
          selector: "variable",
          modifiers: ["const", "global"],
          format: ["camelCase", "UPPER_CASE", "PascalCase"],
        },
        { selector: "parameter", format: ["camelCase"], leadingUnderscore: "allow" },
        { selector: "function", format: ["camelCase", "PascalCase"] },
        { selector: "typeLike", format: ["PascalCase"] },
        { selector: "typeParameter", format: ["PascalCase"] },
        // Fields must match the JSON of the API (camelCase); UPPER_CASE for VITE_* env vars.
        {
          selector: "typeProperty",
          format: ["camelCase", "UPPER_CASE"],
          leadingUnderscore: "allow",
        },
        // Object literal keys are free: HTTP headers, locale codes, payloads.
        { selector: "objectLiteralProperty", format: null },
        { selector: "import", format: ["camelCase", "PascalCase"] },
      ],
      // Passing an async function to onClick/onSubmit is an accepted React pattern.
      "@typescript-eslint/no-misused-promises": [
        "error",
        { checksVoidReturn: { attributes: false } },
      ],
      "no-console": ["error", { allow: ["error"] }],
    },
  },
]);
