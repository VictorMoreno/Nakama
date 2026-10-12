using System.Linq.Expressions;
using Moq;
using Nakama.LlmProviders.Domain;
using Test.Modules.Shared;

namespace Test.Modules.LlmProviders;

public abstract class LlmProvidersUnitTestCase : UnitTestCase
{
    protected Mock<ILlmProviderConfigRepository> ProviderConfigsRepository { get; } = new();

    [SetUp]
    public void LlmProvidersSetUp() => this.ProviderConfigsRepository.Reset();

    protected void ShouldFindProviderConfig(LlmProvider provider, LlmProviderConfig? config) =>
        this.ProviderConfigsRepository
            .Setup(repository => repository.FindByProvider(provider, It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

    protected void ShouldFindAllProviderConfigs(IReadOnlyList<LlmProviderConfig> configs) =>
        this.ProviderConfigsRepository
            .Setup(repository => repository.FindAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(configs);

    protected void ShouldHaveSavedProviderConfig(LlmProviderConfig config) =>
        this.ProviderConfigsRepository.Verify(
            repository => repository.Save(config, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveSavedProviderConfigMatching(Expression<Func<LlmProviderConfig, bool>> matches) =>
        this.ProviderConfigsRepository.Verify(
            repository => repository.Save(It.Is<LlmProviderConfig>(matches), It.IsAny<CancellationToken>()),
            Times.Once);
}
