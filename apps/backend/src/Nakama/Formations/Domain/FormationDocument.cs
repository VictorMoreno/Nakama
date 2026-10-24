namespace Nakama.Formations.Domain;

public sealed record FormationDocument(
    Guid Id,
    string Name,
    string Origin,
    DateTime Updated,
    string FileName);
