using System.Text.Json;
using CodeMeet.BLL.DTOs.Sessions;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Implementations;

public class SessionReportService : ISessionReportService
{
    private readonly CodeMeetDbContext _dbContext;

    public SessionReportService(CodeMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SessionSummaryDto?> GetSummaryAsync(Guid sessionId, CancellationToken ct = default)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        var rows = await _dbContext.SessionSummaries
            .FromSqlInterpolated($"select * from public.sp_get_session_summary({sessionId})")
            .AsNoTracking()
            .ToListAsync(ct);


        var row = rows.FirstOrDefault();
        if (row is null)
            return null;

        var status = SessionStatus.Draft;
        Enum.TryParse(row.Status, ignoreCase: true, out status);

        var askedQuestions = new List<SessionSummaryQuestionDto>();

        if (!string.IsNullOrWhiteSpace(row.QuestionsJson))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<List<SessionSummaryQuestionDto>>(row.QuestionsJson);
                if (parsed is not null)
                    askedQuestions = parsed;
            }
            catch
            {
            }
        }

        return new SessionSummaryDto
        {
            SessionId = row.SessionId,
            OrgId = row.OrgId,
            OrgName = row.OrgName,
            Title = row.Title,
            Status = status,
            ScheduledStartUtc = row.ScheduledStartUtc,
            ScheduledEndUtc = row.ScheduledEndUtc,
            CreatedUtc = row.CreatedUtc,
            CandidateCount = row.CandidateCount,
            InterviewerCount = row.InterviewerCount,
            ObserverCount = row.ObserverCount,
            SnippetCount = row.SnippetCount,
            ExecRequestCount = row.ExecRequestCount,
            AvgScoreOverall = row.AvgScoreOverall,
            AvgScoreTech = row.AvgScoreTech,
            AvgScoreComm = row.AvgScoreComm,
            Questions = askedQuestions,
            StrengthsSummary = row.StrengthsSummary,
            ConcernsSummary = row.ConcernsSummary
        };
    }
}
