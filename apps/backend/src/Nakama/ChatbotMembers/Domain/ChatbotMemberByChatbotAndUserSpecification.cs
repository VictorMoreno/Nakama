using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.ChatbotMembers.Domain;

public sealed class ChatbotMemberByChatbotAndUserSpecification(Guid chatbotId, Guid userId) : BaseSpecification<ChatbotMember>
{
    public Guid ChatbotId { get; } = chatbotId;

    public Guid UserId { get; } = userId;

    protected override Expression<Func<ChatbotMember, bool>> CreateExpression() =>
        member => member.ChatbotId == this.ChatbotId && member.UserId == this.UserId;
}
