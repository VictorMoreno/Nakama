using Nakama.ChatbotDetail.Domain;
using Test.Modules.Shared;
using ChatbotDetailEntity = Nakama.ChatbotDetail.Domain.ChatbotDetail;

namespace Test.Modules.ChatbotDetail.Domain;

public static class ChatbotDetailMother
{
    public static ChatbotDetailEntity Create(
        Guid id,
        string label,
        string description,
        string status,
        string context,
        bool archived,
        string? iconUrl,
        bool socratic,
        bool explicaMas,
        string provider,
        string model,
        List<ChatbotDocumentRow> docs,
        List<TeamMemberRow> team,
        List<GapQuestionRow> gaps,
        List<GapQuestionRow> topQuestions,
        List<StatTileRow> stats) =>
        new(
            id,
            label,
            description,
            status,
            context,
            archived,
            iconUrl,
            socratic,
            explicaMas,
            provider,
            model,
            docs,
            team,
            gaps,
            topQuestions,
            stats);

    public static ChatbotDetailEntity Random() => With();

    public static ChatbotDetailEntity Populated() =>
        With(
            docs: [ChatbotDocumentRowMother.Random()],
            team: [TeamMemberRowMother.Random()],
            gaps: [GapQuestionRowMother.Random()],
            topQuestions: [GapQuestionRowMother.Random()],
            stats: [StatTileRowMother.Random()]);

    public static ChatbotDetailEntity With(
        Guid? id = null,
        string? label = null,
        string? description = null,
        string? status = null,
        string? context = null,
        bool? archived = null,
        string? iconUrl = null,
        bool? socratic = null,
        bool? explicaMas = null,
        string? provider = null,
        string? model = null,
        List<ChatbotDocumentRow>? docs = null,
        List<TeamMemberRow>? team = null,
        List<GapQuestionRow>? gaps = null,
        List<GapQuestionRow>? topQuestions = null,
        List<StatTileRow>? stats = null) =>
        Create(
            id ?? Guid.NewGuid(),
            label ?? WordMother.Random(),
            description ?? WordMother.Random(),
            status ?? "Active",
            context ?? WordMother.Random(),
            archived ?? false,
            iconUrl,
            socratic ?? MotherCreator.Random.Random.Bool(),
            explicaMas ?? MotherCreator.Random.Random.Bool(),
            provider ?? "Anthropic",
            model ?? WordMother.Random(),
            docs ?? [],
            team ?? [],
            gaps ?? [],
            topQuestions ?? [],
            stats ?? []);
}
