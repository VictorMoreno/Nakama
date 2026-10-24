using AwesomeAssertions;
using Nakama.ChatbotDetail.Application.Handlers;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.ChatbotDetail.Domain;
using Test.Modules.Chatbots.Domain;
using ChatbotDetailEntity = Nakama.ChatbotDetail.Domain.ChatbotDetail;

namespace Test.Modules.ChatbotDetail.Application;

[TestFixture]
public sealed class ChatbotDetailUpdatedOnChatbotSettingsUpdatedShould : ChatbotDetailUnitTestCase
{
    private ChatbotDetailUpdatedOnChatbotSettingsUpdated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDetailUpdatedOnChatbotSettingsUpdated(this.DetailRepository.Object);
    }

    [Test]
    public async Task replace_the_settings_of_the_detail()
    {
        ChatbotDetailEntity detail = ChatbotDetailMother.With(socratic: false, explicaMas: true);
        ChatbotSettings settings = ChatbotSettingsMother.With(socratic: true, explicaMas: false);
        ChatbotSettingsUpdated @event = ChatbotSettingsUpdatedMother.With(ChatbotId.From(detail.Id), settings);
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotSettingsUpdated>(@event), CancellationToken.None);

        detail.Socratic.Should().BeTrue();
        detail.ExplicaMas.Should().BeFalse();
        detail.Provider.Should().Be(settings.Provider.ToString());
        detail.Model.Should().Be(settings.Model);
        this.ShouldHaveUpdatedChatbotDetail(detail);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        ChatbotSettingsUpdated @event = ChatbotSettingsUpdatedMother.Random();
        this.ShouldNotFindChatbotDetail();

        await this._handler.Handle(new DomainEventNotification<ChatbotSettingsUpdated>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotDetail();
    }
}
