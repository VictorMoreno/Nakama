using System.Runtime.CompilerServices;
using System.Text;
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
        IChatbotDocumentFileReader documentReader,
        IServiceProvider serviceProvider,
        IEventBus eventBus,
        ISystemClock systemClock) : IService
    {
        public async IAsyncEnumerable<string> Execute(Command cmd, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ChatbotDetailResponse chatbot = await queryBus.Ask(new ChatbotDetailFinder.Query(cmd.BotKey), cancellationToken);

            ILlmClient llmClient = serviceProvider.GetKeyedService<ILlmClient>(chatbot.Provider)
                                   ?? throw ChatbotErrors.ProviderNotConnected(chatbot.Provider);

            ChatbotId botKey = ChatbotId.From(cmd.BotKey);
            ConversationId conversationId = ConversationId.From(cmd.ConversationId);
            Conversation conversation = await conversationsRepository.FindById(conversationId, cancellationToken)
                                        ?? Conversation.Start(conversationId, botKey, cmd.UserId, cmd.Title,
                                            systemClock);

            List<LlmDocument> documents = await this.LoadDocuments(chatbot, cancellationToken);
            string systemPrompt = BuildSystemPrompt(chatbot, documents.Count > 0);
            List<LlmMessage> history = BuildHistory(conversation, cmd.Question);

            StringBuilder answerBuilder = new();
            await foreach (string chunk in llmClient.AskStream(chatbot.Model, systemPrompt, history, documents, cancellationToken))
            {
                answerBuilder.Append(chunk);
                yield return chunk;
            }

            string answer = answerBuilder.ToString();
            conversation.AppendExchange(new ConversationExchange(cmd.Question, answer, string.Empty, string.Empty,
                string.Empty));

            await conversationsRepository.Save(conversation, cancellationToken);
            await eventBus.Publish(conversation, cancellationToken);
        }

        private static List<LlmMessage> BuildHistory(Conversation conversation, string question)
        {
            List<LlmMessage> history = conversation.Exchanges
                .SelectMany(e => new[] { new LlmMessage("user", e.Question), new LlmMessage("assistant", e.Answer) })
                .ToList();
            history.Add(new LlmMessage("user", question));
            return history;
        }

        private async Task<List<LlmDocument>> LoadDocuments(ChatbotDetailResponse chatbot, CancellationToken ct)
        {
            List<LlmDocument> documents = new();

            foreach (ChatbotDocumentResponse document in chatbot.Docs)
            {
                string fileName = Path.GetFileName(document.Url);
                LlmDocumentKind? kind = ResolveKind(fileName);
                if (kind is null)
                {
                    continue;
                }

                byte[]? content = await documentReader.Read(fileName, ct);
                if (content is not null)
                {
                    documents.Add(new LlmDocument(document.Name, kind.Value, content));
                }
            }

            return documents;
        }

        private static LlmDocumentKind? ResolveKind(string fileName) =>
            Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".pdf" => LlmDocumentKind.Pdf,
                ".md" or ".markdown" or ".txt" => LlmDocumentKind.PlainText,
                _ => null
            };

        private static string BuildSystemPrompt(ChatbotDetailResponse chatbot, bool hasDocuments)
        {
            List<string> parts = new List<string> { $"Eres el asistente del chatbot \"{chatbot.Label}\"." };

            if (!string.IsNullOrWhiteSpace(chatbot.Description))
            {
                parts.Add(chatbot.Description);
            }

            if (hasDocuments)
            {
                parts.Add("Los documentos adjuntos al inicio de la conversación son tu documentación de referencia.");
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
