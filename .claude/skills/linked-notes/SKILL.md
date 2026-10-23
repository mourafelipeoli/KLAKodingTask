---
name: linked-notes
description: C# decisions live in sibling *.notes.md files nested under their source file, not in code comments. Use when creating, editing, moving or deleting any .cs file in this repository.
---

# Linked notes

Every authored C# file that is new or changed gets a sibling note. Explanations, decisions,
trade-offs and pitfalls go in the note; the code carries no explanatory comments. When a
comment already exists in code you touch, move its reasoning into the note instead of
deleting it. Adoption is incremental: do not rewrite files outside the task.

| Source | Note |
| --- | --- |
| `Class.cs` | `Class.notes.md` |
| `Page.cshtml` and `Page.cshtml.cs` | `Page.cshtml.notes.md`, shared |

## What a note contains

- A title (the type name).
- Why the code is the way it is: the decision, the alternative that was rejected and why,
  the constraint that is not visible in the code (a contract, a deploy order, a limit).
- Only the sections that are useful. A three-line note is fine for a small type; an empty
  one is not.

Not in the note: a line-by-line narration of the code, anything secret, names of private
systems.

## Rules

- A removed source file takes its note with it; a renamed one renames the note.
- Generated code (EF migrations and their designer files, scaffolding) has no note.
- CSS, JavaScript, TypeScript and HTML markup comments are outside this rule (the web client
  has its own conventions; see `react-patterns`).
- `#region` may stay.

## How the nesting works

`Directory.Build.targets` declares, once for every C# project, the two `DependentUpon` rules
that nest `X.notes.md` under `X.cs` (and `Page.cshtml.notes.md` under `Page.cshtml`) in
Visual Studio's Solution Explorer. A new project needs nothing extra.

## Why

Code comments age silently and clutter the code a reviewer reads first. A sibling note is
one click away, survives refactors that move code around, and can hold the context
(decisions, history, contracts) that would be noise inline.
