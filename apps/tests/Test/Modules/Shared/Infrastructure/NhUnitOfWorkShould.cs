using AwesomeAssertions;
using Nakama.Authentication.Domain;
using Nakama.Authentication.Infrastructure;
using Nakama.Shared.Infrastructure.Persistence;
using NHibernate;
using Test.Modules.Authentication.Domain;

namespace Test.Modules.Shared.Infrastructure;

[TestFixture]
public sealed class NhUnitOfWorkShould : InfrastructureTestCase
{
    private NhRefreshTokensRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        this._repository = new NhRefreshTokensRepository(this.UnitOfWork, this.UnitOfWorkFactory);
    }

    [Test]
    public async Task persist_what_was_saved_once_the_changes_are_saved()
    {
        RefreshToken token = RefreshTokenMother.Random();
        await this._repository.Save(token, CancellationToken.None);

        await this.UnitOfWork.SaveChanges(CancellationToken.None);

        (await this.Reader().FindById(token.Id, CancellationToken.None)).Should().NotBeNull();
    }

    [Test]
    public async Task discard_what_was_saved_when_the_changes_are_never_saved()
    {
        RefreshToken token = RefreshTokenMother.Random();
        await this._repository.Save(token, CancellationToken.None);

        await this.UnitOfWork.DisposeAsync();

        (await this.Reader().FindById(token.Id, CancellationToken.None)).Should().BeNull();
    }

    [Test]
    public async Task keep_what_was_saved_invisible_to_other_units_until_the_changes_are_saved()
    {
        RefreshToken token = RefreshTokenMother.Random();
        await this._repository.Save(token, CancellationToken.None);
        await this._repository.Search(new RefreshTokenByValueSpecification(token.Value!), CancellationToken.None);

        (await this.Reader().FindById(token.Id, CancellationToken.None)).Should().BeNull();
    }

    [Test]
    public async Task see_its_own_pending_writes_when_it_reads_before_saving_the_changes()
    {
        RefreshToken token = RefreshTokenMother.Random();
        await this._repository.Save(token, CancellationToken.None);

        IReadOnlyList<RefreshToken> found = await this._repository.Search(
            new RefreshTokenByValueSpecification(token.Value!),
            CancellationToken.None);

        found.Should().ContainSingle().Which.Id.Should().Be(token.Id);
    }

    [Test]
    public async Task not_open_a_transaction_nor_keep_the_connection_while_it_only_reads()
    {
        await this._repository.FindById(RefreshTokenMother.Random().Id, CancellationToken.None);

        ((NhUnitOfWork)this.UnitOfWork).HoldsConnection.Should().BeFalse();
    }

    [Test]
    public async Task open_the_transaction_with_the_first_write()
    {
        await this._repository.Save(RefreshTokenMother.Random(), CancellationToken.None);

        ITransaction? transaction = this.UnitOfWork.Session.GetCurrentTransaction();

        transaction.Should().NotBeNull();
        transaction.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task do_nothing_when_the_changes_are_saved_without_any_write()
    {
        Func<Task> act = async () => await this.UnitOfWork.SaveChanges(CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task accept_being_saved_twice()
    {
        RefreshToken token = RefreshTokenMother.Random();
        await this._repository.Save(token, CancellationToken.None);
        await this.UnitOfWork.SaveChanges(CancellationToken.None);

        Func<Task> act = async () => await this.UnitOfWork.SaveChanges(CancellationToken.None);

        await act.Should().NotThrowAsync();
        (await this.Reader().FindById(token.Id, CancellationToken.None)).Should().NotBeNull();
    }

    [Test]
    public async Task save_again_the_writes_made_after_a_previous_save()
    {
        RefreshToken first = RefreshTokenMother.Random();
        RefreshToken second = RefreshTokenMother.Random();
        await this._repository.Save(first, CancellationToken.None);
        await this.UnitOfWork.SaveChanges(CancellationToken.None);
        await this._repository.Save(second, CancellationToken.None);

        await this.UnitOfWork.SaveChanges(CancellationToken.None);

        NhRefreshTokensRepository reader = this.Reader();
        (await reader.FindById(first.Id, CancellationToken.None)).Should().NotBeNull();
        (await reader.FindById(second.Id, CancellationToken.None)).Should().NotBeNull();
    }

    [Test]
    public async Task discard_the_pending_writes_when_it_is_rolled_back_and_then_save_nothing()
    {
        RefreshToken token = RefreshTokenMother.Random();
        await this._repository.Save(token, CancellationToken.None);

        await this.UnitOfWork.Rollback(CancellationToken.None);
        await this.UnitOfWork.SaveChanges(CancellationToken.None);

        (await this.Reader().FindById(token.Id, CancellationToken.None)).Should().BeNull();
    }

    [Test]
    public async Task accept_new_writes_after_a_rollback()
    {
        RefreshToken discarded = RefreshTokenMother.Random();
        RefreshToken kept = RefreshTokenMother.Random();
        await this._repository.Save(discarded, CancellationToken.None);
        await this.UnitOfWork.Rollback(CancellationToken.None);
        await this._repository.Save(kept, CancellationToken.None);

        await this.UnitOfWork.SaveChanges(CancellationToken.None);

        NhRefreshTokensRepository reader = this.Reader();
        (await reader.FindById(discarded.Id, CancellationToken.None)).Should().BeNull();
        (await reader.FindById(kept.Id, CancellationToken.None)).Should().NotBeNull();
    }

    [Test]
    public async Task do_nothing_when_it_is_rolled_back_without_any_write()
    {
        Func<Task> act = async () => await this.UnitOfWork.Rollback(CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    private NhRefreshTokensRepository Reader() => new(this.NewUnitOfWork(), this.UnitOfWorkFactory);
}
