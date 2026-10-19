using System.Linq.Expressions;
using Moq;
using Nakama.ChatbotMembers.Domain;
using Nakama.Shared.Domain.Specification;
using Test.Modules.Shared;

namespace Test.Modules.ChatbotMembers;

public abstract class ChatbotMembersUnitTestCase : UnitTestCase
{
    protected Mock<IChatbotMemberRepository> MembersRepository { get; } = new();

    [SetUp]
    public void ChatbotMembersSetUp() => this.MembersRepository.Reset();

    protected void ShouldFindMembershipsOfTheUser(Guid userId, IReadOnlyList<ChatbotMember> members) =>
        this.MembersRepository
            .Setup(repository => repository.Search(
                It.Is<ISpecification<ChatbotMember>>(specification =>
                    specification is ChatbotMemberByUserSpecification
                    && ((ChatbotMemberByUserSpecification)specification).UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(members);

    protected void ShouldFindChatbotMember(Guid chatbotId, Guid userId, ChatbotMember? member) =>
        this.MembersRepository
            .Setup(repository => repository.Search(
                It.Is<ISpecification<ChatbotMember>>(specification =>
                    specification is ChatbotMemberByChatbotAndUserSpecification
                    && ((ChatbotMemberByChatbotAndUserSpecification)specification).ChatbotId == chatbotId
                    && ((ChatbotMemberByChatbotAndUserSpecification)specification).UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(member is null ? [] : [member]);

    protected void ShouldHaveSavedChatbotMemberMatching(Expression<Func<ChatbotMember, bool>> matches) =>
        this.MembersRepository.Verify(
            repository => repository.Save(It.Is(matches), It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveRemovedChatbotMember(ChatbotMember member) =>
        this.MembersRepository.Verify(
            repository => repository.Remove(member, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveRemovedAnyChatbotMember() =>
        this.MembersRepository.Verify(
            repository => repository.Remove(It.IsAny<ChatbotMember>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
