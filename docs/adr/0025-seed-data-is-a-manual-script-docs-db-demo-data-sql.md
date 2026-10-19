# ADR-25: seed data is a manual script (`docs/db/demo-data.sql`), not a migration

- **Status:** Accepted
- **Area:** Persistence — schema

Seed rows are not schema, so they do not belong in a migration: a migration runs on every database it touches —
including the Testcontainers template —, would hard-code passwords into the assembly and cannot be skipped. The
migrations only create the schema, and the demo data — the two login accounts (`admin@nakama.com` and `user@nakama.com`)
plus 20 users, 15 chatbots, 10 formations, 72 conversations and the read models that go with them — lives in
`docs/db/demo-data.sql`, which a person runs by hand (`docker exec -i nakama-postgres psql -U nakama -d nakama < docs/db/demo-data.sql`).

The script was not written by hand. The old generator was run once against a throwaway PostgreSQL and its output was
dumped with `pg_dump --data-only --column-inserts --on-conflict-do-nothing`, then the generator was deleted. That
keeps the exact JSON shapes and password hashes the application produces, and `ON CONFLICT DO NOTHING` makes the
script safe to run twice. The `pg_dump` preamble was trimmed to `client_encoding` and `standard_conforming_strings`
(`SET transaction_timeout` does not exist before PostgreSQL 17 and the `\restrict` meta-commands are psql-only).

Consequences: a fresh database has **no users**, so nobody can log in until the script is run; the dates inside it
are those of the day it was generated; `read_analytics` only exists once a chatbot is created or the script is loaded.
No test covers the script on purpose: it is demonstration data, nothing depends on it, and a schema change that
breaks it shows up as an error the moment someone runs it. If that turns out to be annoying, a single test that
loads it into a freshly migrated database (no row counts, no passwords) is the most that would be worth adding.

**Where it applies:** `docs/db/demo-data.sql`.
