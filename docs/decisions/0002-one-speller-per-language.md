# 2. One speller per language, owning the whole sentence

Date: 2026-10-23 · Status: accepted

## Context

A common design for number-to-words is a shared engine with per-language word tables.
That works for the digits, but the currency sentence differs structurally between
languages: German inflects "one" depending on what follows, Portuguese inserts "de" after
an exact million and joins groups with "e" under specific conditions, French drops the
plural "s" of "cents" before "mille" but keeps it before "millions", Italian elides vowels
and accents a final "tre".

## Decision

`ICurrencySpeller.Spell(MonetaryAmount)` returns the complete sentence. Each language has
its own class with its own small helpers (`UpTo99`, `UpTo999`, `Cardinal`). Only the split
into millions, thousands and units is shared (`NumberGroups`). Regional variants that differ
only in numbering (French) are one class with a constructor argument.

## Consequences

- Each speller reads top to bottom as the rules of one language; a native speaker can review
  it without learning a framework.
- Some structure repeats between classes (each has an `UpTo999`). That repetition is
  deliberate: the bodies differ in exactly the places that matter.
- Adding a language means one new class and one line in `SpellerCatalog.Default`.

## Alternatives considered

- **Table-driven engine with flags** ("hasConjunction", "elidesVowels", …): the flags
  multiply with every language and the rules become hard to see.
- **A third-party library** (Humanizer and similar): no regional French numbering, no control
  over the currency sentence, and the task is to write the conversion.
