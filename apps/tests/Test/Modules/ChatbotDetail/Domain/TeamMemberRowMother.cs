using Nakama.ChatbotDetail.Domain;
using Test.Modules.Shared;

namespace Test.Modules.ChatbotDetail.Domain;

public static class TeamMemberRowMother
{
    public static TeamMemberRow Random() => With();

    public static TeamMemberRow With(Guid? userId = null, string? role = null) =>
        new() { UserId = userId ?? Guid.NewGuid(), Role = role ?? WordMother.Random() };
}
