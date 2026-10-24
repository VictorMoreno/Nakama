using System.Text.Json.Serialization;
using Nakama.FormationDetail.Domain;

namespace Nakama.FormationDetail.Application;

using FormationDetail = Nakama.FormationDetail.Domain.FormationDetail;

public sealed record FormationDetailResponse(
    Guid Id,
    string Title,
    string Audience,
    string Description,
    string Status,
    bool Archived,
    string? IconUrl,
    List<FormationModuleResponse> Modules,
    List<FormationMemberRow> Team,
    List<FormationDocumentResponse> Docs)
{
    public static FormationDetailResponse From(FormationDetail entity, string iconsRoute) => new(
        entity.Id,
        entity.Title,
        entity.Audience,
        entity.Description,
        entity.Status,
        entity.Archived,
        entity.IconUrl is null ? null : $"{iconsRoute}/{entity.IconUrl}",
        entity.Modules.Select(FormationModuleResponse.From).ToList(),
        entity.Team,
        entity.Docs.Select(FormationDocumentResponse.From).ToList());

    public FormationDetailResponse WithoutAnswers() =>
        this with { Modules = this.Modules.Select(module => module.WithoutAnswers()).ToList() };
}

public sealed record QuizQuestionResponse(string Question, List<string> Options, int? CorrectIndex)
{
    public static QuizQuestionResponse From(QuizQuestionRow row) => new(row.Question, row.Options, row.CorrectIndex);
}

public sealed record FormationModuleResponse(
    Guid Id,
    string Title,
    string Type,
    string Summary,
    bool Gated,
    bool AiDraft,
    string? ResourceUrl,
    int? DurationMin,
    List<QuizQuestionResponse>? Quiz,
    int? PassingScore)
{
    public static FormationModuleResponse From(FormationModuleRow row) => new(
        row.Id,
        row.Title,
        row.Type,
        row.Summary,
        row.Gated,
        row.AiDraft,
        row.ResourceUrl,
        row.DurationMin,
        row.Quiz?.Select(QuizQuestionResponse.From).ToList(),
        row.PassingScore);

    public FormationModuleResponse WithoutAnswers() =>
        this with { Quiz = this.Quiz?.Select(question => question with { CorrectIndex = null }).ToList() };
}

public sealed record FormationDocumentResponse(
    Guid Id,
    string Name,
    string Origin,
    DateTime Updated,
    [property: JsonIgnore] string FileName)
{
    public static FormationDocumentResponse From(FormationDocumentRow row) => new(
        row.Id,
        row.Name,
        row.Origin,
        row.Updated,
        row.FileName);
}
