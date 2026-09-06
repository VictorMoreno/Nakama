namespace Nakama.Chatbots.Domain;

/// <summary>
/// Mirrors the frontend's <c>TeamRole</c>. The frontend distinguishes grammatical gender for
/// the owner label ("Propietario"/"Propietaria") for display purposes only - the domain models
/// a single <see cref="Owner"/> role; rendering the gendered label is a presentation concern.
/// </summary>
public enum TeamRole
{
    Owner,
    Editor,
    Reader,
}
