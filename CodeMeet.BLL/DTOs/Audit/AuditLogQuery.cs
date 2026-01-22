namespace CodeMeet.BLL.DTOs.Audit;

public class AuditLogQuery
{
    public Guid? OrgId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? SessionId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public string? ActionContains { get; set; }
    public int MaxRows { get; set; } = 200;
}
