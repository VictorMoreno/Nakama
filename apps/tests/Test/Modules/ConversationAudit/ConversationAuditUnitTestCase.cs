using System.Linq.Expressions;
using Moq;
using Nakama.ConversationAudit.Domain;
using Test.Modules.Shared;

namespace Test.Modules.ConversationAudit;

public abstract class ConversationAuditUnitTestCase : UnitTestCase
{
    protected Mock<IConversationAuditRepository> AuditRepository { get; } = new();

    [SetUp]
    public void ConversationAuditSetUp() => this.AuditRepository.Reset();

    protected void ShouldFindConversationAuditLog(ConversationAuditLog log) =>
        this.AuditRepository
            .Setup(repository => repository.FindById(log.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);

    protected void ShouldNotFindConversationAuditLog() =>
        this.AuditRepository
            .Setup(repository => repository.FindById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as ConversationAuditLog);

    protected void ShouldHaveSavedConversationAuditLogMatching(Expression<Func<ConversationAuditLog, bool>> matches) =>
        this.AuditRepository.Verify(
            repository => repository.Save(It.Is(matches), It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSavedAnyConversationAuditLog() =>
        this.AuditRepository.Verify(
            repository => repository.Save(It.IsAny<ConversationAuditLog>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHaveUpdatedConversationAuditLog(ConversationAuditLog log) =>
        this.AuditRepository.Verify(
            repository => repository.Update(log, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveUpdatedAnyConversationAuditLog() =>
        this.AuditRepository.Verify(
            repository => repository.Update(It.IsAny<ConversationAuditLog>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
