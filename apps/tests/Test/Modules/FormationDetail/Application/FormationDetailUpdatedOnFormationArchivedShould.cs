using AwesomeAssertions;
using Nakama.FormationDetail.Application.Handlers;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.FormationDetail.Domain;
using Test.Modules.Formations.Domain;
using FormationDetailEntity = Nakama.FormationDetail.Domain.FormationDetail;

namespace Test.Modules.FormationDetail.Application;

[TestFixture]
public sealed class FormationDetailUpdatedOnFormationArchivedShould : FormationDetailUnitTestCase
{
    private FormationDetailUpdatedOnFormationArchived _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailUpdatedOnFormationArchived(this.DetailRepository.Object);
    }

    [Test]
    public async Task mark_the_detail_as_archived()
    {
        FormationDetailEntity detail = FormationDetailMother.With(archived: false);
        FormationArchived @event = FormationArchivedMother.With(FormationId.From(detail.Id));
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationArchived>(@event), CancellationToken.None);

        detail.Archived.Should().BeTrue();
        this.ShouldHaveUpdatedFormationDetail(detail);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        FormationArchived @event = FormationArchivedMother.Random();
        this.ShouldNotFindFormationDetail();

        await this._handler.Handle(new DomainEventNotification<FormationArchived>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }
}
