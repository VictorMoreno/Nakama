using FluentMigrator;

namespace Nakama.Shared.Infrastructure.Persistence.Migrations;

[Migration(1)]
public sealed class InitialSchemaMigration : Migration
{
    public override void Up()
    {
        Create.Table("chatbots")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("label").AsString(255).Nullable()
            .WithColumn("description").AsCustom("text").Nullable()
            .WithColumn("status").AsInt32().Nullable()
            .WithColumn("context").AsCustom("text").Nullable()
            .WithColumn("general").AsBoolean().Nullable()
            .WithColumn("archived").AsBoolean().Nullable()
            .WithColumn("iconurl").AsString(255).Nullable()
            .WithColumn("settings").AsCustom("text").Nullable()
            .WithColumn("lastused").AsDateTime().Nullable()
            .WithColumn("documents_json").AsCustom("text").Nullable()
            .WithColumn("team_json").AsCustom("text").Nullable();

        Create.Table("conversations")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("bot_key").AsGuid().Nullable()
            .WithColumn("user_id").AsGuid().Nullable()
            .WithColumn("title").AsString(255).Nullable()
            .WithColumn("startedat").AsDateTime().Nullable()
            .WithColumn("exchanges_json").AsCustom("text").Nullable();

        Create.Table("formations")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("title").AsString(255).Nullable()
            .WithColumn("audience").AsString(255).Nullable()
            .WithColumn("description").AsCustom("text").Nullable()
            .WithColumn("status").AsInt32().Nullable()
            .WithColumn("linked_chatbot_key").AsGuid().Nullable()
            .WithColumn("archived").AsBoolean().Nullable()
            .WithColumn("iconurl").AsString(255).Nullable()
            .WithColumn("modules_json").AsCustom("text").Nullable();

        Create.Table("formation_enrollments")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("formation_id").AsGuid().NotNullable()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("progress_json").AsCustom("text").Nullable();

        Create.Index("ix_formation_enrollments_formation_user")
            .OnTable("formation_enrollments")
            .OnColumn("formation_id").Ascending()
            .OnColumn("user_id").Ascending()
            .WithOptions().Unique();

        Create.Table("read_chatbot_summaries")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("label").AsString(255).Nullable()
            .WithColumn("description").AsCustom("text").Nullable()
            .WithColumn("status").AsString(255).Nullable()
            .WithColumn("docscount").AsInt32().Nullable()
            .WithColumn("teamcount").AsInt32().Nullable()
            .WithColumn("general").AsBoolean().Nullable()
            .WithColumn("archived").AsBoolean().Nullable()
            .WithColumn("iconurl").AsString(255).Nullable();

        Create.Table("read_chatbot_detail")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("label").AsString(255).Nullable()
            .WithColumn("description").AsCustom("text").Nullable()
            .WithColumn("status").AsString(255).Nullable()
            .WithColumn("lastused").AsDateTime().Nullable()
            .WithColumn("context").AsCustom("text").Nullable()
            .WithColumn("general").AsBoolean().Nullable()
            .WithColumn("archived").AsBoolean().Nullable()
            .WithColumn("iconurl").AsString(255).Nullable()
            .WithColumn("socratic").AsBoolean().Nullable()
            .WithColumn("explicamas").AsBoolean().Nullable()
            .WithColumn("provider").AsString(255).Nullable()
            .WithColumn("docs_json").AsCustom("text").Nullable()
            .WithColumn("team_json").AsCustom("text").Nullable()
            .WithColumn("gaps_json").AsCustom("text").Nullable()
            .WithColumn("top_questions_json").AsCustom("text").Nullable()
            .WithColumn("stats_json").AsCustom("text").Nullable();

        Create.Table("read_chat_page")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("label").AsString(255).Nullable()
            .WithColumn("context").AsCustom("text").Nullable()
            .WithColumn("socratic").AsBoolean().Nullable()
            .WithColumn("explicamas").AsBoolean().Nullable()
            .WithColumn("provider").AsString(255).Nullable()
            .WithColumn("opening_json").AsCustom("text").Nullable();

        Create.Table("read_formations_list")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("title").AsString(255).Nullable()
            .WithColumn("audience").AsString(255).Nullable()
            .WithColumn("description").AsCustom("text").Nullable()
            .WithColumn("status").AsString(255).Nullable()
            .WithColumn("archived").AsBoolean().Nullable()
            .WithColumn("iconurl").AsString(255).Nullable()
            .WithColumn("modulescount").AsInt32().Nullable();

        Create.Table("read_formation_detail")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("title").AsString(255).Nullable()
            .WithColumn("audience").AsString(255).Nullable()
            .WithColumn("description").AsCustom("text").Nullable()
            .WithColumn("status").AsString(255).Nullable()
            .WithColumn("archived").AsBoolean().Nullable()
            .WithColumn("iconurl").AsString(255).Nullable()
            .WithColumn("linked_chatbot_key").AsGuid().Nullable()
            .WithColumn("linked_chatbot_label").AsString(255).Nullable()
            .WithColumn("modules_json").AsCustom("text").Nullable()
            .WithColumn("team_json").AsCustom("text").Nullable();

        Create.Table("read_formation_detail_chatbot_label_ref")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("label").AsString(255).Nullable();

        Create.Table("read_analytics")
            .WithColumn("id").AsString(255).PrimaryKey()
            .WithColumn("activebotscount").AsInt32().Nullable()
            .WithColumn("answeredthisweek").AsInt32().Nullable()
            .WithColumn("usage_by_bot_json").AsCustom("text").Nullable()
            .WithColumn("usefulfeedbackcount").AsInt32().Nullable()
            .WithColumn("totalfeedbackcount").AsInt32().Nullable();

        Create.Table("users")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("email").AsString(255).Nullable()
            .WithColumn("normalizedemail").AsString(255).Nullable()
            .WithColumn("name").AsString(255).Nullable()
            .WithColumn("isadmin").AsBoolean().Nullable()
            .WithColumn("passwordhash").AsString(255).Nullable()
            .WithColumn("lastactiveat").AsDateTime().Nullable()
            .WithColumn("createdat").AsDateTime().Nullable();

        Create.Table("read_user_activity")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("userid").AsGuid().Nullable()
            .WithColumn("type").AsString(255).Nullable()
            .WithColumn("occurredat").AsDateTime().Nullable();

        Create.Table("read_conversation_audit")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("user_id").AsGuid().Nullable()
            .WithColumn("bot_key").AsGuid().Nullable()
            .WithColumn("title").AsString(255).Nullable()
            .WithColumn("startedat").AsDateTime().Nullable()
            .WithColumn("messages_json").AsCustom("text").Nullable();

        Create.Table("read_conversation_history")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("entries_json").AsCustom("text").Nullable();
    }

    public override void Down() => throw new NotSupportedException("No down migrations - see ADR entry.");
}
