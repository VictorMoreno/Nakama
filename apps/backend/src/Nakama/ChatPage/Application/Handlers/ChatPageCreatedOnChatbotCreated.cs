using Nakama.ChatPage.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatPage.Application.Handlers;

using ChatPage = Nakama.ChatPage.Domain.ChatPage;

public sealed class ChatPageCreatedOnChatbotCreated(IChatPageRepository chatPageRepository)
    : DomainEventHandler<ChatbotCreated>
{
    protected override Task Handle(ChatbotCreated chatbotCreated, CancellationToken ct) =>
        chatPageRepository.Save(
            ChatPage.Create(
                chatbotCreated.Id.Value,
                chatbotCreated.Label,
                chatbotCreated.Settings.Socratic,
                chatbotCreated.Settings.ExplicaMas,
                chatbotCreated.Settings.Provider.ToString()),
            ct);
}
