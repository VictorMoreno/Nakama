using Nakama.ChatbotDetail.Domain;
using Test.Modules.Shared;

namespace Test.Modules.ChatbotDetail.Domain;

public static class StatTileRowMother
{
    public static StatTileRow Random() => With();

    public static StatTileRow With(string? label = null, string? value = null, string? unit = null, int? bar = null) =>
        new()
        {
            Label = label ?? WordMother.Random(),
            Value = value ?? WordMother.Random(),
            Unit = unit ?? WordMother.Random(),
            Bar = bar ?? MotherCreator.Random.Random.Number(0, 100)
        };
}
