using CodeMeet.BLL.DTOs.Exports;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Implementations;

public class ExportService : IExportService
{
    private readonly CodeMeetDbContext _dbContext;

    public ExportService(CodeMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExportDto> CreateAsync(CreateExportRequest request, CancellationToken ct = default)
    {
        if (request.OrgId == Guid.Empty)
            throw new ArgumentException("OrgId is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Type))
            throw new ArgumentException("Type is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.BlobUrl))
            throw new ArgumentException("BlobUrl is required", nameof(request));

        if (request.CreatedByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId is required", nameof(request));

        var orgExists = await _dbContext.Organizations
            .AnyAsync(o => o.Id == request.OrgId, ct);

        if (!orgExists)
            throw new InvalidOperationException("Organization not found");

        if (request.SessionId.HasValue)
        {
            var sessionExists = await _dbContext.Sessions
                .AnyAsync(s => s.Id == request.SessionId.Value && s.OrgId == request.OrgId, ct);

            if (!sessionExists)
                throw new InvalidOperationException("Session not found in this organization");
        }

        var userExists = await _dbContext.Users
            .AnyAsync(u => u.Id == request.CreatedByUserId, ct);

        if (!userExists)
            throw new InvalidOperationException("User not found");

        var now = DateTime.UtcNow;

        var entity = new Export
        {
            Id = Guid.NewGuid(),
            OrgId = request.OrgId,
            SessionId = request.SessionId,
            Type = request.Type.Trim(),
            BlobUrl = request.BlobUrl.Trim(),
            CreatedByUserId = request.CreatedByUserId,
            CreatedUtc = now
        };

        _dbContext.Exports.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        return Map(entity);
    }

    public async Task<IReadOnlyList<ExportDto>> GetByOrgAsync(Guid orgId, CancellationToken ct = default)
    {
        if (orgId == Guid.Empty)
            throw new ArgumentException("OrgId is required", nameof(orgId));

        var list = await _dbContext.Exports
            .AsNoTracking()
            .Where(e => e.OrgId == orgId)
            .OrderByDescending(e => e.CreatedUtc)
            .ToListAsync(ct);

        return list.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ExportDto>> GetBySessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        var list = await _dbContext.Exports
            .AsNoTracking()
            .Where(e => e.SessionId == sessionId)
            .OrderByDescending(e => e.CreatedUtc)
            .ToListAsync(ct);

        return list.Select(Map).ToList();
    }

    private static ExportDto Map(Export e) => new()
    {
        Id = e.Id,
        OrgId = e.OrgId,
        SessionId = e.SessionId,
        Type = e.Type,
        BlobUrl = e.BlobUrl,
        CreatedByUserId = e.CreatedByUserId,
        CreatedUtc = e.CreatedUtc
    };
}
