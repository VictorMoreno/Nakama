using Nakama.Shared.Infrastructure.AnswerFeedback;
using Nakama.Shared.Infrastructure.Chat;
using Nakama.Shared.Infrastructure.Documents;
using Nakama.Shared.Infrastructure.Icons;
using Nakama.Shared.Infrastructure.Llm;
using Nakama.Shared.Infrastructure.Pagination;
using Nakama.Shared.Infrastructure.QuestionActivity;
using Nakama.Shared.Infrastructure.Reporting;
using Nakama.Shared.Infrastructure.Storage;
using Nakama.Shared.Infrastructure.UserActivity;

namespace Api.Configuration;

public static class OptionsExtensions
{
    public static IServiceCollection AddOptionsConfiguration(this IServiceCollection services)
    {
        services.AddOptions<StorageOptions>()
            .BindConfiguration(StorageOptions.SectionName)
            .ValidateOnStart();

        services.AddOptions<LlmOptions>()
            .BindConfiguration(LlmOptions.SectionName)
            .Validate(options => options.MaxTokens > 0, "Llm:MaxTokens must be greater than zero.")
            .ValidateOnStart();

        services.AddOptions<ChatOptions>()
            .BindConfiguration(ChatOptions.SectionName)
            .Validate(options => options.MaxAttachmentsPerMessage > 0, "Chat:MaxAttachmentsPerMessage must be greater than zero.")
            .Validate(options => options.MaxAttachmentSizeMb > 0, "Chat:MaxAttachmentSizeMb must be greater than zero.")
            .Validate(options => options.AllowedAttachmentExtensions.Length > 0, "Chat:AllowedAttachmentExtensions must not be empty.")
            .ValidateOnStart();

        services.AddOptions<DocumentsOptions>()
            .BindConfiguration(DocumentsOptions.SectionName)
            .Validate(options => options.MaxSizeMb > 0, "Documents:MaxSizeMb must be greater than zero.")
            .Validate(options => options.AllowedExtensions.Length > 0, "Documents:AllowedExtensions must not be empty.")
            .ValidateOnStart();

        services.AddOptions<PaginationOptions>()
            .BindConfiguration(PaginationOptions.SectionName)
            .Validate(options => options.DefaultPageSize > 0, "Pagination:DefaultPageSize must be greater than zero.")
            .Validate(options => options.MaxPageSize >= options.DefaultPageSize, "Pagination:MaxPageSize must not be lower than Pagination:DefaultPageSize.")
            .ValidateOnStart();

        services.AddOptions<ReportingOptions>()
            .BindConfiguration(ReportingOptions.SectionName)
            .Validate(options => TimeZoneInfo.TryFindSystemTimeZoneById(options.TimeZone, out _), "Reporting:TimeZone must be a time zone known to the system.")
            .ValidateOnStart();

        services.AddOptions<UserActivityOptions>()
            .BindConfiguration(UserActivityOptions.SectionName)
            .Validate(options => options.MaxDays >= 7, "Reporting:UserActivity:MaxDays must be at least 7.")
            .Validate(options => options.DefaultDays >= 1 && options.DefaultDays <= options.MaxDays, "Reporting:UserActivity:DefaultDays must be between 1 and Reporting:UserActivity:MaxDays.")
            .Validate(options => options.SessionIntervalMinutes > 0, "Reporting:UserActivity:SessionIntervalMinutes must be greater than zero.")
            .ValidateOnStart();

        services.AddOptions<QuestionActivityOptions>()
            .BindConfiguration(QuestionActivityOptions.SectionName)
            .Validate(options => options.MaxWeeks >= 4, "Reporting:QuestionActivity:MaxWeeks must be at least 4.")
            .Validate(options => options.DefaultWeeks >= 1 && options.DefaultWeeks <= options.MaxWeeks, "Reporting:QuestionActivity:DefaultWeeks must be between 1 and Reporting:QuestionActivity:MaxWeeks.")
            .ValidateOnStart();

        services.AddOptions<AnswerFeedbackOptions>()
            .BindConfiguration(AnswerFeedbackOptions.SectionName)
            .Validate(options => options.UnhelpfulQuestionsLimit >= 1, "Reporting:AnswerFeedback:UnhelpfulQuestionsLimit must be at least 1.")
            .ValidateOnStart();

        services.AddOptions<IconsOptions>()
            .BindConfiguration(IconsOptions.SectionName)
            .Validate(options => options.MaxSizeMb > 0, "Icons:MaxSizeMb must be greater than zero.")
            .Validate(options => options.ContentTypeExtensions.Count > 0, "Icons:ContentTypeExtensions must not be empty.")
            .ValidateOnStart();

        return services;
    }
}
