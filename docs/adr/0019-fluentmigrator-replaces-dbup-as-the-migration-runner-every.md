# ADR-19: FluentMigrator replaces DbUp as the migration runner; every table's primary key column is `id`, not `key`

- **Status:** Accepted
- **Area:** Persistence — schema

`MigrationRunner.Run(connectionString)` now builds a FluentMigrator `IMigrationRunner`
(`AddFluentMigratorCore().ConfigureRunner(rb => rb.AddPostgres().WithGlobalConnectionString(...).ScanIn(assembly).For.Migrations())`)
and calls `MigrateUp()`, instead of DbUp's `DeployChanges.To.PostgresqlDatabase(...)`. FluentMigrator tracks applied
migrations in its own `VersionInfo` table (three of its own bootstrap migrations create/extend it on first run) — same
role DbUp's `SchemaVersions` journal used to have.

`[Migration(1)] InitialSchemaMigration` defines the schema with FluentMigrator's own fluent API
(`Create.Table("chatbots").WithColumn("id").AsGuid().PrimaryKey()...`), not by executing a `.sql` file — the
embedded-script approach (`Execute.EmbeddedScript(...)`) was the first cut but was deliberately replaced with
hand-written fluent code per explicit request, trading away part of [ADR-17](0017-migrations-are-generated-from-the-real-nhibernate-mapping.md)'s guarantee: the schema is no longer
regenerated from the live NHibernate mapping metadata on every change, so a future mapping change (new property, renamed
column, different nullability) does **not** automatically keep the migration in sync — whoever changes a
`ClassMapping<T>` now also has to hand-update `InitialSchemaMigration.Up()` to match. The column types and names in this
first migration were transcribed 1:1 from a `SchemaExport` run against the mappings as they stood at the time (verified
against the live Postgres schema via `information_schema.columns` before/after), so there is no drift *yet* - only going
forward.

**Every non-snake_case column name in `InitialSchemaMigration` is written lowercase on purpose** (`label`, not `Label`),
even though the NHibernate mapping is `Property(x => x.Label)`. `FluentMigrator.Runner.Postgres` quotes every identifier
it emits in `CREATE TABLE`, preserving exact case (`"Label"`); NHibernate's own generated DML is unquoted, and Postgres
case-folds an unquoted identifier to lowercase before matching it against the catalog. A quoted `"Label"` and a folded
`label` are two different identifiers to Postgres — the first version of this migration used `.WithColumn("Label")` and
every read endpoint failed at runtime with
`42703 column chatbotsum0_.label does not exist (Hint: Perhaps you meant "Label")`, caught by hitting the real endpoints
against a freshly-migrated database, not by `dotnet build`. Lowercasing every column name side-steps the mismatch
entirely, matching what the old SchemaExport-generated unquoted SQL produced.

Every `ClassMapping<T>`'s `Id(x => x.Id, m => m.Column("..."))` that used to name its primary-key column `key` (or, for
two read-model tables whose id happens to equal their owning chatbot's id, `bot_key`) now names it `id` — consistent,
predictable naming across all ten mapped tables, no exceptions for read-model tables. This does **not** apply to genuine
foreign-key-shaped columns that reference a *different* aggregate's identity (`conversations.bot_key`)
— that keeps its `_key` suffix, since renaming them to `id` would make a foreign
reference look like a local primary key. Since this changed the physical schema and there was no production data to
preserve, the fix was a full local Postgres volume recreation (`docker compose down -v && docker compose up -d`) rather
than a second migration transforming the old columns in place — acceptable because `MigrationRunner.Run` only ever
executes in Development ([ADR-17](0017-migrations-are-generated-from-the-real-nhibernate-mapping.md)).

**The schema is a single baseline until the first production release.** While nothing is deployed, every schema change
is edited straight into `InitialSchemaMigration` instead of stacking new migrations: the refresh-tokens table that
briefly lived in its own migration and the removal of the formation's linked chatbot (the
`linked_chatbot_key`/`linked_chatbot_label` columns and the `read_formation_detail_chatbot_label_ref` table) were folded
back in, so the file always describes the whole schema as one `CREATE`, with no history to replay. The folded result was
checked against the three migrations it replaced by dumping `information_schema.columns` and `pg_indexes` from a
freshly migrated database before and after (identical) and by running NHibernate's `SchemaValidator` against it (no
mapping/schema mismatch).

Before freezing it the baseline was also hardened. Every column is `NOT NULL` except the ones whose C# property is
nullable (`iconurl` on the chatbot and formation tables and their read models, `users.passwordhash`,
`users.lastactiveat`, `refresh_tokens.revoked_at` and `replaced_by_id`); the JSON columns are `NOT NULL` too, since the
domain never leaves a collection or a settings object null. Two indexes were added where the code actually queries:
a unique `users.normalizedemail` (login and `UserByEmailSpecification` look users up by it, and two accounts must not
share an email) and `formation_enrollments (user_id, formation_id)` unique, which replaces the previous
`(formation_id, user_id)` one because its `user_id` prefix also serves `FormationEnrollmentByUserSpecification`. No
foreign keys on purpose: cross-aggregate references are raw ids ([ADR-5](0005-cross-aggregate-cross-module-references-are-raw-identifiers.md)). The hardened schema was verified with the whole
suite (including the E2E tests) and by loading `docs/db/demo-data.sql` into it with no constraint violation.

The cost is that any existing local database must be recreated
(`docker compose down -v && docker compose up -d`): its `VersionInfo` already records version 1 as applied, so
FluentMigrator would not re-run the rewritten file. From the first production deployment on, `InitialSchemaMigration`
is frozen and every change is a new numbered migration.

**Where it applies:** `Shared/Infrastructure/Persistence/{MigrationRunner,Migrations/InitialSchemaMigration}.cs`,
`Shared/Infrastructure/Persistence/Nh/Mappings/*.cs`, `Nakama.csproj`, `Directory.Packages.props`.
