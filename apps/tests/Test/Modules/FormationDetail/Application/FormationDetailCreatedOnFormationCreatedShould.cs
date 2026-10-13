using Nakama.FormationDetail.Application.Handlers;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Nakama.FormationDetail.Domain;
using Test.Modules.FormationDetail.Domain;
using Test.Modules.Formations.Domain;

namespace Test.Modules.FormationDetail.Application;

[TestFixture]
public sealed class FormationDetailCreatedOnFormationCreatedShould : FormationDetailUnitTestCase
{
    private FormationDetailCreatedOnFormationCreated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailCreatedOnFormationCreated(
            this.DetailRepository.Object,
            this.LabelRefsRepository.Object);
    }

    [Test]
    public async Task save_a_detail_with_the_data_of_the_formation_and_no_link_when_it_has_no_chatbot()
    {
        FormationCreated @event = FormationCreatedMother.With(linkedChatbotKey: null);

        await this._handler.Handle(new DomainEventNotification<FormationCreated>(@event), CancellationToken.None);

        this.ShouldHaveSavedFormationDetailMatching(detail =>
            detail.Id == @event.Id.Value
            && detail.Title == @event.Title
            && detail.Audience == @event.Audience
            && detail.Description == @event.Description
            && detail.Status == @event.Status.ToString()
            && !detail.Archived
            && detail.IconUrl == null
            && detail.LinkedChatbotKey == null
            && detail.LinkedChatbotLabel == null
            && detail.Modules.Count == 0
            && detail.Team.Count == 0);
        this.ShouldNotHaveSearchedForAnyChatbotLabelRef();
        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }

    [Test]
    public async Task take_the_label_of_the_linked_chatbot_from_its_reference()
    {
        ChatbotLabelRef labelRef = ChatbotLabelRefMother.Random();
        FormationCreated @event = FormationCreatedMother.With(linkedChatbotKey: labelRef.Id);
        this.ShouldFindChatbotLabelRef(labelRef);

        await this._handler.Handle(new DomainEventNotification<FormationCreated>(@event), CancellationToken.None);

        this.ShouldHaveSavedFormationDetailMatching(detail =>
            detail.LinkedChatbotKey == labelRef.Id && detail.LinkedChatbotLabel == labelRef.Label);
    }

    [Test]
    public async Task keep_the_link_without_a_label_when_the_chatbot_reference_does_not_exist()
    {
        Guid chatbotKey = Guid.NewGuid();
        FormationCreated @event = FormationCreatedMother.With(linkedChatbotKey: chatbotKey);
        this.ShouldNotFindChatbotLabelRef();

        await this._handler.Handle(new DomainEventNotification<FormationCreated>(@event), CancellationToken.None);

        this.ShouldHaveSavedFormationDetailMatching(detail =>
            detail.LinkedChatbotKey == chatbotKey && detail.LinkedChatbotLabel == null);
    }
}
