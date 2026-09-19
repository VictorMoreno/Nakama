using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

public readonly record struct ChatbotId(Guid Value) : IGuidValueObject
{
    public static ChatbotId From(Guid value) => new(value);

    public override string ToString() => this.Value.ToString();
}
