// Checks the bundled UI dictionaries (npm run check:i18n):
// - en-US.json is the source: every key maps to itself;
// - every other locale except the regional French ones translates every key;
// - fr-CA, fr-CH and fr-BE only override fr-FR, so they may hold a subset;
// - no dictionary has unknown keys, empty values or different {{placeholders}};
// - every literal t("…") / textKey("…") in the code exists in en-US.json, and every
//   en-US.json key is used somewhere.
import { readFileSync, readdirSync, statSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const dictionaryFolder = join(root, "src", "i18n", "dictionaries");
const COMPLETE = [
  "pt-BR",
  "de-DE",
  "fr-FR",
  "it-IT",
  "es-ES",
  "pl-PL",
  "zh-CN",
  "zh-TW",
  "ja-JP",
  "ko-KR",
];
const REGIONAL = ["fr-CA", "fr-CH", "fr-BE"];

const read = (locale) =>
  JSON.parse(readFileSync(join(dictionaryFolder, `${locale}.json`), "utf8"));
const placeholders = (text) =>
  [...text.matchAll(/\{\{(\w+)\}\}/g)]
    .map((m) => m[1])
    .sort()
    .join(",");

const problems = [];
const source = read("en-US");
const sourceKeys = new Set(Object.keys(source));

for (const [key, value] of Object.entries(source)) {
  if (key !== value) problems.push(`en-US: "${key}" must map to itself`);
}

for (const locale of [...COMPLETE, ...REGIONAL]) {
  const dictionary = read(locale);
  for (const [key, value] of Object.entries(dictionary)) {
    if (!sourceKeys.has(key)) problems.push(`${locale}: unknown key "${key}"`);
    if (!value.trim()) problems.push(`${locale}: empty translation for "${key}"`);
    if (placeholders(key) !== placeholders(value)) {
      problems.push(`${locale}: placeholders differ in "${key}"`);
    }
  }
  if (COMPLETE.includes(locale)) {
    for (const key of sourceKeys) {
      if (!(key in dictionary)) problems.push(`${locale}: missing "${key}"`);
    }
  }
}

function sourceFiles(folder) {
  return readdirSync(folder).flatMap((name) => {
    const path = join(folder, name);
    if (statSync(path).isDirectory()) return sourceFiles(path);
    return /\.(ts|tsx)$/.test(name) ? [path] : [];
  });
}

const used = new Set();
for (const file of sourceFiles(join(root, "src"))) {
  const text = readFileSync(file, "utf8");
  for (const match of text.matchAll(/\b(?:t|textKey)\(\s*"((?:[^"\\]|\\.)*)"/g)) {
    used.add(JSON.parse(`"${match[1]}"`));
  }
}
for (const key of used) {
  if (!sourceKeys.has(key))
    problems.push(`code uses "${key}" but en-US.json does not define it`);
}
for (const key of sourceKeys) {
  if (!used.has(key)) problems.push(`en-US.json defines "${key}" but no code uses it`);
}

if (problems.length > 0) {
  console.error(`Dictionary problems (${problems.length}):`);
  for (const problem of problems) console.error(`  ${problem}`);
  process.exit(1);
}
console.log(
  `Dictionaries OK: ${sourceKeys.size} keys, ${COMPLETE.length + REGIONAL.length + 1} locales.`,
);
