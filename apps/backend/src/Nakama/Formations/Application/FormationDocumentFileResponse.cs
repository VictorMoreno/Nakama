namespace Nakama.Formations.Application;

public sealed record FormationDocumentFileResponse(byte[] Content, string ContentType, string FileName);
