using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

public readonly record struct ChatbotKey(string Value) : IStringValueObject
{
    public static ChatbotKey From(string value) => new(value);

    public override string ToString() => this.Value;
}
