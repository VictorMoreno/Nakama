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
public sealed class FormationDetailUpdatedOnFormationDocumentAddedShould : FormationDetailUnitTestCase
{
    private FormationDetailUpdatedOnFormationDocumentAdded _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailUpdatedOnFormationDocumentAdded(this.DetailRepository.Object);
    }

    [Test]
    public async Task add_the_document_row_to_the_detail()
    {
        FormationDetailEntity detail = FormationDetailMother.Random();
        FormationDocument document = FormationDocumentMother.Random();
        FormationDocumentAdded @event = FormationDocumentAddedMother.With(FormationId.From(detail.Id), document);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationDocumentAdded>(@event), CancellationToken.None);

        FormationDocumentRow row = detail.Docs.Should().ContainSingle().Which;
        row.Id.Should().Be(document.Id);
        row.Name.Should().Be(document.Name);
        row.Origin.Should().Be(document.Origin);
        row.Updated.Should().Be(document.Updated);
        row.FileName.Should().Be(document.FileName);
        this.ShouldHaveUpdatedFormationDetail(detail);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        FormationDocumentAdded @event = FormationDocumentAddedMother.Random();
        this.ShouldNotFindFormationDetail();

        await this._handler.Handle(new DomainEventNotification<FormationDocumentAdded>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }
}
