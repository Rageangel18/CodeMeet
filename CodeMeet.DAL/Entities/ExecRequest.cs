using CodeMeet.DAL.Enums;

public class ExecRequest
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? SnippetId { get; set; }
    public Guid? RequestedByUserId { get; set; }
    public CodeLanguage Language { get; set; }
    public string Code { get; set; } = null!;
    public ExecStatus Status { get; set; }
    public int? ExitCode { get; set; }
    public string? Stdout { get; set; }
    public string? Stderr { get; set; }
    public int? DurationMs { get; set; }
    public bool Truncated { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? CompletedUtc { get; set; }

    public Session Session { get; set; } = null!;
    public Snippet? Snippet { get; set; }
    public User? RequestedByUser { get; set; }
}