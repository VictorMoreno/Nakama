using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nakama.ChatbotDetail.Application;
using Nakama.ChatbotDetail.Application.Finder;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Chat;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.QuestionAsker;

public static class ChatbotQuestionAsker
{
    public sealed record AttachmentUpload(string FileName, byte[] Content);

    public sealed record Command(
        Guid BotKey,
        Guid ConversationId,
        Guid UserId,
        string Title,
        string Question,
        IReadOnlyList<AttachmentUpload> Attachments);

    public sealed class Asker(
        IQueryBus queryBus,
        IConversationsRepository conversationsRepository,
        IChatbotDocumentFileReader documentReader,
        IConversationAttachmentStorage attachmentStorage,
        IOptions<ChatOptions> chatOptions,
        IServiceProvider serviceProvider,
        IEventBus eventBus,
        ISystemClock systemClock) : IService
    {
        private readonly ConversationFinderById _finder = new(conversationsRepository);

        public async IAsyncEnumerable<string> Execute(Command cmd, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ChatbotDetailResponse chatbot = await queryBus.Ask(new ChatbotDetailFinder.Query(cmd.BotKey), cancellationToken);

            ILlmClient llmClient = serviceProvider.GetKeyedService<ILlmClient>(chatbot.Provider)
                                   ?? throw ChatbotErrors.ProviderNotConnected(chatbot.Provider);

            if (cmd.Attachments.Count > chatOptions.Value.MaxAttachmentsPerMessage)
            {
                throw ChatbotErrors.AttachmentLimitExceeded(chatOptions.Value.MaxAttachmentsPerMessage);
            }

            ChatbotId botKey = ChatbotId.From(cmd.BotKey);
            ConversationId conversationId = ConversationId.From(cmd.ConversationId);
            Conversation conversation = await this._finder.FindOwned(conversationId, cmd.UserId, cancellationToken)
                                        ?? Conversation.Start(conversationId, botKey, cmd.UserId, cmd.Title,
                                            systemClock);

            List<ExchangeAttachment> savedAttachments = [];
            bool completed = false;
            try
            {
                await this.SaveAttachments(cmd.Attachments, savedAttachments, cancellationToken);

                List<LlmDocument> documents = await this.LoadDocuments(chatbot, cancellationToken);
                string systemPrompt = BuildSystemPrompt(chatbot, documents.Count > 0);
                List<LlmMessage> history = await this.BuildHistory(conversation, cmd, cancellationToken);

                StringBuilder answerBuilder = new();
                await foreach (string chunk in llmClient.AskStream(chatbot.Model, systemPrompt, history, documents, cancellationToken))
                {
                    answerBuilder.Append(chunk);
                    yield return chunk;
                }

                string answer = answerBuilder.ToString();
                conversation.AppendExchange(new ConversationExchange(cmd.Question, answer, savedAttachments), systemClock);

                await conversationsRepository.Save(conversation, cancellationToken);
                await eventBus.Publish(conversation, cancellationToken);
                completed = true;
            }
            finally
            {
                if (!completed)
                {
                    attachmentStorage.Delete(savedAttachments.Select(a => a.FileName));
                }
            }
        }

        private async Task SaveAttachments(
            IReadOnlyList<AttachmentUpload> uploads,
            List<ExchangeAttachment> saved,
            CancellationToken ct)
        {
            foreach (AttachmentUpload upload in uploads)
            {
                string fileName = await attachmentStorage.Save(Guid.NewGuid(), upload.FileName, upload.Content, ct);
                saved.Add(new ExchangeAttachment(upload.FileName, fileName));
            }
        }

        private async Task<List<LlmMessage>> BuildHistory(Conversation conversation, Command cmd, CancellationToken ct)
        {
            List<LlmMessage> history = [];

            foreach (ConversationExchange exchange in conversation.Exchanges)
            {
                List<LlmDocument> documents = await this.LoadAttachmentDocuments(exchange.Attachments ?? [], ct);
                history.Add(new LlmMessage("user", exchange.Question, documents));
                history.Add(new LlmMessage("assistant", exchange.Answer));
            }

            List<LlmDocument> current = cmd.Attachments
                .Select(a => ToDocument(a.FileName, a.FileName, a.Content))
                .OfType<LlmDocument>()
                .ToList();
            history.Add(new LlmMessage("user", cmd.Question, current));
            return history;
        }

        private async Task<List<LlmDocument>> LoadAttachmentDocuments(IReadOnlyList<ExchangeAttachment> attachments, CancellationToken ct)
        {
            List<LlmDocument> documents = [];

            foreach (ExchangeAttachment attachment in attachments)
            {
                byte[]? content = await attachmentStorage.Read(attachment.FileName, ct);
                LlmDocument? document = content is null ? null : ToDocument(attachment.Name, attachment.FileName, content);
                if (document is not null)
                {
                    documents.Add(document);
                }
            }

            return documents;
        }

        private static LlmDocument? ToDocument(string name, string fileName, byte[] content)
        {
            LlmDocumentKind? kind = ResolveKind(fileName);
            return kind is null ? null : new LlmDocument(name, kind.Value, content);
        }

        private async Task<List<LlmDocument>> LoadDocuments(ChatbotDetailResponse chatbot, CancellationToken ct)
        {
            List<LlmDocument> documents = new();

            foreach (ChatbotDocumentResponse document in chatbot.Docs)
            {
                string fileName = document.FileName;
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
            List<string> parts = new() { $"Eres el asistente del chatbot \"{chatbot.Label}\"." };

            if (!string.IsNullOrWhiteSpace(chatbot.Description))
            {
                parts.Add(chatbot.Description);
            }

            if (hasDocuments)
            {
                parts.Add("Los documentos adjuntos al inicio de la conversación son tu documentación de referencia.");
            }

            parts.Add("Si el usuario adjunta archivos a un mensaje, úsalos como contexto para responder ese mensaje y los siguientes.");

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
