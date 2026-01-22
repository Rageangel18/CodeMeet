using CodeMeet.BLL.DTOs.Exec;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Implementations;

public sealed class ExecRequestService : IExecRequestService
{
    private readonly CodeMeetDbContext _db;

    public ExecRequestService(CodeMeetDbContext db)
    {
        _db = db;
    }

    public async Task<ExecRequestDto> CreateAsync(CreateExecRequestRequest request, CancellationToken ct = default)
    {
        if (request.SessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Code is required", nameof(request));

        var sessionExists = await _db.Sessions.AnyAsync(s => s.Id == request.SessionId, ct);
        if (!sessionExists)
            throw new InvalidOperationException("Session not found");

        if (request.SnippetId.HasValue)
        {
            var snippetExists = await _db.Snippets.AnyAsync(s =>
                s.Id == request.SnippetId.Value && s.SessionId == request.SessionId, ct);

            if (!snippetExists)
                throw new InvalidOperationException("Snippet not found in this session");
        }

        if (request.RequestedByUserId.HasValue)
        {
            var userExists = await _db.Users.AnyAsync(u => u.Id == request.RequestedByUserId.Value, ct);
            if (!userExists)
                throw new InvalidOperationException("Requesting user not found");
        }

        var now = DateTime.UtcNow;

        var entity = new ExecRequest
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            SnippetId = request.SnippetId,
            RequestedByUserId = request.RequestedByUserId,
            Language = request.Language,
            Code = request.Code,

            // Пока DAL содержит Queued/Running - сохраняем.
            // Если вы решите оставить в ExecStatus только Completed/Failed - это место изменится.
            Status = ExecStatus.Queued,

            CreatedUtc = now,
            Truncated = false
        };

        _db.ExecRequests.Add(entity);
        await _db.SaveChangesAsync(ct);

        return Map(entity);
    }

    public async Task MarkRunningAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id is required", nameof(id));

        var entity = await _db.ExecRequests.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
            throw new InvalidOperationException("Exec request not found");

        if (entity.Status is ExecStatus.Completed or ExecStatus.Failed)
            return;

        entity.Status = ExecStatus.Running;
        await _db.SaveChangesAsync(ct);
    }

    public async Task CompleteAsync(CompleteExecRequestRequest request, CancellationToken ct = default)
    {
        if (request.Id == Guid.Empty)
            throw new ArgumentException("Id is required", nameof(request));

        var entity = await _db.ExecRequests.FirstOrDefaultAsync(e => e.Id == request.Id, ct);
        if (entity is null)
            throw new InvalidOperationException("Exec request not found");

        entity.Status = request.Status;
        entity.ExitCode = request.ExitCode;
        entity.Stdout = request.Stdout;
        entity.Stderr = request.Stderr;
        entity.DurationMs = request.DurationMs;
        entity.Truncated = request.Truncated;
        entity.CompletedUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ExecRequestDto>> GetBySessionAsync(Guid sessionId, ExecStatus? status = null, CancellationToken ct = default)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        var q = _db.ExecRequests.AsNoTracking().Where(e => e.SessionId == sessionId);

        if (status.HasValue)
            q = q.Where(e => e.Status == status.Value);

        var list = await q.OrderByDescending(e => e.CreatedUtc).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<ExecRequestDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id is required", nameof(id));

        var entity = await _db.ExecRequests.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
        return entity is null ? null : Map(entity);
    }

    private static ExecRequestDto Map(ExecRequest e) => new()
    {
        Id = e.Id,
        SessionId = e.SessionId,
        SnippetId = e.SnippetId,
        RequestedByUserId = e.RequestedByUserId,
        Language = e.Language,
        Code = e.Code,
        Status = e.Status,
        ExitCode = e.ExitCode,
        Stdout = e.Stdout,
        Stderr = e.Stderr,
        DurationMs = e.DurationMs,
        Truncated = e.Truncated,
        CreatedUtc = e.CreatedUtc,
        CompletedUtc = e.CompletedUtc
    };
}
