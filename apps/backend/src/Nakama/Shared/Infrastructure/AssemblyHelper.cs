using System.Reflection;
using Nakama.Chatbots.Domain;

namespace Nakama.Shared.Infrastructure;

public static class AssemblyHelper
{
    public static Assembly Domain { get; } = typeof(Chatbot).Assembly;
}
