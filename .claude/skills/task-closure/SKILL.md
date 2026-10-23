---
name: task-closure
description: Checklist before declaring a functional change done in this repository - build and tests without real services, linked notes, docs, prompt history, privacy and secret checks, and an honest status (prepared / committed / published). Use before finishing any functional change.
---

# Task closure

1. **Build and test without touching real services.**
   - `dotnet build KodingTask.slnx` and `dotnet test KodingTask.slnx` (tests use in-memory
     hosts and temporary SQLite files; they never call AWS, Meta, DeepL or a real identity
     provider).
   - If the web client changed: in `src/kodingtask-web`, `npm run check:i18n`,
     `npm run lint`, `npm run format:check` and `npm run build`.
   - Never start the host against a production database to "check that it compiles".
2. **Notes.** Update the sibling `.notes.md` of every C# file touched (see `linked-notes`).
3. **Docs.** Update `README.md` (the map), `docs/configuration.md` when a setting changes,
   `docs/infrastructure.md` when a contract with the hosting environment changes, and the
   ADRs in `docs/decisions/` when a decision is made or reversed. Keep the reason and the
   cost of a reverted decision; do not erase history.
4. **Translations.** Every new UI text goes into `en-US.json` and is translated into all
   complete dictionaries (`pt-BR`, `de-DE`, `fr-FR`, `it-IT`); regional French files only
   override what differs.
5. **Prompt history.** Append the user's prompts for this task to
   `docs/ai/prompt-history.md`, verbatim and in English. If a prompt names a private system
   or company, propose a neutral wording to the user before appending.
6. **Privacy and secrets.** The repository is public.
   - No credentials, tokens, keys, certificates, connection strings with passwords, account
     ids, ARNs, infrastructure names, or personal data in code, notes, docs, tests, logs or
     commit messages.
   - The pre-commit hook runs `scripts/checks/check-encoding.py` and
     `scripts/checks/check-private-terms.py`; never bypass it.
   - Runtime secrets come from the environment (user secrets locally, a secrets manager in
     the hosted deployment).
7. **Report the state honestly.** Distinguish *prepared* (changed locally), *committed*,
   and *published* (deployed and verified). A deploy is verified by `/health` returning the
   expected version and commit, not by a green build. A healthy rollback is not a successful
   deploy of the new version.
