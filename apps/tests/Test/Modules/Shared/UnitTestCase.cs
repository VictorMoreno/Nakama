using Microsoft.Extensions.Options;
using Moq;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;
using Nakama.Shared.Infrastructure.Reporting;

namespace Test.Modules.Shared;

public abstract class UnitTestCase
{
    protected Mock<ISystemClock> Clock { get; } = new();

    protected IReportingCalendar Calendar => new ReportingCalendar(this.Clock.Object, Options.Create(ReportingOptionsMother.Random()));

    protected Mock<IEventBus> EventBus { get; } = new();

    protected Mock<IQueryBus> QueryBus { get; } = new();

    [SetUp]
    public void BaseSetUp()
    {
        this.Clock.Reset();
        this.EventBus.Reset();
        this.QueryBus.Reset();
    }

    protected void ShouldBeNow(DateTime now) => this.Clock.Setup(clock => clock.UtcNow).Returns(now);

    protected void ShouldHavePublished<TEvent>(Func<TEvent, bool> matches)
        where TEvent : IDomainEvent =>
        this.EventBus.Verify(
            bus => bus.Publish(
                It.Is<AggregateRoot>(aggregate => aggregate.DomainEvents.OfType<TEvent>().Any(matches)),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHavePublishedAnything() =>
        this.EventBus.Verify(
            bus => bus.Publish(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
