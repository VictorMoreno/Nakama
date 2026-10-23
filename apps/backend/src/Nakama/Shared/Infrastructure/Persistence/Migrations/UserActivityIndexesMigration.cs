using FluentMigrator;

namespace Nakama.Shared.Infrastructure.Persistence.Migrations;

[Migration(2)]
public sealed class UserActivityIndexesMigration : Migration
{
    public override void Up()
    {
        this.Create.Index("ix_read_user_activity_occurredat")
            .OnTable("read_user_activity")
            .OnColumn("occurredat").Ascending();

        this.Create.Index("ix_read_user_activity_userid_occurredat")
            .OnTable("read_user_activity")
            .OnColumn("userid").Ascending()
            .OnColumn("occurredat").Ascending();
    }

    public override void Down() => throw new NotSupportedException("No down migrations - see ADR entry.");
}
