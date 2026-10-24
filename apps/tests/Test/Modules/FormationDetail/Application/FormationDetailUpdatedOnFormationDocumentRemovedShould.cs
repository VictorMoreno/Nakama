using AwesomeAssertions;
using Nakama.FormationDetail.Application.Handlers;
using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.FormationDetail.Domain;
using Test.Modules.Formations.Domain;
using FormationDetailEntity = Nakama.FormationDetail.Domain.FormationDetail;

namespace Test.Modules.FormationDetail.Application;

[TestFixture]
public sealed class FormationDetailUpdatedOnFormationDocumentRemovedShould : FormationDetailUnitTestCase
{
    private FormationDetailUpdatedOnFormationDocumentRemoved _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailUpdatedOnFormationDocumentRemoved(this.DetailRepository.Object);
    }

    [Test]
    public async Task remove_the_document_row_with_that_id()
    {
        FormationDocumentRow first = FormationDocumentRowMother.Random();
        FormationDocumentRow second = FormationDocumentRowMother.Random();
        FormationDetailEntity detail = FormationDetailMother.With(docs: [first, second]);
        FormationDocumentRemoved @event = FormationDocumentRemovedMother.With(FormationId.From(detail.Id), first.Id);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationDocumentRemoved>(@event), CancellationToken.None);

        detail.Docs.Should().ContainSingle().Which.Should().BeSameAs(second);
        this.ShouldHaveUpdatedFormationDetail(detail);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        FormationDocumentRemoved @event = FormationDocumentRemovedMother.Random();
        this.ShouldNotFindFormationDetail();

        await this._handler.Handle(new DomainEventNotification<FormationDocumentRemoved>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }
}
