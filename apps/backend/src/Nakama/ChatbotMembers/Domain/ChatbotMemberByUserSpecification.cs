using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.ChatbotMembers.Domain;

public sealed class ChatbotMemberByUserSpecification(Guid userId) : BaseSpecification<ChatbotMember>
{
    public Guid UserId { get; } = userId;

    protected override Expression<Func<ChatbotMember, bool>> CreateExpression() => member => member.UserId == this.UserId;
}
