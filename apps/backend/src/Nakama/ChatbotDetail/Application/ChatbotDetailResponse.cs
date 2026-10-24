using System.Text.Json.Serialization;
using Nakama.ChatbotDetail.Domain;

namespace Nakama.ChatbotDetail.Application;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

public sealed record ChatbotDetailResponse(
    Guid Id,
    string Label,
    string Description,
    string Status,
    bool Archived,
    string? IconUrl,
    bool Socratic,
    bool ExplicaMas,
    string Provider,
    string Model,
    List<ChatbotDocumentResponse> Docs,
    List<TeamMemberRow> Team)
{
    public static ChatbotDetailResponse From(ChatbotDetail entity, string iconsRoute) => new(
        entity.Id,
        entity.Label,
        entity.Description,
        entity.Status,
        entity.Archived,
        entity.IconUrl is null ? null : $"{iconsRoute}/{entity.IconUrl}",
        entity.Socratic,
        entity.ExplicaMas,
        entity.Provider,
        entity.Model,
        entity.Docs.Select(d => ChatbotDocumentResponse.From(d)).ToList(),
        entity.Team);
}

public sealed record ChatbotDocumentResponse(
    Guid Id,
    string Name,
    string Origin,
    DateTime Updated,
    [property: JsonIgnore] string FileName)
{
    public static ChatbotDocumentResponse From(ChatbotDocumentRow row) => new(
        row.Id,
        row.Name,
        row.Origin,
        row.Updated,
        row.FileName);
}
