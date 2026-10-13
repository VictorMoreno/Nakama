using System.Reflection;
using AwesomeAssertions;
using Nakama.Shared.Domain;
using Test.Architecture.Support;

namespace Test.Architecture;

[TestFixture]
public sealed class DomainShould
{
    [Test]
    public void expose_no_public_setters_in_any_entity()
    {
        NakamaCode.CoreClasses()
            .Where(type => typeof(IEntity).IsAssignableFrom(type) && !type.IsAbstract)
            .SelectMany(type => type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(property => property.SetMethod is { IsPublic: true })
                .Select(property => $"{type.FullName}.{property.Name}"))
            .Should().BeEmpty();
    }

    [Test]
    public void keep_every_entity_inside_the_domain_namespace_of_its_module()
    {
        NakamaCode.CoreClasses()
            .Where(type => typeof(IEntity).IsAssignableFrom(type) && !type.IsAbstract)
            .Where(type => !(type.Namespace ?? string.Empty).EndsWith(".Domain", StringComparison.Ordinal))
            .Select(type => type.FullName)
            .Should().BeEmpty();
    }

    [Test]
    public void give_every_entity_a_protected_parameterless_constructor_for_the_persistence_layer()
    {
        NakamaCode.CoreClasses()
            .Where(type => typeof(IEntity).IsAssignableFrom(type) && !type.IsAbstract)
            .Where(type => type.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                Type.EmptyTypes,
                null) is not { IsFamily: true })
            .Select(type => type.FullName)
            .Should().BeEmpty();
    }

    [Test]
    public void give_every_entity_a_public_constructor_that_takes_all_its_state()
    {
        NakamaCode.CoreClasses()
            .Where(type => typeof(IEntity).IsAssignableFrom(type) && !type.IsAbstract)
            .Where(type => type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).All(constructor => constructor.GetParameters().Length == 0))
            .Select(type => type.FullName)
            .Should().BeEmpty();
    }
}
