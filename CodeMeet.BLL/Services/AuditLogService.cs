using CodeMeet.BLL.DTOs.Audit;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Implementations;

public class AuditLogService : IAuditLogService
{
    private readonly CodeMeetDbContext _dbContext;

    public AuditLogService(CodeMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AuditLogEntryDto>> QueryAsync(AuditLogQuery query, CancellationToken ct = default)
    {
        var q = _dbContext.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (query.OrgId.HasValue)
            q = q.Where(a => a.OrgId == query.OrgId.Value);

        if (query.UserId.HasValue)
            q = q.Where(a => a.UserId == query.UserId.Value);

        if (query.SessionId.HasValue)
            q = q.Where(a => a.SessionId == query.SessionId.Value);

        if (query.FromUtc.HasValue)
            q = q.Where(a => a.CreatedUtc >= query.FromUtc.Value);

        if (query.ToUtc.HasValue)
            q = q.Where(a => a.CreatedUtc <= query.ToUtc.Value);

        if (!string.IsNullOrWhiteSpace(query.ActionContains))
            q = q.Where(a => a.Action.Contains(query.ActionContains));

        var max = query.MaxRows <= 0 ? 200 : Math.Min(query.MaxRows, 1000);

        var list = await q
            .OrderByDescending(a => a.CreatedUtc)
            .Take(max)
            .ToListAsync(ct);

        return list.Select(a => Map(a)).ToList();
    }

    private static AuditLogEntryDto Map(AuditLog a) => new() 
    {
        Id = a.Id,
        OrgId = a.OrgId,
        UserId = a.UserId,
        SessionId = a.SessionId,
        Action = a.Action,
        SubjectType = a.SubjectType,
        SubjectId = a.SubjectId,
        Data = a.Data,
        CreatedUtc = a.CreatedUtc,
        Ip = a.Ip,
        UserAgent = a.UserAgent
    };
}
