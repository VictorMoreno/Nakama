using FluentMigrator;

namespace Nakama.Shared.Infrastructure.Persistence.Migrations;

[Migration(1)]
public sealed class InitialSchemaMigration : Migration
{
    public override void Up()
    {
        this.Create.Table("chatbots")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("label").AsString(255).NotNullable()
            .WithColumn("description").AsCustom("text").NotNullable()
            .WithColumn("status").AsInt32().NotNullable()
            .WithColumn("archived").AsBoolean().NotNullable()
            .WithColumn("iconurl").AsString(255).Nullable()
            .WithColumn("settings").AsCustom("text").NotNullable()
            .WithColumn("documents_json").AsCustom("text").NotNullable()
            .WithColumn("team_json").AsCustom("text").NotNullable();

        this.Create.Table("conversations")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("bot_key").AsGuid().NotNullable()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("title").AsString(255).NotNullable()
            .WithColumn("startedat").AsDateTime().NotNullable()
            .WithColumn("exchanges_json").AsCustom("text").NotNullable();

        this.Create.Table("formations")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("title").AsString(255).NotNullable()
            .WithColumn("audience").AsString(255).NotNullable()
            .WithColumn("description").AsCustom("text").NotNullable()
            .WithColumn("status").AsInt32().NotNullable()
            .WithColumn("archived").AsBoolean().NotNullable()
            .WithColumn("iconurl").AsString(255).Nullable()
            .WithColumn("modules_json").AsCustom("text").NotNullable()
            .WithColumn("documents_json").AsCustom("text").NotNullable();

        this.Create.Table("formation_enrollments")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("formation_id").AsGuid().NotNullable()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("progress_json").AsCustom("text").NotNullable();

        this.Create.Index("ix_formation_enrollments_user_formation")
            .OnTable("formation_enrollments")
            .OnColumn("user_id").Ascending()
            .OnColumn("formation_id").Ascending()
            .WithOptions().Unique();

        this.Create.Table("llm_provider_configs")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("provider").AsInt32().NotNullable()
            .WithColumn("api_key").AsCustom("text").NotNullable()
            .WithColumn("enabled").AsBoolean().NotNullable()
            .WithColumn("models_json").AsCustom("text").NotNullable()
            .WithColumn("updated_at").AsDateTime().NotNullable();

        this.Create.Index("ix_llm_provider_configs_provider")
            .OnTable("llm_provider_configs")
            .OnColumn("provider").Ascending()
            .WithOptions().Unique();

        this.Create.Table("read_chatbot_summaries")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("label").AsString(255).NotNullable()
            .WithColumn("description").AsCustom("text").NotNullable()
            .WithColumn("status").AsString(255).NotNullable()
            .WithColumn("docscount").AsInt32().NotNullable()
            .WithColumn("teamcount").AsInt32().NotNullable()
            .WithColumn("archived").AsBoolean().NotNullable()
            .WithColumn("iconurl").AsString(255).Nullable();

        this.Create.Table("read_chatbot_members")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("chatbot_id").AsGuid().NotNullable()
            .WithColumn("user_id").AsGuid().NotNullable();

        this.Create.Index("ix_read_chatbot_members_user_chatbot")
            .OnTable("read_chatbot_members")
            .OnColumn("user_id").Ascending()
            .OnColumn("chatbot_id").Ascending()
            .WithOptions().Unique();

        this.Create.Table("read_chatbot_detail")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("label").AsString(255).NotNullable()
            .WithColumn("description").AsCustom("text").NotNullable()
            .WithColumn("status").AsString(255).NotNullable()
            .WithColumn("context").AsCustom("text").NotNullable()
            .WithColumn("archived").AsBoolean().NotNullable()
            .WithColumn("iconurl").AsString(255).Nullable()
            .WithColumn("socratic").AsBoolean().NotNullable()
            .WithColumn("explicamas").AsBoolean().NotNullable()
            .WithColumn("provider").AsString(255).NotNullable()
            .WithColumn("model").AsString(255).NotNullable()
            .WithColumn("docs_json").AsCustom("text").NotNullable()
            .WithColumn("team_json").AsCustom("text").NotNullable();

        this.Create.Table("read_formations_list")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("title").AsString(255).NotNullable()
            .WithColumn("audience").AsString(255).NotNullable()
            .WithColumn("description").AsCustom("text").NotNullable()
            .WithColumn("status").AsString(255).NotNullable()
            .WithColumn("archived").AsBoolean().NotNullable()
            .WithColumn("iconurl").AsString(255).Nullable()
            .WithColumn("modulescount").AsInt32().NotNullable();

        this.Create.Table("read_formation_detail")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("title").AsString(255).NotNullable()
            .WithColumn("audience").AsString(255).NotNullable()
            .WithColumn("description").AsCustom("text").NotNullable()
            .WithColumn("status").AsString(255).NotNullable()
            .WithColumn("archived").AsBoolean().NotNullable()
            .WithColumn("iconurl").AsString(255).Nullable()
            .WithColumn("modules_json").AsCustom("text").NotNullable()
            .WithColumn("team_json").AsCustom("text").NotNullable()
            .WithColumn("docs_json").AsCustom("text").NotNullable();

        this.Create.Table("read_analytics")
            .WithColumn("id").AsString(255).PrimaryKey()
            .WithColumn("activebotscount").AsInt32().NotNullable();

        this.Create.Table("users")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("email").AsString(255).NotNullable()
            .WithColumn("normalizedemail").AsString(255).NotNullable()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("isadmin").AsBoolean().NotNullable()
            .WithColumn("passwordhash").AsString(255).Nullable()
            .WithColumn("lastactiveat").AsDateTime().Nullable()
            .WithColumn("createdat").AsDateTime().NotNullable();

        this.Create.Index("ix_users_normalizedemail").OnTable("users").OnColumn("normalizedemail").Ascending().WithOptions().Unique();

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

        this.Create.Table("read_user_activity")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("userid").AsGuid().NotNullable()
            .WithColumn("type").AsString(255).NotNullable()
            .WithColumn("occurredat").AsDateTime().NotNullable();

        this.Create.Index("ix_read_user_activity_occurredat")
            .OnTable("read_user_activity")
            .OnColumn("occurredat").Ascending();

        this.Create.Index("ix_read_user_activity_userid_occurredat")
            .OnTable("read_user_activity")
            .OnColumn("userid").Ascending()
            .OnColumn("occurredat").Ascending();

        this.Create.Table("read_question_activity")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("botkey").AsGuid().NotNullable()
            .WithColumn("userid").AsGuid().NotNullable()
            .WithColumn("occurredat").AsDateTime().NotNullable();

        this.Create.Index("ix_read_question_activity_occurredat")
            .OnTable("read_question_activity")
            .OnColumn("occurredat").Ascending();

        this.Create.Table("read_answer_feedback")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("botkey").AsGuid().NotNullable()
            .WithColumn("question").AsCustom("text").NotNullable()
            .WithColumn("useful").AsBoolean().NotNullable()
            .WithColumn("ratedat").AsDateTime().NotNullable();

        this.Create.Index("ix_read_answer_feedback_ratedat")
            .OnTable("read_answer_feedback")
            .OnColumn("ratedat").Ascending();

        this.Create.Table("read_conversation_audit")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("bot_key").AsGuid().NotNullable()
            .WithColumn("title").AsString(255).NotNullable()
            .WithColumn("startedat").AsDateTime().NotNullable()
            .WithColumn("messages_json").AsCustom("text").NotNullable();

        this.Create.Table("read_conversation_history")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("entries_json").AsCustom("text").NotNullable();
    }

    public override void Down() => throw new NotSupportedException("No down migrations - see ADR entry.");
}
