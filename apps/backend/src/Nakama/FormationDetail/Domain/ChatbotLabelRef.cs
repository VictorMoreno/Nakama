using Nakama.Shared.Domain;

namespace Nakama.FormationDetail.Domain;

/// <summary>
/// FormationDetail's own local denormalized copy of a chatbot's label, keyed by <c>BotKey</c>.
/// Not exposed through <see cref="IFormationDetailRepository"/>/Application - it exists purely
/// so <see cref="Infrastructure.Projections.FormationDetailProjection"/> can resolve
/// <c>Chatbot.Label</c> for the "Basado en la documentación de X" caption without this module's
/// Infrastructure ever touching another module's Domain entities (only its Domain/Events,
/// per the plan's dependency rule) or another read module's repository.
/// Label never changes after creation - there is no <c>ChatbotLabelChanged</c> domain event,
/// only <see cref="ChatbotCreated"/> carries it - so this entity has no other Apply method.
/// </summary>
public partial class ChatbotLabelRef : IEntity
{
    protected ChatbotLabelRef() { } // NHibernate

    public virtual string BotKey { get; protected set; } = string.Empty;
    public virtual string Label { get; protected set; } = string.Empty;
}
