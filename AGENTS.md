# Koding Task

Instructions for AI coding agents working in this repository. Humans: start with
[README.md](README.md).

- Read `.claude/skills/linked-notes/SKILL.md` when creating or editing C#.
- Read `.claude/skills/task-closure/SKILL.md` before declaring a functional change done.
- Read `.claude/skills/react-patterns/SKILL.md` and
  `.claude/skills/styles-without-modules/SKILL.md` for anything under `src/kodingtask-web`.
- Everything is in English: code, notes, docs, UI source texts, commit messages.
- **This repository is public.** Never write secrets, credentials, account ids,
  infrastructure identifiers (ARNs, cluster or database names) or names of private systems
  anywhere, including commit messages. The pre-commit hook enforces the maintainer's list;
  never bypass it with `--no-verify`.
- **Prompt history is a deliverable.** The task requires the AI prompt history: append the
  user's prompts to `docs/ai/prompt-history.md` (see `task-closure`).
- `/health` is exact, anonymous, GET/HEAD only, first in the pipeline and never touches the
  database or any external service.
- Conversion is server-side and pure (`src/KodingTask.Conversion`); keep it free of I/O.
- Optional features (email, WhatsApp, AI, machine translation, external login) must keep
  the application fully usable when they are not configured: a fresh clone runs with no
  credentials at all.
- **Hand-over standard.** A copy that lacks an optional feature never shows a passive
  "use the published version" notice and never greys the action out. The action stays
  usable; using it opens `DeployedVersionDialog`, which continues on the deployed version
  with the same page and state (amount, output language, engine, interface language), lands
  on the section that triggered it, and the deployed version completes the action there.
  Build every new optional feature this way (`useDeployedVersion`, `converterPath`).
- Migrations are MySQL-only, never run at startup, and are applied by an operator:
  `dotnet ef migrations add <Name> --project src/KodingTask.Data --startup-project src/KodingTask.Api`.
  SQLite (local default) is created with `EnsureCreated()`.
- Run locally: `dotnet run --project src/KodingTask.Api` → `https://localhost:7162`. Tests:
  `dotnet test KodingTask.slnx`. Never start the host against a production database.
- Deployment is manual and done by the maintainer; do not push or deploy.
