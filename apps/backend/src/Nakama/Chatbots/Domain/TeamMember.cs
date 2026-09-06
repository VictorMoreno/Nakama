namespace Nakama.Chatbots.Domain;

public sealed record TeamMember(string Name, TeamRole Role)
{
    /// <summary>Domain invariant carrier: mirrors the frontend's <c>isOwnerRole</c>.</summary>
    public bool IsOwner => this.Role == TeamRole.Owner;
}
