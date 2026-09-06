using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

public readonly record struct ConversationId(string Value) : IStringValueObject
{
    public static ConversationId From(string value) => new(value);

    public override string ToString() => this.Value;
}
