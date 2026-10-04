using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.IconUpdater;

public static class ChatbotIconUpdater
{
    public sealed record Upload(string ContentType, Stream Content, long Length);

    public sealed record Command(Guid Id, Upload? Upload) : ICommand;

    public sealed class Handler(Updater updater) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await updater.Execute(ChatbotId.From(cmd.Id), cmd.Upload, ct);
        }
    }

    public sealed class Updater(
        IChatbotsRepository repository,
        IChatbotIconStorage iconStorage,
        IEventBus eventBus) : IService
    {
        private readonly ChatbotFinderById _finder = new(repository);

        public async Task Execute(ChatbotId id, Upload? upload, CancellationToken cancellationToken)
        {
            Chatbot chatbot = await this._finder.Execute(id, cancellationToken);

            string? iconFileName = null;
            if (upload is not null)
            {
                iconFileName = await iconStorage.Save(id.Value, upload.ContentType, upload.Content, upload.Length, cancellationToken);
            }
            else
            {
                iconStorage.DeleteExisting(id.Value);
            }

            chatbot.UpdateIcon(iconFileName);

            await repository.Save(chatbot, cancellationToken);
            await eventBus.Publish(chatbot, cancellationToken);
        }
    }
}
