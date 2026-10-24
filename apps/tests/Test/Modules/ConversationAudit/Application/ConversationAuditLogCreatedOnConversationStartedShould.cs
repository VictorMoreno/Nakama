using Nakama.Chatbots.Domain.Events;
using Nakama.ConversationAudit.Application.Handlers;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.Chatbots.Domain;
using Test.Modules.ConversationAudit.Domain;

namespace Test.Modules.ConversationAudit.Application;

[TestFixture]
public sealed class ConversationAuditLogCreatedOnConversationStartedShould : ConversationAuditUnitTestCase
{
    private ConversationAuditLogCreatedOnConversationStarted _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ConversationAuditLogCreatedOnConversationStarted(this.AuditRepository.Object);
    }

    [Test]
    public async Task save_an_audit_log_with_the_data_of_the_conversation_and_no_messages()
    {
        ConversationStarted @event = ConversationStartedMother.Random();
        this.ShouldNotFindConversationAuditLog();

        await this._handler.Handle(new DomainEventNotification<ConversationStarted>(@event), CancellationToken.None);

        this.ShouldHaveSavedConversationAuditLogMatching(log =>
            log.Id == @event.Id.Value
            && log.UserId == @event.UserId
            && log.BotKey == @event.BotKey.Value
            && log.Title == @event.Title
            && log.StartedAt == @event.StartedAt
            && log.Messages.Count == 0);
        this.ShouldNotHaveUpdatedAnyConversationAuditLog();
    }

    [Test]
    public async Task save_nothing_when_the_audit_log_already_exists()
    {
        ConversationStarted @event = ConversationStartedMother.Random();
        this.ShouldFindConversationAuditLog(ConversationAuditLogMother.With(id: @event.Id.Value));

        await this._handler.Handle(new DomainEventNotification<ConversationStarted>(@event), CancellationToken.None);

        this.ShouldNotHaveSavedAnyConversationAuditLog();
        this.ShouldNotHaveUpdatedAnyConversationAuditLog();
    }
}
