using CodeMeet.BLL.DTOs.Sessions;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Implementations;

public class SessionService : ISessionService
{
    private readonly CodeMeetDbContext _dbContext;

    public SessionService(CodeMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SessionDto> CreateAsync(CreateSessionRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Session title is required", nameof(request));

        var orgExists = await _dbContext.Organizations
            .AnyAsync(o => o.Id == request.OrgId, ct);

        if (!orgExists)
            throw new InvalidOperationException("Organization not found");

        Guid createdByUserId = request.CreatedByUserId;

        if (createdByUserId == Guid.Empty)
        {
            createdByUserId = await _dbContext.Users
                .OrderBy(u => u.CreatedUtc)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(ct);

            if (createdByUserId == Guid.Empty)
                throw new InvalidOperationException("No users in the system to assign as session creator.");
        }
        else
        {
            var userExists = await _dbContext.Users
                .AnyAsync(u => u.Id == createdByUserId, ct);

            if (!userExists)
                throw new InvalidOperationException("User not found");
        }

        var start = request.ScheduledStartUtc;

        if (start == default)
            throw new ArgumentException("Scheduled start time is required", nameof(request));

        // дефолт: 1 час, если конец не указан
        var end = request.ScheduledEndUtc ?? start + TimeSpan.FromHours(1);

        if (end <= start)
            throw new ArgumentException("Scheduled end time must be later than start time", nameof(request));

        var session = new Session
        {
            Id = Guid.NewGuid(),
            OrgId = request.OrgId,
            Title = request.Title.Trim(),
            Status = SessionStatus.Draft,
            ScheduledStartUtc = start,
            ScheduledEndUtc = end,
            CreatedByUserId = createdByUserId,
            IsExecEnabled = request.IsExecEnabled ?? true,
            DefaultLanguage = request.DefaultLanguage ?? CodeLanguage.CSharp,
            CreatedUtc = DateTime.UtcNow
        };

        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync(ct);

        return MapToDto(session);
    }

    public async Task<SessionDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (session is null)
            return null;

        var nowUtc = DateTime.UtcNow;

        if (ShouldBeCompletedNow(session, nowUtc))
        {
            session.Status = SessionStatus.Completed;
            session.UpdatedUtc = nowUtc;
            await _dbContext.SaveChangesAsync(ct);
        }

        return MapToDto(session);
    }

    public async Task<IReadOnlyList<SessionSummaryDto>> GetByOrgAsync(
        Guid orgId,
        SessionStatus? status,
        CancellationToken ct = default)
    {
        if (orgId == Guid.Empty)
            throw new ArgumentException("OrgId is required", nameof(orgId));

        var query = _dbContext.Sessions
            .AsNoTracking()
            .Where(s => s.OrgId == orgId);

        var sessions = await query
            .OrderByDescending(s => s.ScheduledStartUtc ?? s.CreatedUtc)
            .ToListAsync(ct);

        var nowUtc = DateTime.UtcNow;

        await PersistCompletedAsync(sessions, nowUtc, ct);

        if (status.HasValue)
        {
            sessions = sessions
                .Where(s => GetEffectiveStatus(s, nowUtc) == status.Value)
                .ToList();
        }

        var list = sessions
            .Select(s => new SessionSummaryDto
            {
                SessionId = s.Id,
                OrgId = s.OrgId,
                Title = s.Title,
                Status = GetEffectiveStatus(s, nowUtc),
                ScheduledStartUtc = s.ScheduledStartUtc,
                ScheduledEndUtc = s.ScheduledEndUtc,

                CandidateCount = 0,
                InterviewerCount = 0,
                ObserverCount = 0,
                SnippetCount = 0,
                ExecRequestCount = 0,
                AvgScoreOverall = null,
                AvgScoreTech = null,
                AvgScoreComm = null
            })
            .ToList();

        return list;
    }

    public async Task<IReadOnlyList<SessionSummaryDto>> GetByUserParticipationAsync(
        Guid userId,
        SessionStatus? status,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required", nameof(userId));

        var query = _dbContext.Participants
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.Session)
            .Distinct();

        var sessions = await query
            .OrderByDescending(s => s.ScheduledStartUtc ?? s.CreatedUtc)
            .ToListAsync(ct);

        var nowUtc = DateTime.UtcNow;

        await PersistCompletedAsync(sessions, nowUtc, ct);

        if (status.HasValue)
        {
            sessions = sessions
                .Where(s => GetEffectiveStatus(s, nowUtc) == status.Value)
                .ToList();
        }

        var list = sessions
            .Select(s => new SessionSummaryDto
            {
                SessionId = s.Id,
                OrgId = s.OrgId,
                Title = s.Title,
                Status = GetEffectiveStatus(s, nowUtc),
                ScheduledStartUtc = s.ScheduledStartUtc,
                ScheduledEndUtc = s.ScheduledEndUtc,

                CandidateCount = 0,
                InterviewerCount = 0,
                ObserverCount = 0,
                SnippetCount = 0,
                ExecRequestCount = 0,
                AvgScoreOverall = null,
                AvgScoreTech = null,
                AvgScoreComm = null
            })
            .ToList();

