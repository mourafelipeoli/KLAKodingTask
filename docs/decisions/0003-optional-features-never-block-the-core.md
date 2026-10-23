# 3. Optional features never block the core

Date: 2026-10-23 · Status: accepted

## Context

Beyond the task, this project sends results by e-mail and WhatsApp, offers an AI
conversion engine, machine-translates the interface and supports an external identity
provider. All of them need credentials that a reviewer cloning the repository does not have.
The reviewer must still be able to run the application and test everything the task asks
for, and a team adopting the code must be able to switch every feature on.

## Decision

- Each optional feature registers itself only when its configuration is complete, and
  reports that in `FeatureAvailability` (`GET /api/app-info`).
- The client shows unavailable features with a translated message pointing to the hosted
  version, instead of hiding them or failing.
- The local defaults need no service at all: SQLite, bundled interface dictionaries, no
  external calls.
- Every setting is documented in `docs/configuration.md`.

## Consequences

- `git clone` + `dotnet run` gives a complete converter in eight languages.
- The hosted version shows the full feature set; the code is identical.
- Each feature carries its own "is configured" logic, tested with no configuration at all.
