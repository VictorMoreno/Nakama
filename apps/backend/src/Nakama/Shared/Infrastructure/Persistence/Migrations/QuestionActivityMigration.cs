using FluentMigrator;

namespace Nakama.Shared.Infrastructure.Persistence.Migrations;

[Migration(3)]
public sealed class QuestionActivityMigration : Migration
{
    public override void Up()
    {
        this.Create.Table("read_question_activity")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("botkey").AsGuid().NotNullable()
            .WithColumn("userid").AsGuid().NotNullable()
            .WithColumn("occurredat").AsDateTime().NotNullable();

        this.Create.Index("ix_read_question_activity_occurredat")
            .OnTable("read_question_activity")
            .OnColumn("occurredat").Ascending();
    }

    public override void Down() => throw new NotSupportedException("No down migrations - see ADR entry.");
}
