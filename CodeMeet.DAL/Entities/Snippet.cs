using CodeMeet.DAL.Enums;

public class Snippet
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? AuthorUserId { get; set; }
    public CodeLanguage Language { get; set; }
    public string? Filename { get; set; }
    public string Content { get; set; } = null!;
    public bool IsFinalSolution { get; set; }
    public DateTime CreatedUtc { get; set; }

    public Session Session { get; set; } = null!;
    public User? AuthorUser { get; set; }
    public ICollection<SnippetComment> Comments { get; set; } = new List<SnippetComment>();
    public ICollection<ExecRequest> ExecRequests { get; set; } = new List<ExecRequest>();
}