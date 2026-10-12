using Moq;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Test.Modules.Shared;

public abstract class UnitTestCase
{
    protected Mock<ISystemClock> Clock { get; } = new();

    protected Mock<IEventBus> EventBus { get; } = new();

    [SetUp]
    public void BaseSetUp()
    {
        this.Clock.Reset();
        this.EventBus.Reset();
    }

    protected void ShouldBeNow(DateTime now) => this.Clock.Setup(clock => clock.UtcNow).Returns(now);

    protected void ShouldNotHavePublishedAnything() =>
        this.EventBus.Verify(
            bus => bus.Publish(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
