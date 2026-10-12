using Nakama.Analytics.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Analytics.Domain;

public static class UsageByBotRowMother
{
    public static UsageByBotRow Random() => With();

    public static UsageByBotRow With(Guid? botKey = null, int? value = null) =>
        new()
        {
            BotKey = botKey ?? Guid.NewGuid(),
            Value = value ?? MotherCreator.Random.Random.Number(1, 50)
        };
}
