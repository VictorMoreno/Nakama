using Microsoft.Extensions.DependencyInjection;
using Nakama.ChatbotDetail.Application;
using Nakama.ChatbotDetail.Application.Finder;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.QuestionAsker;

public static class ChatbotQuestionAsker
{
    public sealed record Command(Guid BotKey, Guid ConversationId, Guid UserId, string Title, string Question);

    public sealed class Asker(
        IQueryBus queryBus,
        IConversationsRepository conversationsRepository,
        IChatbotDocumentContentReader documentReader,
        IServiceProvider serviceProvider,
        IEventBus eventBus,
        ISystemClock systemClock) : IService
    {
        public async Task<string> Execute(Command cmd, CancellationToken cancellationToken)
        {
            ChatbotDetailResponse chatbot =
                await queryBus.Ask(new ChatbotDetailFinder.Query(cmd.BotKey), cancellationToken);

            ILlmClient llmClient = serviceProvider.GetKeyedService<ILlmClient>(chatbot.Provider)
                                   ?? throw ChatbotErrors.ProviderNotConnected(chatbot.Provider);

            ChatbotId botKey = ChatbotId.From(cmd.BotKey);
            ConversationId conversationId = ConversationId.From(cmd.ConversationId);
            Conversation conversation = await conversationsRepository.FindById(conversationId, cancellationToken)
                                        ?? Conversation.Start(conversationId, botKey, cmd.UserId, cmd.Title,
                                            systemClock);

            string systemPrompt = await BuildSystemPrompt(chatbot, cancellationToken);
            List<LlmMessage> history = BuildHistory(conversation, cmd.Question);

            string answer = await llmClient.Ask(chatbot.Model, systemPrompt, history, cancellationToken);

            conversation.AppendExchange(new ConversationExchange(cmd.Question, answer, string.Empty, string.Empty,
                string.Empty));

            await conversationsRepository.Save(conversation, cancellationToken);
            await eventBus.Publish(conversation, cancellationToken);

            return answer;
        }

        private static List<LlmMessage> BuildHistory(Conversation conversation, string question)
        {
            List<LlmMessage> history = conversation.Exchanges
                .SelectMany(e => new[] { new LlmMessage("user", e.Question), new LlmMessage("assistant", e.Answer) })
                .ToList();
            history.Add(new LlmMessage("user", question));
            return history;
        }

        private async Task<string> BuildSystemPrompt(ChatbotDetailResponse chatbot, CancellationToken ct)
        {
            List<string> parts = new List<string> { $"Eres el asistente del chatbot \"{chatbot.Label}\"." };

            if (!string.IsNullOrWhiteSpace(chatbot.Description))
            {
                parts.Add(chatbot.Description);
            }

            foreach (ChatbotDocumentResponse document in chatbot.Docs)
            {
                string fileName = Path.GetFileName(document.Url);
                string? text = await documentReader.ReadText(fileName, ct);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    parts.Add($"Documentación de referencia ({document.Name}):\n{text}");
                }
            }

            if (chatbot.Socratic)
            {
                parts.Add(
                    "Modo Socrático activo: guía con una pregunta en vez de responder directo, salvo que te pidan la respuesta directa explícitamente.");
            }

            if (chatbot.ExplicaMas)
            {
                parts.Add("Si el usuario pide \"explícame más\", profundiza con más detalle y ejemplos.");
            }

            return string.Join("\n\n", parts);
        }
    }
}