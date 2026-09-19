namespace Nakama.Chatbots.Domain;

public sealed record TeamMember(string Name, TeamRole Role)
{
    public bool IsOwner => this.Role == TeamRole.Owner;
}
