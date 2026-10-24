namespace Nakama.Formations.Application;

public sealed record GeneratedQuestionResponse(string Question, IReadOnlyList<string> Options, int CorrectIndex);

public sealed record GeneratedQuizResponse(IReadOnlyList<GeneratedQuestionResponse> Questions);
