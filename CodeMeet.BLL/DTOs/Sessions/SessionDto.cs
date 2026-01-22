using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Sessions;

public class SessionDto
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
    public DateTime CreatedUtc { get; set; }
}
