namespace Nakama.Formations.Domain;

public sealed record QuizQuestion(string Question, IReadOnlyList<string> Options, int CorrectIndex);
