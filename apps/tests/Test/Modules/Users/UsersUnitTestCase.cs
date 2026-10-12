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

    protected void ShouldFindUser(User? user) =>
        this.Repository
            .Setup(repository => repository.FindById(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    protected void ShouldFindAllUsers(IReadOnlyList<User> users) =>
        this.Repository
            .Setup(repository => repository.FindAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

    protected void ShouldVerifyPassword(bool isValid) =>
        this.Repository
            .Setup(repository => repository.VerifyPassword(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(isValid);

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

    protected void ShouldHaveSavedUser(User user) =>
        this.Repository.Verify(
            repository => repository.Save(user, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSavedAnyUser() =>
        this.Repository.Verify(
            repository => repository.Save(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldNotHaveVerifiedAnyPassword() =>
        this.Repository.Verify(
            repository => repository.VerifyPassword(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHavePublishedUserActivityRecorded(UserId id, DateTime occurredAt) =>
        this.EventBus.Verify(
            bus => bus.Publish(
                It.Is<AggregateRoot>(aggregate => RecordsUserActivity(aggregate, id, occurredAt)),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHavePublishedUserCreated(UserId id) =>
        this.EventBus.Verify(
            bus => bus.Publish(
                It.Is<AggregateRoot>(aggregate => RecordsUserCreated(aggregate, id)),
                It.IsAny<CancellationToken>()),
            Times.Once);

    private static bool RecordsUserActivity(AggregateRoot aggregate, UserId id, DateTime occurredAt) =>
        aggregate.DomainEvents
            .OfType<UserActivityRecorded>()
            .Any(@event => @event.Id == id && @event.OccurredAt == occurredAt);

    private static bool RecordsUserCreated(AggregateRoot aggregate, UserId id) =>
        aggregate.DomainEvents.OfType<UserCreated>().Any(@event => @event.Id == id);
}
