# TestDatabase

An in-memory SQLite database for service tests. The connection stays open for the lifetime
of the object, because an in-memory SQLite database disappears when its last connection
closes; every context created from it shares that one connection and therefore the same
data, which is what lets a test simulate two requests racing for a counter.
