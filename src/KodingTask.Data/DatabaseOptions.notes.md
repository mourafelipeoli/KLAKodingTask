# DatabaseOptions

Bound from the `Database` configuration section.

`MySqlServerVersion` is configured instead of detected. `ServerVersion.AutoDetect` opens a
connection while the services are being built, so a database outage would stop the
container from starting at all, and the `/health` endpoint (which deliberately never touches
the database) would not answer either. A declared version keeps startup independent of the
database; the value only has to match the server's major version closely enough for SQL
generation.
