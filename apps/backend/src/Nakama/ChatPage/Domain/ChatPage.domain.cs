namespace Nakama.ChatPage.Domain;

public partial class ChatPage
{
    private ChatPage(Guid botKey, string label, bool socratic, bool explicaMas, string provider)
    {
        this.Id = botKey;
        this.Label = label;
        this.Socratic = socratic;
        this.ExplicaMas = explicaMas;
        this.Provider = provider;
    }

    public static ChatPage Create(Guid botKey, string label, bool socratic, bool explicaMas, string provider) =>
        new(botKey, label, socratic, explicaMas, provider);

    public void UpdateSettings(bool socratic, bool explicaMas, string provider)
    {
        this.Socratic = socratic;
        this.ExplicaMas = explicaMas;
        this.Provider = provider;
    }
}