        return list;
    }

    public async Task UpdateScheduleAsync(Guid sessionId, DateTime? startUtc, DateTime? endUtc, CancellationToken ct = default)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        var s = await _dbContext.Sessions
            .FirstOrDefaultAsync(x => x.Id == sessionId, ct);

        if (s is null)
            throw new InvalidOperationException("Session not found");

        // если старт задан, а конец не задан - ставим дефолт 1 час
        if (startUtc.HasValue && !endUtc.HasValue)
            endUtc = startUtc.Value.AddHours(1);

        // если оба заданы - проверяем
        if (startUtc.HasValue && endUtc.HasValue && endUtc.Value <= startUtc.Value)
            throw new ArgumentException("Scheduled end time must be later than start time");

        s.ScheduledStartUtc = startUtc;
        s.ScheduledEndUtc = endUtc;
        s.UpdatedUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task ChangeStatusAsync(Guid sessionId, SessionStatus newStatus, CancellationToken ct = default)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        var s = await _dbContext.Sessions
            .FirstOrDefaultAsync(x => x.Id == sessionId, ct);

        if (s is null)
            throw new InvalidOperationException("Session not found");

        var allowedNext = s.Status switch
        {
            SessionStatus.Draft => new[] { SessionStatus.Scheduled, SessionStatus.Cancelled },
            SessionStatus.Scheduled => new[] { SessionStatus.Live, SessionStatus.Cancelled },
            SessionStatus.Live => new[] { SessionStatus.Completed, SessionStatus.Cancelled },
            SessionStatus.Completed => Array.Empty<SessionStatus>(),
            SessionStatus.Cancelled => Array.Empty<SessionStatus>(),
            _ => Array.Empty<SessionStatus>()
        };

        if (!allowedNext.Contains(newStatus))
            throw new InvalidOperationException($"Cannot change status from {s.Status} to {newStatus}");

        s.Status = newStatus;
        s.UpdatedUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);
    }

    // (опционально полезно) отдельный метод, если ты хочешь стартовать строго через API
    public async Task StartAsync(Guid sessionId, CancellationToken ct = default)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        var s = await _dbContext.Sessions
            .FirstOrDefaultAsync(x => x.Id == sessionId, ct);

        if (s is null)
            throw new InvalidOperationException("Session not found");

        if (s.Status != SessionStatus.Scheduled)
            throw new InvalidOperationException("Only Scheduled sessions can be started.");

        var nowUtc = DateTime.UtcNow;

        // можно ужесточить: нельзя стартовать до scheduled start
        if (s.ScheduledStartUtc.HasValue && s.ScheduledStartUtc.Value > nowUtc)
            throw new InvalidOperationException("Session cannot be started before scheduled start time.");

        s.Status = SessionStatus.Live;
        s.UpdatedUtc = nowUtc;

        // если end почему-то пустой - ставим дефолт 1 час от now или от start
        if (!s.ScheduledEndUtc.HasValue)
        {
            var baseTime = s.ScheduledStartUtc ?? nowUtc;
            s.ScheduledEndUtc = baseTime.AddHours(1);
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    private static SessionStatus GetEffectiveStatus(Session s, DateTime utcNow)
    {
        // ВАЖНО: авто-Live разрешаем только если статус Scheduled или уже Live.
        // Draft не должен становиться Live только из-за дат.
        var canAutoRunByTime =
            s.Status == SessionStatus.Scheduled ||
            s.Status == SessionStatus.Live;

        if (canAutoRunByTime &&
            s.ScheduledStartUtc.HasValue &&
            s.ScheduledStartUtc.Value <= utcNow &&
            (!s.ScheduledEndUtc.HasValue || s.ScheduledEndUtc.Value > utcNow))
        {
            return SessionStatus.Live;
        }

        // Completed - тоже не делаем для Draft
        if (canAutoRunByTime &&
            s.ScheduledEndUtc.HasValue &&
            s.ScheduledEndUtc.Value <= utcNow &&
            s.Status != SessionStatus.Cancelled)
        {
            return SessionStatus.Completed;
        }

        return s.Status;
    }

    private static bool ShouldBeCompletedNow(Session s, DateTime utcNow)
    {
        // Завершаем только если сессия была Scheduled/Live
        var canAutoComplete =
            s.Status == SessionStatus.Scheduled ||
            s.Status == SessionStatus.Live;

        return canAutoComplete &&
               s.ScheduledEndUtc.HasValue &&
               s.ScheduledEndUtc.Value <= utcNow &&
               s.Status != SessionStatus.Completed &&
               s.Status != SessionStatus.Cancelled;
    }

    private async Task PersistCompletedAsync(IEnumerable<Session> snapshot, DateTime nowUtc, CancellationToken ct)
    {
        var idsToComplete = snapshot
            .Where(s => ShouldBeCompletedNow(s, nowUtc))
            .Select(s => s.Id)
            .Distinct()
            .ToList();

        if (idsToComplete.Count == 0)
            return;

        var tracked = await _dbContext.Sessions
            .Where(s => idsToComplete.Contains(s.Id))
            .ToListAsync(ct);

        foreach (var s in tracked)
        {
            s.Status = SessionStatus.Completed;
            s.UpdatedUtc = nowUtc;
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    private static SessionDto MapToDto(Session s)
    {
        var nowUtc = DateTime.UtcNow;

        return new SessionDto
        {
            Id = s.Id,
            OrgId = s.OrgId,
            Title = s.Title,
            Status = GetEffectiveStatus(s, nowUtc),
            ScheduledStartUtc = s.ScheduledStartUtc,
            ScheduledEndUtc = s.ScheduledEndUtc,
            CreatedByUserId = s.CreatedByUserId,
            IsExecEnabled = s.IsExecEnabled,
            DefaultLanguage = s.DefaultLanguage,
            CreatedUtc = s.CreatedUtc
        };
    }
}
