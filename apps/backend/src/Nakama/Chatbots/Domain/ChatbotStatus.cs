namespace Nakama.Chatbots.Domain;

/// <summary>Mirrors the frontend's <c>ChatbotLifecycleStatus</c> ("borrador" | "activo" | "inactivo").</summary>
public enum ChatbotStatus
{
    Draft,
    Active,
    Inactive,
}
