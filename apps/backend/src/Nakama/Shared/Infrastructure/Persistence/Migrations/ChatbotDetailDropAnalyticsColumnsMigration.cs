using FluentMigrator;

namespace Nakama.Shared.Infrastructure.Persistence.Migrations;

[Migration(6)]
public sealed class ChatbotDetailDropAnalyticsColumnsMigration : Migration
{
    public override void Up()
    {
        this.Delete.Column("gaps_json").FromTable("read_chatbot_detail");
        this.Delete.Column("top_questions_json").FromTable("read_chatbot_detail");
        this.Delete.Column("stats_json").FromTable("read_chatbot_detail");
    }

    public override void Down() => throw new NotSupportedException("No down migrations - see ADR entry.");
}
