using Nakama.ChatbotMembers.Domain;

namespace Nakama.ChatbotMembers.Application;

public sealed record ChatbotMemberResponse(Guid ChatbotId, Guid UserId)
{
    public static explicit operator ChatbotMemberResponse(ChatbotMember entity) => new(entity.ChatbotId, entity.UserId);
}
