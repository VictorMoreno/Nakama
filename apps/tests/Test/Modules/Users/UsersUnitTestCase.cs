using Moq;
using Nakama.Shared.Domain;
using Test.Modules.Shared;
using Nakama.Users.Domain;
using Nakama.Users.Domain.Events;

namespace Test.Modules.Users;

public abstract class UsersUnitTestCase : UnitTestCase
{
    protected Mock<IUsersRepository> Repository { get; } = new();

    [SetUp]
    public void UsersSetUp() => this.Repository.Reset();

    protected void ShouldFindUserByEmail(User? user) =>
        this.Repository
            .Setup(repository => repository.GetByEmail(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    protected void ShouldAcceptPassword() =>
        this.Repository
            .Setup(repository => repository.CreateWithPassword(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserCredentialResult(true, []));

    protected void ShouldRejectPassword(string error) =>
        this.Repository
            .Setup(repository => repository.CreateWithPassword(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserCredentialResult(false, [error]));

    protected void ShouldHaveCreated(UserId id, string password) =>
        this.Repository.Verify(
            repository => repository.CreateWithPassword(
                It.Is<User>(user => user.Id == id),
                password,
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveCreatedUserAt(DateTime createdAt) =>
        this.Repository.Verify(
            repository => repository.CreateWithPassword(
                It.Is<User>(user => user.CreatedAt == createdAt),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveCreatedAnyUser() =>
        this.Repository.Verify(
            repository => repository.CreateWithPassword(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHavePublishedUserCreated(UserId id) =>
        this.EventBus.Verify(
            bus => bus.Publish(
                It.Is<AggregateRoot>(aggregate => RecordsUserCreated(aggregate, id)),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHavePublishedAnything() =>
        this.EventBus.Verify(
            bus => bus.Publish(It.IsAny<AggregateRoot>(), It.IsAny<CancellationToken>()),
            Times.Never);

    private static bool RecordsUserCreated(AggregateRoot aggregate, UserId id) =>
        aggregate.DomainEvents.OfType<UserCreated>().Any(@event => @event.Id == id);
}
