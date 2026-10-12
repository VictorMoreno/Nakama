using AwesomeAssertions;
using Nakama.LlmProviders.Application.Registerer;
using Nakama.LlmProviders.Domain;
using Test.Modules.LlmProviders.Domain;

namespace Test.Modules.LlmProviders.Application;

[TestFixture]
public sealed class LlmProviderRegistererShould : LlmProvidersUnitTestCase
{
    private LlmProviderRegisterer.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new LlmProviderRegisterer.Handler(new(this.ProviderConfigsRepository.Object, this.Clock.Object));
    }

    [Test]
    public async Task register_a_new_config_with_the_given_data_when_the_provider_has_none()
    {
        DateTime now = DateTime.UtcNow;
        LlmModelOption first = LlmModelOptionMother.Random();
        LlmModelOption second = LlmModelOptionMother.Random();
        LlmProviderRegisterer.Command command = LlmProviderRegistererCommandMother.With(
            LlmProvider.OpenAi,
            enabled: true,
            models:
            [
                new LlmProviderRegisterer.ModelInput(first.Id, first.Label),
                new LlmProviderRegisterer.ModelInput(second.Id, second.Label)
            ]);
        this.ShouldBeNow(now);
        this.ShouldFindProviderConfig(LlmProvider.OpenAi, null);

        await this._handler.Handle(command, CancellationToken.None);

        this.ShouldHaveSavedProviderConfigMatching(config =>
            config.Provider == LlmProvider.OpenAi
            && config.ApiKey == command.ApiKey
            && config.Enabled
            && config.UpdatedAt == now
            && config.Models.SequenceEqual(new[] { first, second }));
    }

    [Test]
    public async Task update_the_existing_config_replacing_the_key_the_flag_and_the_models()
    {
        DateTime now = DateTime.UtcNow;
        LlmModelOption newModel = LlmModelOptionMother.Random();
        LlmProviderConfig existing = LlmProviderConfigMother.With(
            provider: LlmProvider.Anthropic,
            enabled: false,
            models: [LlmModelOptionMother.Random(), LlmModelOptionMother.Random()]);
        LlmProviderRegisterer.Command command = LlmProviderRegistererCommandMother.With(
            LlmProvider.Anthropic,
            enabled: true,
            models: [new LlmProviderRegisterer.ModelInput(newModel.Id, newModel.Label)]);
        this.ShouldBeNow(now);
        this.ShouldFindProviderConfig(LlmProvider.Anthropic, existing);

        await this._handler.Handle(command, CancellationToken.None);

        existing.ApiKey.Should().Be(command.ApiKey);
        existing.Enabled.Should().BeTrue();
        existing.Models.Should().Equal(newModel);
        existing.UpdatedAt.Should().Be(now);
        this.ShouldHaveSavedProviderConfig(existing);
    }

    [Test]
    public async Task keep_the_existing_api_key_when_the_command_has_an_empty_one()
    {
        LlmProviderConfig existing = LlmProviderConfigMother.With(provider: LlmProvider.Local, enabled: false);
        string originalKey = existing.ApiKey;
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindProviderConfig(LlmProvider.Local, existing);

        await this._handler.Handle(
            LlmProviderRegistererCommandMother.With(LlmProvider.Local, apiKey: string.Empty, enabled: true),
            CancellationToken.None);

        existing.ApiKey.Should().Be(originalKey);
        existing.Enabled.Should().BeTrue();
        this.ShouldHaveSavedProviderConfig(existing);
    }

    [Test]
    public async Task clear_the_models_when_the_command_has_none()
    {
        LlmProviderConfig existing = LlmProviderConfigMother.With(
            provider: LlmProvider.OpenAi,
            models: [LlmModelOptionMother.Random(), LlmModelOptionMother.Random()]);
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindProviderConfig(LlmProvider.OpenAi, existing);

        await this._handler.Handle(
            LlmProviderRegistererCommandMother.With(LlmProvider.OpenAi, models: []),
            CancellationToken.None);

        existing.Models.Should().BeEmpty();
    }
}
