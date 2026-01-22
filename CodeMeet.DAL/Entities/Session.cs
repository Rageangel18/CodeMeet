using CodeMeet.DAL.Enums;

public class Session
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public string Title { get; set; } = null!;
    public SessionStatus Status { get; set; }
    public DateTime? ScheduledStartUtc { get; set; }
    public DateTime? ScheduledEndUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public bool IsExecEnabled { get; set; }
    public CodeLanguage DefaultLanguage { get; set; }
    public string? RecordingBlobUrl { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }

    public Organization Org { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;

    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
    public ICollection<SessionQuestion> SessionQuestions { get; set; } = new List<SessionQuestion>();
    public ICollection<Snippet> Snippets { get; set; } = new List<Snippet>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<ExecRequest> ExecRequests { get; set; } = new List<ExecRequest>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<Export> Exports { get; set; } = new List<Export>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}