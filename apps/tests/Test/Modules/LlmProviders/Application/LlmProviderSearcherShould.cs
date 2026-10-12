using AwesomeAssertions;
using Nakama.LlmProviders.Application;
using Nakama.LlmProviders.Application.Searcher;
using Nakama.LlmProviders.Domain;
using Test.Modules.LlmProviders.Domain;

namespace Test.Modules.LlmProviders.Application;

[TestFixture]
public sealed class LlmProviderSearcherShould : LlmProvidersUnitTestCase
{
    private LlmProviderSearcher.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new LlmProviderSearcher.Handler(new(this.ProviderConfigsRepository.Object));
    }

    [Test]
    public async Task return_a_response_per_config_with_its_provider_flags_and_models()
    {
        LlmModelOption model = LlmModelOptionMother.Random();
        LlmProviderConfig config = LlmProviderConfigMother.With(
            provider: LlmProvider.OpenAi,
            enabled: true,
            models: [model]);
        this.ShouldFindAllProviderConfigs([config]);

        IReadOnlyList<LlmProviderConfigResponse> response = await this._handler.Handle(
            new LlmProviderSearcher.Query(),
            CancellationToken.None);

        LlmProviderConfigResponse item = response.Should().ContainSingle().Which;
        item.Provider.Should().Be((int)LlmProvider.OpenAi);
        item.Configured.Should().BeTrue();
        item.Enabled.Should().BeTrue();
        item.Models.Should().Equal(new LlmModelOptionResponse(model.Id, model.Label));
        item.UpdatedAt.Should().Be(config.UpdatedAt);
    }

    [Test]
    public async Task report_the_provider_as_not_configured_when_it_has_no_api_key()
    {
        this.ShouldFindAllProviderConfigs([LlmProviderConfigMother.With(apiKey: string.Empty)]);

        IReadOnlyList<LlmProviderConfigResponse> response = await this._handler.Handle(
            new LlmProviderSearcher.Query(),
            CancellationToken.None);

        response.Should().ContainSingle().Which.Configured.Should().BeFalse();
    }

    [Test]
    public async Task return_an_empty_list_when_there_are_no_configs()
    {
        this.ShouldFindAllProviderConfigs([]);

        IReadOnlyList<LlmProviderConfigResponse> response = await this._handler.Handle(
            new LlmProviderSearcher.Query(),
            CancellationToken.None);

        response.Should().BeEmpty();
    }
}
