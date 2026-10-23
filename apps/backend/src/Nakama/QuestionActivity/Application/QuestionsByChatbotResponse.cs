namespace Nakama.QuestionActivity.Application;

public sealed record ChatbotQuestionsRow(Guid BotKey, int Questions);

public sealed record QuestionsByChatbotResponse(IReadOnlyList<ChatbotQuestionsRow> Chatbots);
