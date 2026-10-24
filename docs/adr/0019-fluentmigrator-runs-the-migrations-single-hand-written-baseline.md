# ADR-19: FluentMigrator runs the migrations and the schema is a single hand-written baseline

- **Status:** Accepted
- **Area:** Persistence — schema

`MigrationRunner.Run(connectionString)` builds a FluentMigrator `IMigrationRunner`
(`AddFluentMigratorCore().ConfigureRunner(rb => rb.AddPostgres().WithGlobalConnectionString(...).ScanIn(assembly).For.Migrations())`)
and calls `MigrateUp()`. Applied migrations are tracked in FluentMigrator's own `VersionInfo` table. The runner is wired to
API startup **only in Development** (`WebApplicationConfigurationExtensions.Configure()`): a dev convenience so a fresh
clone plus `docker compose up` plus `dotnet run` yields a working schema with no manual step. It is not a deployment
strategy and nothing runs it in other environments. The Testcontainers template database that the infrastructure and E2E
tests clone is migrated with the same `MigrationRunner.Run`. Migrations have no `Down`: it throws `NotSupportedException`.

**The schema is a single hand-written baseline until the first production release.** `[Migration(1)] InitialSchemaMigration`
defines every table with FluentMigrator's fluent API (`Create.Table("chatbots").WithColumn("id").AsGuid().PrimaryKey()...`).
Because it is written by hand, a change to a `ClassMapping<T>` (a new property, a renamed column, a different
nullability) is not mirrored automatically: whoever changes the mapping edits `InitialSchemaMigration.Up()` too, and the
infrastructure and E2E tests, which run on the migrated schema, are what notice a mismatch. While nothing is deployed,
every schema change is edited straight into this file instead of adding migrations, so it always describes the whole
schema as one `CREATE` with no history to replay. The cost is that an existing local database has to be recreated
(`docker compose down -v && docker compose up -d`) after such an edit: its `VersionInfo` already records version 1 as
applied, so FluentMigrator would not run the file again. The numbered migrations that were added while a development
environment already existed in Azure (indexes of `read_user_activity`, `read_question_activity`, `read_answer_feedback` and
the columns dropped from `read_analytics` and `read_chatbot_detail`) were folded back into the baseline, and that
environment's database was recreated. From the first production deployment on, `InitialSchemaMigration` is frozen and
every change is a new numbered migration.

**Column naming.** Every column name is lowercase (`label`, not `Label`) even though the mapping says
`Property(x => x.Label)`. FluentMigrator's Postgres runner quotes every identifier it emits, preserving case (`"Label"`),
while NHibernate's generated DML is unquoted and Postgres folds an unquoted identifier to lowercase. `"Label"` and `label`
are different identifiers, and the mismatch only shows at runtime, as
`42703 column chatbotsum0_.label does not exist (Hint: Perhaps you meant "Label")`. Lowercase names avoid it. Every
primary key column is named `id`; a column that references a *different* aggregate's identity keeps a `_key`-style name
(`conversations.bot_key`, `read_conversation_audit.bot_key`), so it never looks like a local primary key.

**Nullability and indexes.** Every column is `NOT NULL` except the ones whose C# property is nullable (`iconurl` on the
chatbot and formation tables and their read models, `users.passwordhash`, `users.lastactiveat`,
`refresh_tokens.revoked_at` and `replaced_by_id`); the JSON columns are `NOT NULL` too, since the domain never leaves a
collection or a settings object null. Indexes exist where the code queries: a unique `users.normalizedemail` (login and
`UserByEmailSpecification` look users up by it, and two accounts must not share an email), a unique
`formation_enrollments (user_id, formation_id)` (its `user_id` prefix also serves `FormationEnrollmentByUserSpecification`),
a unique `read_chatbot_members (user_id, chatbot_id)`, `refresh_tokens.token_hash` (unique) and `user_id`, and a unique
`llm_provider_configs.provider`. There are no foreign keys on purpose: cross-aggregate references are raw ids
([ADR-5](0005-cross-aggregate-cross-module-references-are-raw-identifiers.md)).

**How it was checked.** The schema was compared with the mappings using NHibernate's `SchemaValidator` (no mismatch), the
whole test suite runs on it, and `docs/db/demo-data.sql` loads into it with no constraint violation
([ADR-25](0025-seed-data-is-a-manual-script-docs-db-demo-data-sql.md)).

**Where it applies:** `Shared/Infrastructure/Persistence/{MigrationRunner,Migrations/InitialSchemaMigration}.cs`,
`Shared/Infrastructure/Persistence/Nh/Mappings/*.cs`, `apps/backend/api/Configuration/WebApplicationConfigurationExtensions.cs`,
`Nakama.csproj`, `Directory.Packages.props`.
