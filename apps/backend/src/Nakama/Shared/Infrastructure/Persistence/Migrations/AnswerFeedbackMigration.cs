using FluentMigrator;

namespace Nakama.Shared.Infrastructure.Persistence.Migrations;

[Migration(5)]
public sealed class AnswerFeedbackMigration : Migration
{
    public override void Up()
    {
        this.Create.Table("read_answer_feedback")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("botkey").AsGuid().NotNullable()
            .WithColumn("question").AsCustom("text").NotNullable()
            .WithColumn("useful").AsBoolean().NotNullable()
            .WithColumn("ratedat").AsDateTime().NotNullable();

        this.Create.Index("ix_read_answer_feedback_ratedat")
            .OnTable("read_answer_feedback")
            .OnColumn("ratedat").Ascending();

        this.Delete.Column("usefulfeedbackcount").FromTable("read_analytics");
        this.Delete.Column("totalfeedbackcount").FromTable("read_analytics");
    }

    public override void Down() => throw new NotSupportedException("No down migrations - see ADR entry.");
}
