using AwesomeAssertions;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.ConversationAudit.Application.Handlers;
using Nakama.ConversationAudit.Domain;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.Chatbots.Domain;
using Test.Modules.ConversationAudit.Domain;

namespace Test.Modules.ConversationAudit.Application;

[TestFixture]
public sealed class ConversationAuditLogUpdatedOnConversationExchangeAppendedShould : ConversationAuditUnitTestCase
{
    private ConversationAuditLogUpdatedOnConversationExchangeAppended _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ConversationAuditLogUpdatedOnConversationExchangeAppended(
            this.AuditRepository.Object,
            this.Clock.Object);
    }

    [Test]
    public async Task append_the_exchange_as_an_audit_message_recorded_at_the_time_of_the_clock()
    {
        DateTime now = DateTime.UtcNow;
        ConversationAuditLog log = ConversationAuditLogMother.Random();
        ConversationExchange exchange = ConversationExchangeMother.Random();
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.With(
            ConversationId.From(log.Id),
            exchange: exchange);
        this.ShouldBeNow(now);
        this.ShouldFindConversationAuditLog(log);

        await this._handler.Handle(new DomainEventNotification<ConversationExchangeAppended>(@event), CancellationToken.None);

        AuditMessageRow message = log.Messages.Should().ContainSingle().Which;
        message.Question.Should().Be(exchange.Question);
        message.Answer.Should().Be(exchange.Answer);
        message.RecordedAt.Should().Be(now);
        this.ShouldHaveUpdatedConversationAuditLog(log);
    }

    [Test]
    public async Task keep_the_messages_already_in_the_audit_log()
    {
        AuditMessageRow existing = AuditMessageRowMother.Random();
        ConversationAuditLog log = ConversationAuditLogMother.With(messages: [existing]);
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.With(ConversationId.From(log.Id));
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindConversationAuditLog(log);

        await this._handler.Handle(new DomainEventNotification<ConversationExchangeAppended>(@event), CancellationToken.None);

        log.Messages.Should().HaveCount(2);
        log.Messages.First().Should().BeSameAs(existing);
    }

    [Test]
    public async Task do_nothing_when_the_audit_log_does_not_exist()
    {
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.Random();
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldNotFindConversationAuditLog();

        await this._handler.Handle(new DomainEventNotification<ConversationExchangeAppended>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyConversationAuditLog();
        this.ShouldNotHaveSavedAnyConversationAuditLog();
    }
}
