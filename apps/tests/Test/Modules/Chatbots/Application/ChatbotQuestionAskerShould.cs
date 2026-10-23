using AwesomeAssertions;
using Nakama.ChatbotDetail.Application;
using Nakama.ChatbotDetail.Domain.Errors;
using Nakama.Chatbots.Application.QuestionAsker;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Domain;
using Test.Modules.ChatbotDetail.Application;
using Test.Modules.Chatbots.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ChatbotQuestionAskerShould : ChatbotsUnitTestCase
{
    private ChatbotQuestionAsker.Asker _asker = null!;

    [SetUp]
    public void SetUp()
    {
        this._asker = new ChatbotQuestionAsker.Asker(
            this.QueryBus.Object,
            this.ConversationsRepository.Object,
            this.DocumentFileReader.Object,
            this.AttachmentStorage.Object,
            this.ChatOptions,
            this.ServiceProvider.Object,
            this.EventBus.Object,
            this.Clock.Object);
    }

    [Test]
    public async Task start_a_new_conversation_stream_the_answer_and_publish_the_events()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.Random();
        ChatbotQuestionAsker.Command command = ChatbotQuestionAskerCommandMother.With(botKey: chatbot.Id);
        DateTime now = DateTime.UtcNow;
        this.ShouldBeNow(now);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldNotFindConversation();
        this.ShouldAnswer(chatbot.Provider, "Hel", "lo");

        List<string> chunks = await this._asker.Execute(command, CancellationToken.None).ToListAsync();

        chunks.Should().Equal("Hel", "lo");
        this.AskedRequest.Model.Should().Be(chatbot.Model);
        this.ShouldHaveSavedConversationMatching(conversation =>
            conversation.Id.Value == command.ConversationId
            && conversation.BotKey.Value == command.BotKey
            && conversation.UserId == command.UserId
            && conversation.Title == command.Title
            && conversation.StartedAt == now
            && conversation.Exchanges.Count == 1
            && conversation.Exchanges.First().Question == command.Question
            && conversation.Exchanges.First().Answer == "Hello");
        this.ShouldHavePublished<ConversationStarted>(@event => @event.Id.Value == command.ConversationId);
        this.ShouldHavePublished<ConversationExchangeAppended>(@event => @event.Exchange.Answer == "Hello" && @event.OccurredAt == now);
    }

    [Test]
    public async Task append_the_exchange_to_the_existing_conversation_and_send_its_history_to_the_model()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.Random();
        ChatbotQuestionAsker.Command command = ChatbotQuestionAskerCommandMother.With(botKey: chatbot.Id);
        ConversationExchange previous = ConversationExchangeMother.Random();
        Conversation conversation = ConversationMother.With(
            id: ConversationId.From(command.ConversationId),
            userId: command.UserId,
            exchanges: [previous]);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldFindConversation(conversation);
        this.ShouldAnswer(chatbot.Provider, "ok");

        await this._asker.Execute(command, CancellationToken.None).ToListAsync();

        this.AskedRequest.History.Select(message => (message.Role, message.Content)).Should().Equal(
            ("user", previous.Question),
            ("assistant", previous.Answer),
            ("user", command.Question));
        conversation.Exchanges.Should().HaveCount(2);
        conversation.Exchanges.Last().Question.Should().Be(command.Question);
        conversation.DomainEvents.OfType<ConversationStarted>().Should().BeEmpty();
        this.ShouldHaveSavedConversation(conversation);
        this.ShouldHavePublished<ConversationExchangeAppended>(@event => @event.Exchange.Answer == "ok");
    }

    [Test]
    public async Task fail_without_touching_anything_when_the_provider_is_not_connected()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.Random();
        ChatbotQuestionAsker.Command command = ChatbotQuestionAskerCommandMother.WithAttachmentCount(1, chatbot.Id);
        this.ShouldFindChatbotDetail(chatbot);

        Func<Task> act = async () => await this._asker.Execute(command, CancellationToken.None).ToListAsync();

        await act.Should().ThrowAsync<ChatbotProviderNotConnectedError>();
        this.ShouldNotHaveAskedTheModel();
        this.ShouldNotHaveStoredAnyAttachment();
        this.ShouldNotHaveSavedAnyConversation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_touching_anything_when_the_chatbot_detail_does_not_exist()
    {
        ChatbotQuestionAsker.Command command = ChatbotQuestionAskerCommandMother.WithAttachmentCount(1);
        this.ShouldNotFindChatbotDetail(command.BotKey);

        Func<Task> act = async () => await this._asker.Execute(command, CancellationToken.None).ToListAsync();

        await act.Should().ThrowAsync<ChatbotDetailNotFoundError>();
        this.ShouldNotHaveAskedTheModel();
        this.ShouldNotHaveStoredAnyAttachment();
        this.ShouldNotHaveSavedAnyConversation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_touching_anything_when_the_attachments_exceed_the_limit()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.Random();
        ChatbotQuestionAsker.Command command = ChatbotQuestionAskerCommandMother.WithAttachmentCount(
            this.ChatOptions.Value.MaxAttachmentsPerMessage + 1,
            chatbot.Id);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldAnswer(chatbot.Provider, "ok");

        Func<Task> act = async () => await this._asker.Execute(command, CancellationToken.None).ToListAsync();

        await act.Should().ThrowAsync<ChatbotAttachmentLimitExceededError>();
        this.ShouldNotHaveAskedTheModel();
        this.ShouldNotHaveStoredAnyAttachment();
        this.ShouldNotHaveSavedAnyConversation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task accept_exactly_the_maximum_number_of_attachments()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.Random();
        int maximum = this.ChatOptions.Value.MaxAttachmentsPerMessage;
        ChatbotQuestionAsker.Command command = ChatbotQuestionAskerCommandMother.WithAttachmentCount(maximum, chatbot.Id);
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldNotFindConversation();
        this.ShouldStoreAttachment($"{Guid.NewGuid():N}.pdf");
        this.ShouldAnswer(chatbot.Provider, "ok");

        await this._asker.Execute(command, CancellationToken.None).ToListAsync();

        this.ShouldHaveSavedConversationMatching(conversation =>
            conversation.Exchanges.First().Attachments!.Count == maximum);
    }

    [Test]
    public async Task fail_without_touching_anything_when_the_conversation_belongs_to_another_user()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.Random();
        ChatbotQuestionAsker.Command command = ChatbotQuestionAskerCommandMother.WithAttachmentCount(1, chatbot.Id);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldFindConversation(ConversationMother.With(id: ConversationId.From(command.ConversationId)));
        this.ShouldAnswer(chatbot.Provider, "ok");

        Func<Task> act = async () => await this._asker.Execute(command, CancellationToken.None).ToListAsync();

        await act.Should().ThrowAsync<ConversationNotFoundError>();
        this.ShouldNotHaveAskedTheModel();
        this.ShouldNotHaveStoredAnyAttachment();
        this.ShouldNotHaveSavedAnyConversation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task store_the_attachments_send_them_to_the_model_and_record_them_in_the_exchange()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.Random();
        ChatbotQuestionAsker.AttachmentUpload upload = AttachmentUploadMother.With(fileName: "report.pdf");
        ChatbotQuestionAsker.Command command = ChatbotQuestionAskerCommandMother.With(botKey: chatbot.Id, attachments: [upload]);
        string storedFileName = $"{Guid.NewGuid():N}.pdf";
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldNotFindConversation();
        this.ShouldStoreAttachment(storedFileName);
        this.ShouldAnswer(chatbot.Provider, "ok");

        await this._asker.Execute(command, CancellationToken.None).ToListAsync();

        this.ShouldHaveStoredAttachment(upload.FileName, upload.Content);
        this.AskedRequest.History[^1].Documents.Should().Equal(
            new LlmDocument(upload.FileName, LlmDocumentKind.Pdf, upload.Content));
        this.ShouldHaveSavedConversationMatching(conversation =>
            conversation.Exchanges.First().Attachments!.Count == 1
            && conversation.Exchanges.First().Attachments!.First().Name == upload.FileName
            && conversation.Exchanges.First().Attachments!.First().FileName == storedFileName);
        this.ShouldNotHaveDeletedAnyAttachment();
    }

    [Test]
    public async Task not_send_attachments_with_an_unsupported_extension_to_the_model()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.Random();
        ChatbotQuestionAsker.Command command = ChatbotQuestionAskerCommandMother.With(
            botKey: chatbot.Id,
            attachments: [AttachmentUploadMother.With(fileName: "program.exe")]);
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldNotFindConversation();
        this.ShouldStoreAttachment($"{Guid.NewGuid():N}.exe");
        this.ShouldAnswer(chatbot.Provider, "ok");

        await this._asker.Execute(command, CancellationToken.None).ToListAsync();

        this.AskedRequest.History[^1].Documents.Should().BeEmpty();
    }

    [Test]
    public async Task delete_the_stored_attachments_and_save_nothing_when_the_stream_fails()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.Random();
        ChatbotQuestionAsker.Command command = ChatbotQuestionAskerCommandMother.WithAttachmentCount(1, chatbot.Id);
        string storedFileName = $"{Guid.NewGuid():N}.pdf";
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldNotFindConversation();
        this.ShouldStoreAttachment(storedFileName);
        this.ShouldFailAfter(chatbot.Provider, ["partial"], new InvalidOperationException(WordMother.Random()));

        Func<Task> act = async () => await this._asker.Execute(command, CancellationToken.None).ToListAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
        this.ShouldHaveDeletedAttachments([storedFileName]);
        this.ShouldNotHaveSavedAnyConversation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task delete_the_attachments_already_stored_when_storing_a_later_one_fails()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.Random();
        ChatbotQuestionAsker.Command command = ChatbotQuestionAskerCommandMother.WithAttachmentCount(2, chatbot.Id);
        string firstFileName = $"{Guid.NewGuid():N}.pdf";
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldNotFindConversation();
        this.ShouldFailToStoreSecondAttachment(firstFileName, ChatbotErrors.DocumentUnsupportedType);
        this.ShouldAnswer(chatbot.Provider, "ok");

        Func<Task> act = async () => await this._asker.Execute(command, CancellationToken.None).ToListAsync();

        await act.Should().ThrowAsync<ChatbotDocumentUnsupportedTypeError>();
        this.ShouldHaveDeletedAttachments([firstFileName]);
        this.ShouldNotHaveAskedTheModel();
        this.ShouldNotHaveSavedAnyConversation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task include_the_attachments_of_previous_exchanges_in_the_history()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.Random();
        ChatbotQuestionAsker.Command command = ChatbotQuestionAskerCommandMother.With(botKey: chatbot.Id);
        ExchangeAttachment attachment = ExchangeAttachmentMother.Random();
        byte[] content = BytesMother.Random();
        Conversation conversation = ConversationMother.With(
            id: ConversationId.From(command.ConversationId),
            userId: command.UserId,
            exchanges: [ConversationExchangeMother.With(attachments: [attachment])]);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldFindConversation(conversation);
        this.ShouldReadAttachment(content);
        this.ShouldAnswer(chatbot.Provider, "ok");

        await this._asker.Execute(command, CancellationToken.None).ToListAsync();

        this.AskedRequest.History[0].Documents.Should().Equal(
            new LlmDocument(attachment.Name, LlmDocumentKind.Pdf, content));
    }

    [Test]
    public async Task skip_the_attachments_of_previous_exchanges_whose_file_is_missing()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.Random();
        ChatbotQuestionAsker.Command command = ChatbotQuestionAskerCommandMother.With(botKey: chatbot.Id);
        Conversation conversation = ConversationMother.With(
            id: ConversationId.From(command.ConversationId),
            userId: command.UserId,
            exchanges: [ConversationExchangeMother.With(attachments: [ExchangeAttachmentMother.Random()])]);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldFindConversation(conversation);
        this.ShouldReadAttachment(null);
        this.ShouldAnswer(chatbot.Provider, "ok");

        await this._asker.Execute(command, CancellationToken.None).ToListAsync();

        this.AskedRequest.History[0].Documents.Should().BeEmpty();
        this.ShouldHaveSavedConversation(conversation);
    }

    [Test]
    [TestCase(".pdf", LlmDocumentKind.Pdf)]
    [TestCase(".md", LlmDocumentKind.PlainText)]
    [TestCase(".markdown", LlmDocumentKind.PlainText)]
    [TestCase(".txt", LlmDocumentKind.PlainText)]
    public async Task send_the_reference_documents_of_the_chatbot_to_the_model(string extension, LlmDocumentKind kind)
    {
        ChatbotDocumentResponse document = ChatbotDocumentResponseMother.With(
            fileName: $"{Guid.NewGuid():N}{extension}");
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.With(docs: [document]);
        byte[] content = BytesMother.Random();
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldNotFindConversation();
        this.ShouldReadDocumentFile(content);
        this.ShouldAnswer(chatbot.Provider, "ok");

        await this._asker.Execute(ChatbotQuestionAskerCommandMother.With(botKey: chatbot.Id), CancellationToken.None).ToListAsync();

        this.AskedRequest.Documents.Should().Equal(new LlmDocument(document.Name, kind, content));
        this.AskedRequest.SystemPrompt.Should().Contain("documentación de referencia");
    }

    [Test]
    public async Task skip_the_reference_documents_whose_file_is_missing()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.With(docs: [ChatbotDocumentResponseMother.Random()]);
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldNotFindConversation();
        this.ShouldReadDocumentFile(null);
        this.ShouldAnswer(chatbot.Provider, "ok");

        await this._asker.Execute(ChatbotQuestionAskerCommandMother.With(botKey: chatbot.Id), CancellationToken.None).ToListAsync();

        this.AskedRequest.Documents.Should().BeEmpty();
        this.AskedRequest.SystemPrompt.Should().NotContain("documentación de referencia");
    }

    [Test]
    public async Task not_read_the_reference_documents_with_an_unsupported_extension()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.With(
            docs: [ChatbotDocumentResponseMother.With(fileName: "program.exe")]);
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldNotFindConversation();
        this.ShouldAnswer(chatbot.Provider, "ok");

        await this._asker.Execute(ChatbotQuestionAskerCommandMother.With(botKey: chatbot.Id), CancellationToken.None).ToListAsync();

        this.ShouldNotHaveReadAnyDocumentFile();
        this.AskedRequest.Documents.Should().BeEmpty();
    }

    [Test]
    public async Task introduce_the_chatbot_in_the_system_prompt_with_its_label_and_description()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.Random();
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldNotFindConversation();
        this.ShouldAnswer(chatbot.Provider, "ok");

        await this._asker.Execute(ChatbotQuestionAskerCommandMother.With(botKey: chatbot.Id), CancellationToken.None).ToListAsync();

        this.AskedRequest.SystemPrompt.Should().Contain(chatbot.Label).And.Contain(chatbot.Description);
    }

    [Test]
    public async Task leave_the_description_out_of_the_system_prompt_when_it_is_blank()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.With(description: " ");
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldNotFindConversation();
        this.ShouldAnswer(chatbot.Provider, "ok");

        await this._asker.Execute(ChatbotQuestionAskerCommandMother.With(botKey: chatbot.Id), CancellationToken.None).ToListAsync();

        this.AskedRequest.SystemPrompt.Split("\n\n").Should().HaveCount(2);
    }

    [Test]
    public async Task ask_for_socratic_guidance_in_the_system_prompt_only_when_it_is_enabled()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.With(socratic: true, explicaMas: false);
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldNotFindConversation();
        this.ShouldAnswer(chatbot.Provider, "ok");

        await this._asker.Execute(ChatbotQuestionAskerCommandMother.With(botKey: chatbot.Id), CancellationToken.None).ToListAsync();

        this.AskedRequest.SystemPrompt.Should().Contain("Socrático").And.NotContain("explícame más");
    }

    [Test]
    public async Task ask_for_deeper_explanations_in_the_system_prompt_only_when_it_is_enabled()
    {
        ChatbotDetailResponse chatbot = ChatbotDetailResponseMother.With(socratic: false, explicaMas: true);
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindChatbotDetail(chatbot);
        this.ShouldNotFindConversation();
        this.ShouldAnswer(chatbot.Provider, "ok");

        await this._asker.Execute(ChatbotQuestionAskerCommandMother.With(botKey: chatbot.Id), CancellationToken.None).ToListAsync();

        this.AskedRequest.SystemPrompt.Should().Contain("explícame más").And.NotContain("Socrático");
    }
}
