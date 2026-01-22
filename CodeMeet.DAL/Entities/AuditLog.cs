public class AuditLog
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? SessionId { get; set; }
    public string Action { get; set; } = null!;
    public string? SubjectType { get; set; }
    public Guid? SubjectId { get; set; }
    public string? Data { get; set; }
    public DateTime CreatedUtc { get; set; }
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }

    public Organization Org { get; set; } = null!;
    public User? User { get; set; }
    public Session? Session { get; set; }
}