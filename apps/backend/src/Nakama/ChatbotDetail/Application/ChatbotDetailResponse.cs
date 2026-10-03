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
    public static explicit operator ChatbotDetailResponse(ChatbotDetail entity) => new(
        entity.Id,
        entity.Label,
        entity.Description,
        entity.Status,
        entity.LastUsed,
        entity.Context,
        entity.General,
        entity.Archived,
        entity.IconUrl is null ? null : $"/api/chatbots/icons/{entity.IconUrl}",
        entity.Socratic,
        entity.ExplicaMas,
        entity.Provider,
        entity.Model,
        entity.Docs.Select(d => (ChatbotDocumentResponse)d).ToList(),
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
    public static explicit operator ChatbotDocumentResponse(ChatbotDocumentRow row) => new(
        row.Id,
        row.Name,
        row.Origin,
        row.Status,
        row.StatusLabel,
        row.Updated,
        $"/api/chatbots/documents/{row.FileName}");
}
