using Nakama.ChatbotDetail.Domain;

namespace Nakama.ChatbotDetail.Application;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

public sealed record ChatbotDetailResponse(
    Guid Id,
    string Label,
    string Description,
    string Status,
    DateTime LastUsed,
    string Context,
    bool General,
    bool Archived,
    string? IconUrl,
    bool Socratic,
    bool ExplicaMas,
    string Provider,
    string Model,
    List<ChatbotDocumentResponse> Docs,
    List<TeamMemberRow> Team,
    List<GapQuestionRow> Gaps,
    List<GapQuestionRow> TopQuestions,
    List<StatTileRow> Stats)
{
    public static ChatbotDetailResponse From(ChatbotDetail entity, string iconsRoute, string documentsRoute) => new(
        entity.Id,
        entity.Label,
        entity.Description,
        entity.Status,
        entity.LastUsed,
        entity.Context,
        entity.General,
        entity.Archived,
        entity.IconUrl is null ? null : $"{iconsRoute}/{entity.IconUrl}",
        entity.Socratic,
        entity.ExplicaMas,
        entity.Provider,
        entity.Model,
        entity.Docs.Select(d => ChatbotDocumentResponse.From(d, documentsRoute)).ToList(),
        entity.Team,
        entity.Gaps,
        entity.TopQuestions,
        entity.Stats);
}

public sealed record ChatbotDocumentResponse(
    Guid Id,
    string Name,
    string Origin,
    string Status,
    string StatusLabel,
    DateTime Updated,
    string Url)
{
    public static ChatbotDocumentResponse From(ChatbotDocumentRow row, string documentsRoute) => new(
        row.Id,
        row.Name,
        row.Origin,
        row.Status,
        row.StatusLabel,
        row.Updated,
        $"{documentsRoute}/{row.FileName}");
}
