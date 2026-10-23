# 1. Server-side conversion in a pure library

Date: 2026-10-23 · Status: accepted

## Context

The task requires client-server architecture with the conversion on the server, and values
clean, well-structured code.

## Decision

The conversion lives in `KodingTask.Conversion`, a class library with no dependency on
ASP.NET Core, Entity Framework or any I/O. The API project only adapts HTTP to it
(`ConversionsController`) and the client only sends text and shows the answer.

## Consequences

- The domain is tested in isolation with fast, deterministic unit tests (golden tables per
  language plus a sweep over thousands of generated amounts).
- The same library could serve a desktop client, a CLI or a queue consumer unchanged.
- The API stays thin: validation results come back as values (`ConversionResult`), and the
  controller maps them to `200` or `400` with an error code.

## Alternatives considered

- **Conversion in the React client**: violates the task.
- **Conversion inside the controller**: mixes HTTP and domain rules and makes the rules
  harder to test without a web host.
