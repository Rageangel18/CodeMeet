namespace CodeMeet.DAL.QueryModels;

public class SessionSummary
{
    public Guid SessionId { get; set; }
    public Guid OrgId { get; set; }
    public string OrgName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Status { get; set; } = null!;
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
    public string? QuestionsJson { get; set; }
    public string? StrengthsSummary { get; set; }
    public string? ConcernsSummary { get; set; }
}
