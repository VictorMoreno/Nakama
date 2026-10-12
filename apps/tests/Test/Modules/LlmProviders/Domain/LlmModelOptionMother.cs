using Nakama.LlmProviders.Domain;
using Test.Modules.Shared;

namespace Test.Modules.LlmProviders.Domain;

public static class LlmModelOptionMother
{
    public static LlmModelOption Random() => With();

    public static LlmModelOption With(string? id = null, string? label = null) =>
        new(id ?? WordMother.Random(), label ?? WordMother.Random());
}
