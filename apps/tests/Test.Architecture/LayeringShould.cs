using Test.Architecture.Support;

namespace Test.Architecture;

[TestFixture]
public sealed class LayeringShould
{
    [Test]
    [TestCaseSource(typeof(NakamaCode), nameof(NakamaCode.ModuleNames))]
    public void keep_the_domain_of_a_module_independent_of_its_application_layer(string module)
    {
        NakamaCode.CoreTypes()
            .ResideInNamespaceStartingWith($"Nakama.{module}.Domain")
            .ShouldNot().HaveDependencyOn($"Nakama.{module}.Application")
            .GetResult()
            .ShouldHaveNoViolations();
    }

    [Test]
    [TestCaseSource(typeof(NakamaCode), nameof(NakamaCode.ModuleNames))]
    public void keep_the_domain_of_a_module_independent_of_every_infrastructure_layer(string module)
    {
        NakamaCode.CoreTypes()
            .ResideInNamespaceStartingWith($"Nakama.{module}.Domain")
            .ShouldNot().HaveDependencyOnAny(NakamaCode.InfrastructureOfEveryModule)
            .GetResult()
            .ShouldHaveNoViolations();
    }

    [Test]
    [TestCaseSource(typeof(NakamaCode), nameof(NakamaCode.ModuleNames))]
    public void keep_the_domain_of_a_module_free_of_frameworks(string module)
    {
        NakamaCode.CoreTypes()
            .ResideInNamespaceStartingWith($"Nakama.{module}.Domain")
            .ShouldNot().HaveDependencyOnAny(NakamaCode.Frameworks)
            .GetResult()
            .ShouldHaveNoViolations();
    }

    [Test]
    [TestCaseSource(typeof(NakamaCode), nameof(NakamaCode.ModuleNames))]
    public void keep_the_domain_of_a_module_independent_of_the_other_modules_except_the_documented_ones(string module)
    {
        string[] allowed = NakamaCode.AllowedDomainDependencies.GetValueOrDefault(module, []);

        NakamaCode.CoreTypes()
            .ResideInNamespaceStartingWith($"Nakama.{module}.Domain")
            .ShouldNot().HaveDependencyOnAny(NakamaCode.OtherModules(module, allowed))
            .GetResult()
            .ShouldHaveNoViolations();
    }

    [Test]
    [TestCaseSource(typeof(NakamaCode), nameof(NakamaCode.ModuleNames))]
    public void keep_the_application_of_a_module_independent_of_every_infrastructure_layer(string module)
    {
        NakamaCode.CoreTypes()
            .ResideInNamespaceStartingWith($"Nakama.{module}.Application")
            .ShouldNot().HaveDependencyOnAny(
                NakamaCode.Layers(NakamaCode.Modules, "Infrastructure"))
            .GetResult()
            .ShouldHaveNoViolations();
    }

    [Test]
    [TestCaseSource(typeof(NakamaCode), nameof(NakamaCode.ModuleNames))]
    public void keep_the_application_of_a_module_independent_of_the_nhibernate_unit_of_work(string module)
    {
        NakamaCode.CoreTypes()
            .ResideInNamespaceStartingWith($"Nakama.{module}.Application")
            .ShouldNot().HaveDependencyOn("Nakama.Shared.Infrastructure.Persistence")
            .GetResult()
            .ShouldHaveNoViolations();
    }

    [Test]
    [TestCaseSource(typeof(NakamaCode), nameof(NakamaCode.ModuleNames))]
    public void keep_the_application_of_a_module_free_of_persistence_web_and_provider_sdks(string module)
    {
        NakamaCode.CoreTypes()
            .ResideInNamespaceStartingWith($"Nakama.{module}.Application")
            .ShouldNot().HaveDependencyOnAny("NHibernate", "MediatR", "Microsoft.AspNetCore", "FluentMigrator", "Npgsql", "Anthropic")
            .GetResult()
            .ShouldHaveNoViolations();
    }

    [Test]
    public void keep_the_shared_domain_independent_of_the_modules_and_frameworks()
    {
        NakamaCode.CoreTypes()
            .ResideInNamespaceStartingWith("Nakama.Shared.Domain")
            .ShouldNot().HaveDependencyOnAny(
                [
                    .. NakamaCode.Layers(NakamaCode.Modules, "Domain", "Application", "Infrastructure"),
                    .. NakamaCode.Frameworks,
                    "Nakama.Shared.Infrastructure",
                    "Nakama.Shared.Application"
                ])
            .GetResult()
            .ShouldHaveNoViolations();
    }

    [Test]
    public void keep_the_shared_application_independent_of_the_modules_and_infrastructure()
    {
        NakamaCode.CoreTypes()
            .ResideInNamespaceStartingWith("Nakama.Shared.Application")
            .ShouldNot().HaveDependencyOnAny(
                [
                    .. NakamaCode.Layers(NakamaCode.Modules, "Domain", "Application", "Infrastructure"),
                    "Nakama.Shared.Infrastructure"
                ])
            .GetResult()
            .ShouldHaveNoViolations();
    }
}
