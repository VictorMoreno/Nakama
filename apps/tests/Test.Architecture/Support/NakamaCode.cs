using System.Reflection;
using Api.Configuration;
using AwesomeAssertions;
using Nakama.Shared.Domain;
using NetArchTest.Rules;

namespace Test.Architecture.Support;

public static class NakamaCode
{
    public static Assembly Core { get; } = typeof(IEntity).Assembly;

    public static Assembly Api { get; } = typeof(IEndpoint).Assembly;

    public static IReadOnlyList<string> Modules { get; } = Core
        .GetTypes()
        .Select(type => type.Namespace)
        .Where(ns => ns is not null && ns.StartsWith("Nakama.", StringComparison.Ordinal))
        .Select(ns => ns!.Split('.')[1])
        .Where(module => module != "Shared")
        .Distinct()
        .Order()
        .ToList();

    public static IEnumerable<string> ModuleNames => Modules;

    public static IReadOnlyDictionary<string, string[]> AllowedDomainDependencies { get; } =
        new Dictionary<string, string[]> { ["Chatbots"] = ["LlmProviders"] };

    public static string[] Frameworks { get; } =
    [
        "NHibernate",
        "MediatR",
        "Microsoft.AspNetCore",
        "Microsoft.Extensions",
        "FluentMigrator",
        "Npgsql",
        "Anthropic",
        "Scrutor"
    ];

    public static string[] InfrastructureOfEveryModule { get; } =
        [.. Layers(Modules, "Infrastructure"), "Nakama.Shared.Infrastructure"];

    public static string[] Layers(IEnumerable<string> modules, params string[] layers) => modules
        .SelectMany(module => layers.Select(layer => $"Nakama.{module}.{layer}"))
        .ToArray();

    public static string[] OtherModules(string module, params string[] allowed) =>
        Layers(Modules.Where(other => other != module && !allowed.Contains(other)), "Domain", "Application", "Infrastructure");

    public static Predicates CoreTypes() => Types.InAssembly(Core).That();

    public static Predicates ApiTypes() => Types.InAssembly(Api).That();

    public static IEnumerable<Type> CoreClasses() => Core.GetTypes().Where(type => type.IsClass && !IsCompilerGenerated(type));

    public static bool IsCompilerGenerated(Type type) =>
        type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)
        || (type.DeclaringType is not null && IsCompilerGenerated(type.DeclaringType));

    public static void ShouldHaveNoViolations(this TestResult result) =>
        (result.FailingTypeNames ?? []).Should().BeEmpty();
}
