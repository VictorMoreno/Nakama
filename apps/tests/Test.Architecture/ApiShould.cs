using Api.Configuration;
using AwesomeAssertions;
using Test.Architecture.Support;

namespace Test.Architecture;

[TestFixture]
public sealed class ApiShould
{
    [Test]
    public void seal_every_endpoint_group_and_name_it_after_the_resource_in_the_endpoints_namespace()
    {
        NakamaCode.Api.GetTypes()
            .Where(type => type.IsClass && typeof(IEndpoint).IsAssignableFrom(type))
            .Where(type => !type.IsSealed
                           || !type.Name.EndsWith("Endpoints", StringComparison.Ordinal)
                           || type.Namespace != "Api.Endpoints")
            .Select(type => type.FullName)
            .Should().BeEmpty();
    }

    [Test]
    public void keep_the_endpoints_independent_of_the_infrastructure_of_the_modules()
    {
        NakamaCode.ApiTypes()
            .ResideInNamespace("Api.Endpoints")
            .ShouldNot().HaveDependencyOnAny(
                NakamaCode.Layers(NakamaCode.Modules, "Infrastructure"))
            .GetResult()
            .ShouldHaveNoViolations();
    }

    [Test]
    public void keep_the_endpoints_independent_of_the_persistence_layer()
    {
        NakamaCode.ApiTypes()
            .ResideInNamespace("Api.Endpoints")
            .ShouldNot().HaveDependencyOnAny("NHibernate", "Nakama.Shared.Infrastructure.Persistence", "FluentMigrator", "Npgsql")
            .GetResult()
            .ShouldHaveNoViolations();
    }

    [Test]
    public void keep_the_endpoints_independent_of_the_domain_of_the_modules()
    {
        NakamaCode.ApiTypes()
            .ResideInNamespace("Api.Endpoints")
            .ShouldNot().HaveDependencyOnAny(NakamaCode.Layers(NakamaCode.Modules, "Domain"))
            .GetResult()
            .ShouldHaveNoViolations();
    }
}
