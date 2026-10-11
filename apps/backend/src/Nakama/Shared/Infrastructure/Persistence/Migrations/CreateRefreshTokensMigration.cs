using FluentMigrator;

namespace Nakama.Shared.Infrastructure.Persistence.Migrations;

[Migration(4)]
public sealed class CreateRefreshTokensMigration : Migration
{
    public override void Up()
    {
        this.Create.Table("refresh_tokens")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("token_hash").AsString(64).NotNullable()
            .WithColumn("expires_at").AsDateTime().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("revoked_at").AsDateTime().Nullable()
            .WithColumn("replaced_by_id").AsGuid().Nullable();

        this.Create.Index("ix_refresh_tokens_token_hash").OnTable("refresh_tokens").OnColumn("token_hash").Ascending().WithOptions().Unique();
        this.Create.Index("ix_refresh_tokens_user_id").OnTable("refresh_tokens").OnColumn("user_id").Ascending();
    }

    public override void Down() => throw new NotSupportedException("No down migrations - see ADR entry.");
}
