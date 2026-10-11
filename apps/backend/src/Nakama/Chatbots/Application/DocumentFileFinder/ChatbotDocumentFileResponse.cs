namespace Nakama.Chatbots.Application.DocumentFileFinder;

public sealed record ChatbotDocumentFileResponse(byte[] Content, string ContentType, string FileName);
