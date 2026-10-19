using Nakama.Shared.Domain.Specification;

namespace Nakama.ChatbotMembers.Domain;

public interface IChatbotMemberRepository
{
    Task<IReadOnlyList<ChatbotMember>> Search(ISpecification<ChatbotMember> specification, CancellationToken ct);

    Task Save(ChatbotMember member, CancellationToken ct);

    Task Remove(ChatbotMember member, CancellationToken ct);
}
