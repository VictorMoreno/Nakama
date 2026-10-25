namespace Nakama.FormationDetail.Domain;

public sealed class QuizQuestionRow
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public int CorrectIndex { get; set; }
}

public sealed class FormationModuleRow
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public bool Gated { get; set; }
    public bool AiDraft { get; set; }
    public string? ResourceUrl { get; set; }
    public int? DurationMin { get; set; }
    public List<QuizQuestionRow>? Quiz { get; set; }
    public int? PassingScore { get; set; }
}

public sealed class FormationDocumentRow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public DateTime Updated { get; set; }
    public string FileName { get; set; } = string.Empty;
}

public sealed class FormationMemberRow
{
    public Guid UserId { get; set; }
    public int Progress { get; set; }
}
