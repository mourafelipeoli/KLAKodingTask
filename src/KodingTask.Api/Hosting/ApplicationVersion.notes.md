# ApplicationVersion

Where the running version and commit come from.

- **Version**: `versions.json` at the repository root is the single source. The API project
  links it into its build and publish output, so the file inside the container is the one
  that was built. The deploy script bumps the patch number before building.
- **Commit**: the `BUILD_COMMIT` environment variable, injected by the deploy into the task
  definition. A local run has none, and `/health` reports `null`, which is honest.
- An explicit `App:Version` (environment variable `App__Version`) wins over the file, which
  is useful in tests.

A broken or missing `versions.json` never stops the application: the version is reported
as unknown instead. Read errors are limited to I/O and JSON errors; anything else is a bug
and should surface.
