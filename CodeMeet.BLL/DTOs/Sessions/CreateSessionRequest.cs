using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Sessions;

public class CreateSessionRequest
{
    public Guid OrgId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime? ScheduledStartUtc { get; set; }

    public DateTime? ScheduledEndUtc { get; set; }

    public Guid CreatedByUserId { get; set; }

    public bool? IsExecEnabled { get; set; }

    public CodeLanguage? DefaultLanguage { get; set; }
}