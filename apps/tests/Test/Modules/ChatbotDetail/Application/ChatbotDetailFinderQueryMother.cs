using Nakama.ChatbotDetail.Application.Finder;

namespace Test.Modules.ChatbotDetail.Application;

public static class ChatbotDetailFinderQueryMother
{
    public static ChatbotDetailFinder.Query Random() => With();

    public static ChatbotDetailFinder.Query With(Guid? id = null) => new(id ?? Guid.NewGuid());
}
