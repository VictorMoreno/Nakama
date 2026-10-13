using Nakama.ConversationHistory.Application.Finder;

namespace Test.Modules.ConversationHistory.Application;

public static class ConversationHistoryFinderQueryMother
{
    public static ConversationHistoryFinder.Query Random() => With();

    public static ConversationHistoryFinder.Query With(Guid? botKey = null, Guid? userId = null) =>
        new(botKey ?? Guid.NewGuid(), userId ?? Guid.NewGuid());

    public static ConversationHistoryFinder.Query WithoutUser(Guid? botKey = null) =>
        new(botKey ?? Guid.NewGuid(), null);
}
