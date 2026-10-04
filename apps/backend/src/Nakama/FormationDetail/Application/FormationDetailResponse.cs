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
    Guid? LinkedChatbotKey,
    string? LinkedChatbotLabel,
    List<FormationModuleRow> Modules,
    List<FormationMemberRow> Team)
{
    public static FormationDetailResponse From(FormationDetail entity, string iconsRoute) => new(
        entity.Id,
        entity.Title,
        entity.Audience,
        entity.Description,
        entity.Status,
        entity.Archived,
        entity.IconUrl is null ? null : $"{iconsRoute}/{entity.IconUrl}",
        entity.LinkedChatbotKey,
        entity.LinkedChatbotLabel,
        entity.Modules,
        entity.Team);
}
