using AwesomeAssertions;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;
using Test.Architecture.Support;

namespace Test.Architecture;

[TestFixture]
public sealed class NamingShould
{
    [Test]
    public void start_every_interface_name_with_i()
    {
        NakamaCode.Core.GetTypes()
            .Where(type => type.IsInterface && !NakamaCode.IsCompilerGenerated(type))
            .Where(type => !type.Name.StartsWith('I'))
            .Select(type => type.FullName)
            .Should().BeEmpty();
    }

    [Test]
    public void name_every_repository_implementation_nh_followed_by_the_interface_name()
    {
        NakamaCode.CoreClasses()
            .Where(type => !type.IsAbstract)
            .SelectMany(type => type.GetInterfaces()
                .Where(contract => contract.Name.EndsWith("Repository", StringComparison.Ordinal))
                .Select(contract => (Implementation: type, Contract: contract)))
            .Where(pair => pair.Implementation.Name != "Nh" + pair.Contract.Name[1..])
            .Select(pair => $"{pair.Implementation.FullName} implements {pair.Contract.Name}")
            .Should().BeEmpty();
    }

    [Test]
    public void keep_every_repository_implementation_in_the_infrastructure_of_its_module()
    {
        NakamaCode.CoreClasses()
            .Where(type => !type.IsAbstract)
            .Where(type => type.GetInterfaces().Any(contract => contract.Name.EndsWith("Repository", StringComparison.Ordinal)))
            .Where(type => !(type.Namespace ?? string.Empty).Contains(".Infrastructure", StringComparison.Ordinal))
            .Select(type => type.FullName)
            .Should().BeEmpty();
    }

    [Test]
    public void seal_every_domain_event_and_keep_it_in_the_events_namespace_of_the_domain()
    {
        NakamaCode.CoreClasses()
            .Where(type => typeof(IDomainEvent).IsAssignableFrom(type))
            .Where(type => !type.IsSealed || !(type.Namespace ?? string.Empty).EndsWith(".Domain.Events", StringComparison.Ordinal))
            .Select(type => type.FullName)
            .Should().BeEmpty();
    }

    [Test]
    public void declare_every_domain_event_as_a_record()
    {
        NakamaCode.CoreClasses()
            .Where(type => typeof(IDomainEvent).IsAssignableFrom(type))
            .Where(type => type.GetMethod("<Clone>$") is null)
            .Select(type => type.FullName)
            .Should().BeEmpty();
    }

    [Test]
    public void seal_every_domain_error_and_end_its_name_with_error()
    {
        NakamaCode.CoreClasses()
            .Where(type => typeof(DomainError).IsAssignableFrom(type) && type != typeof(DomainError))
            .Where(type => !type.IsSealed || !type.Name.EndsWith("Error", StringComparison.Ordinal))
            .Select(type => type.FullName)
            .Should().BeEmpty();
    }

    [Test]
    public void seal_every_event_handler_and_keep_it_in_the_handlers_namespace_named_after_the_event()
    {
        NakamaCode.CoreClasses()
            .Where(type => type.BaseType is { IsGenericType: true } baseType
                           && baseType.GetGenericTypeDefinition() == typeof(DomainEventHandler<>))
            .Where(type => !type.IsSealed
                           || !(type.Namespace ?? string.Empty).EndsWith(".Application.Handlers", StringComparison.Ordinal)
                           || !type.Name.Contains("On", StringComparison.Ordinal))
            .Select(type => type.FullName)
            .Should().BeEmpty();
    }

    [Test]
    public void declare_every_command_and_query_handler_as_a_sealed_nested_handler()
    {
        NakamaCode.CoreClasses()
            .Where(type => type.GetInterfaces().Any(contract => contract.IsGenericType
                && (contract.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                    || contract.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))))
            .Where(type => !type.IsSealed || !type.IsNested || type.Name != "Handler")
            .Select(type => type.FullName)
            .Should().BeEmpty();
    }

    [Test]
    public void seal_every_command_and_query()
    {
        NakamaCode.CoreClasses()
            .Where(type => typeof(ICommand).IsAssignableFrom(type)
                           || type.GetInterfaces().Any(contract => contract.IsGenericType
                                                                   && contract.GetGenericTypeDefinition() == typeof(IQuery<>)))
            .Where(type => !type.IsSealed)
            .Select(type => type.FullName)
            .Should().BeEmpty();
    }

    [Test]
    public void seal_every_application_service()
    {
        NakamaCode.CoreClasses()
            .Where(type => typeof(IService).IsAssignableFrom(type) && !type.IsAbstract)
            .Where(type => !type.IsSealed)
            .Select(type => type.FullName)
            .Should().BeEmpty();
    }

    [Test]
    public void inject_only_interfaces_or_generic_abstractions_into_the_application_services()
    {
        NakamaCode.CoreClasses()
            .Where(type => typeof(IService).IsAssignableFrom(type) && !type.IsAbstract)
            .SelectMany(type => type.GetConstructors().SelectMany(constructor => constructor.GetParameters()
                .Select(parameter => (Service: type, Parameter: parameter))))
            .Where(pair => !pair.Parameter.ParameterType.IsInterface && !pair.Parameter.ParameterType.IsGenericType)
            .Select(pair => $"{pair.Service.FullName}({pair.Parameter.ParameterType.Name} {pair.Parameter.Name})")
            .Should().BeEmpty();
    }
}
