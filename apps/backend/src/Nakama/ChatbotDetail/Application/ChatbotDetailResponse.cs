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
    List<ChatbotDocumentRow> Docs,
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
        entity.IconUrl,
        entity.Socratic,
        entity.ExplicaMas,
        entity.Provider,
        entity.Docs,
        entity.Team,
        entity.Gaps,
        entity.TopQuestions,
        entity.Stats);
}
