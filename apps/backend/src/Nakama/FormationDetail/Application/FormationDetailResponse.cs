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
    List<FormationModuleRow> Modules,
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
        entity.Modules,
        entity.Team,
        entity.Docs.Select(FormationDocumentResponse.From).ToList());
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
