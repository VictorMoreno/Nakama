using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

public readonly record struct ConversationId(Guid Value) : IGuidValueObject
{
    public static ConversationId From(Guid value) => new(value);

    public override string ToString() => this.Value.ToString();
}
