using Nakama.FormationDetail.Domain;
using Test.Modules.Shared;

namespace Test.Modules.FormationDetail.Domain;

public static class ChatbotLabelRefMother
{
    public static ChatbotLabelRef Random() => With();

    public static ChatbotLabelRef With(Guid? id = null, string? label = null) =>
        new(id ?? Guid.NewGuid(), label ?? WordMother.Random());
}
