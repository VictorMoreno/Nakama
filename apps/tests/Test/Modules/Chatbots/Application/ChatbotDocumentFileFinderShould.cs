using AwesomeAssertions;
using Nakama.Chatbots.Application.DocumentFileFinder;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Test.Modules.Chatbots.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ChatbotDocumentFileFinderShould : ChatbotsUnitTestCase
{
    private ChatbotDocumentFileFinder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDocumentFileFinder.Handler(
            new(this.ChatbotsRepository.Object, this.DocumentFileReader.Object));
    }

    [Test]
    public async Task return_the_content_of_the_document_file_with_its_content_type_and_download_name()
    {
        byte[] content = MotherCreator.Random.Random.Bytes(16);
        ChatbotDocument document = ChatbotDocumentMother.With(name: "report", fileName: $"{Guid.NewGuid():N}.pdf");
        Chatbot chatbot = ChatbotMother.With(documents: [document]);
        this.ShouldFindChatbot(chatbot);
        this.ShouldReadDocumentFile(content);

        ChatbotDocumentFileResponse response = await this._handler.Handle(
            ChatbotDocumentFileFinderQueryMother.With(chatbot.Id.Value, document.Id),
            CancellationToken.None);

        response.Content.Should().Equal(content);
        response.ContentType.Should().Be("application/pdf");
        response.FileName.Should().Be("report.pdf");
    }

    [TestCase(".pdf", "application/pdf")]
    [TestCase(".PDF", "application/pdf")]
    [TestCase(".md", "text/markdown")]
    [TestCase(".markdown", "text/markdown")]
    [TestCase(".txt", "text/plain")]
    [TestCase(".docx", "application/octet-stream")]
    public async Task resolve_the_content_type_from_the_file_extension(string extension, string contentType)
    {
        ChatbotDocument document = ChatbotDocumentMother.With(fileName: $"{Guid.NewGuid():N}{extension}");
        Chatbot chatbot = ChatbotMother.With(documents: [document]);
        this.ShouldFindChatbot(chatbot);
        this.ShouldReadDocumentFile(MotherCreator.Random.Random.Bytes(16));

        ChatbotDocumentFileResponse response = await this._handler.Handle(
            ChatbotDocumentFileFinderQueryMother.With(chatbot.Id.Value, document.Id),
            CancellationToken.None);

        response.ContentType.Should().Be(contentType);
    }

    [Test]
    public async Task keep_the_document_name_when_it_already_ends_with_the_file_extension()
    {
        string name = $"{WordMother.Random()}.PDF";
        ChatbotDocument document = ChatbotDocumentMother.With(name: name, fileName: $"{Guid.NewGuid():N}.pdf");
        Chatbot chatbot = ChatbotMother.With(documents: [document]);
        this.ShouldFindChatbot(chatbot);
        this.ShouldReadDocumentFile(MotherCreator.Random.Random.Bytes(16));

        ChatbotDocumentFileResponse response = await this._handler.Handle(
            ChatbotDocumentFileFinderQueryMother.With(chatbot.Id.Value, document.Id),
            CancellationToken.None);

        response.FileName.Should().Be(name);
    }

    [Test]
    public async Task fail_when_the_chatbot_does_not_exist()
    {
        this.ShouldNotFindChatbot();

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotDocumentFileFinderQueryMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotNotFoundError>();
    }

    [Test]
    public async Task fail_when_the_document_does_not_belong_to_the_chatbot()
    {
        Chatbot chatbot = ChatbotMother.With(documents: [ChatbotDocumentMother.Random()]);
        this.ShouldFindChatbot(chatbot);

        Func<Task> act = async () => await this._handler.Handle(
            ChatbotDocumentFileFinderQueryMother.With(chatbot.Id.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotDocumentNotFoundError>();
    }

    [Test]
    public async Task fail_when_the_file_is_missing_from_the_storage()
    {
        ChatbotDocument document = ChatbotDocumentMother.Random();
        Chatbot chatbot = ChatbotMother.With(documents: [document]);
        this.ShouldFindChatbot(chatbot);
        this.ShouldReadDocumentFile(null);

        Func<Task> act = async () => await this._handler.Handle(
            ChatbotDocumentFileFinderQueryMother.With(chatbot.Id.Value, document.Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotDocumentNotFoundError>();
    }
}
