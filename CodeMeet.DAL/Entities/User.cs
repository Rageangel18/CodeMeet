public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string? DisplayName { get; set; }
    public DateTime CreatedUtc { get; set; }
    public bool IsActive { get; set; }
    public string? PasswordHash { get; set; }
    public string? PasswordSalt { get; set; }
    public DateTime? PasswordCreatedUtc { get; set; }

    public ICollection<OrganizationUser> OrganizationMemberships { get; set; } = new List<OrganizationUser>();
    public ICollection<Session> CreatedSessions { get; set; } = new List<Session>();
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public ICollection<Invitation> AcceptedInvitations { get; set; } = new List<Invitation>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<Snippet> Snippets { get; set; } = new List<Snippet>();
    public ICollection<SnippetComment> SnippetComments { get; set; } = new List<SnippetComment>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<ExecRequest> ExecRequests { get; set; } = new List<ExecRequest>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<Export> Exports { get; set; } = new List<Export>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}