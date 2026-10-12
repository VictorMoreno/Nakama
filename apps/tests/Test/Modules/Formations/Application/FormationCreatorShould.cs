using AwesomeAssertions;
using Nakama.Formations.Application.Creator;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

[TestFixture]
public sealed class FormationCreatorShould : FormationsUnitTestCase
{
    private FormationCreator.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationCreator.Handler(new(this.FormationsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task create_the_formation_and_publish_formation_created_when_it_does_not_exist()
    {
        FormationCreator.Command command = FormationCreatorCommandMother.With(
            status: FormationStatus.Draft,
            linkedChatbotKey: Guid.NewGuid());
        this.ShouldFindFormation(null);

        await this._handler.Handle(command, CancellationToken.None);

        this.ShouldHaveSavedFormationMatching(formation =>
            formation.Id.Value == command.Id
            && formation.Title == command.Title
            && formation.Audience == command.Audience
            && formation.Description == command.Description
            && formation.Status == FormationStatus.Draft
            && formation.LinkedChatbotKey == command.LinkedChatbotKey);
        this.ShouldHavePublished<FormationCreated>(@event => @event.Id.Value == command.Id);
    }

    [Test]
    public async Task create_the_formation_without_a_linked_chatbot()
    {
        this.ShouldFindFormation(null);

        await this._handler.Handle(FormationCreatorCommandMother.Random(), CancellationToken.None);

        this.ShouldHaveSavedFormationMatching(formation => formation.LinkedChatbotKey == null);
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_already_exists()
    {
        FormationCreator.Command command = FormationCreatorCommandMother.Random();
        this.ShouldFindFormation(FormationMother.With(id: FormationId.From(command.Id)));

        Func<Task> act = async () => await this._handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FormationAlreadyExistsError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }
}
