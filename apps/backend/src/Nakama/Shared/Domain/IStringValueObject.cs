namespace Nakama.Shared.Domain;

/// <summary>
/// Implemented by every string-wrapping identity value object (<c>ChatbotKey</c>,
/// <c>ConversationId</c>, <c>FormationKey</c>...) so the shared NHibernate
/// <c>ValueObjectUserType&lt;T&gt;</c> can read/construct them generically, without
/// per-type reflection into private members.
/// </summary>
public interface IStringValueObject
{
    string Value { get; }
}
