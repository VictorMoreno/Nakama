using Nakama.ConversationHistory.Domain;
using ConversationHistoryEntity = Nakama.ConversationHistory.Domain.ConversationHistory;

namespace Test.Modules.ConversationHistory.Domain;

public static class ConversationHistoryMother
{
    public static ConversationHistoryEntity Create(Guid id, List<ConversationSummaryRow> entries) => new(id, entries);

    public static ConversationHistoryEntity Random() => With();

    public static ConversationHistoryEntity With(Guid? id = null, List<ConversationSummaryRow>? entries = null) =>
        Create(id ?? Guid.NewGuid(), entries ?? []);
}
