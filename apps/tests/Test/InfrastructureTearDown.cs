using Test.Modules.Shared.Infrastructure;

namespace Test;

[SetUpFixture]
public sealed class InfrastructureTearDown
{
    [OneTimeTearDown]
    public async Task StopPostgresContainer() => await PostgresContainer.Stop();
}
