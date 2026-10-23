using FluentMigrator;

namespace Nakama.Shared.Infrastructure.Persistence.Migrations;

[Migration(4)]
public sealed class AnalyticsDropQuestionCountersMigration : Migration
{
    public override void Up()
    {
        this.Delete.Column("answeredthisweek").FromTable("read_analytics");
        this.Delete.Column("usage_by_bot_json").FromTable("read_analytics");
    }

    public override void Down() => throw new NotSupportedException("No down migrations - see ADR entry.");
}
