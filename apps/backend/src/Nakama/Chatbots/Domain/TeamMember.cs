namespace Nakama.Chatbots.Domain;

public sealed record TeamMember(Guid UserId, TeamRole Role)
{
    public bool IsOwner => this.Role == TeamRole.Owner;
}
