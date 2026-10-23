# KodingTaskDbContext

The application's own database. Conversion itself needs no database: it is a pure function
in `KodingTask.Conversion`. The database holds what the optional features need (the Data
Protection key ring, and later translations, accounts and usage counters).

## Two providers, one model

- **SQLite** is the default, so a fresh clone runs with no database server. The schema is
  created with `EnsureCreated()` on startup (see `DatabaseSetup` in the API).
- **MySQL** is used in the hosted deployment. Its schema comes only from EF migrations,
  applied by an operator with an administrative account before the deploy that needs them.
  The application's runtime account has no DDL rights, so it could not alter the schema
  even if the code tried.

Migrations are generated for MySQL only (see `KodingTaskDbContextDesignTimeFactory`).
SQLite never runs migrations, so provider-specific column types in a migration cannot
break a local run.

## Conventions

- Enums are stored as their names, not numbers: a row stays readable in a SQL client, and
  reordering an enum cannot silently change the meaning of stored data.
- `decimal` gets an explicit precision (18, 6); MySQL would otherwise default to
  `decimal(65,30)`, and SQLite has no decimal type at all, so the convention makes the
  intent explicit in both.

## Data Protection keys

`IDataProtectionKeyContext` lets ASP.NET Core persist its key ring here. Antiforgery tokens
and authentication cookies are encrypted with those keys; storing them in the database
keeps cookies valid across container restarts and deploys, and across tasks when the
service runs more than one.

## Accounts

The context is an `IdentityDbContext<IdentityUser>`: the standard ASP.NET Core Identity
tables (`AspNetUsers`, `AspNetUserLogins` and the rest) live in the application's own
database, next to the application's per-user tables (`UserProfiles`, `VerifiedPhones`,
`PhoneChallenges`) and the delivery history (`OutboundMessages`). Those are keyed by the
user id string and never reference Identity columns beyond it, so the user store can be
replaced (an external identity provider) without migrating them. `base.OnModelCreating`
must run first: it maps the Identity tables.

