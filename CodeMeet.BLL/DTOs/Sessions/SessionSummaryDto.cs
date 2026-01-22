using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.DTOs.Sessions;

public class SessionSummaryDto
{
    public Guid SessionId { get; set; }
    public Guid OrgId { get; set; }
    public string? OrgName { get; set; }
    public string Title { get; set; } = null!;
    public SessionStatus Status { get; set; }
    public DateTime? ScheduledStartUtc { get; set; }
    public DateTime? ScheduledEndUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
    public int CandidateCount { get; set; }
    public int InterviewerCount { get; set; }
    public int ObserverCount { get; set; }
    public int SnippetCount { get; set; }
    public int ExecRequestCount { get; set; }
    public double? AvgScoreOverall { get; set; }
    public double? AvgScoreTech { get; set; }
    public double? AvgScoreComm { get; set; }
    public string? StrengthsSummary { get; set; }
    public string? ConcernsSummary { get; set; }

    public IReadOnlyList<SessionSummaryQuestionDto> Questions { get; set; }
        = Array.Empty<SessionSummaryQuestionDto>();  
}
