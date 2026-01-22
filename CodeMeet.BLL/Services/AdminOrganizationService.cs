using CodeMeet.BLL.Dtos.Admin;
using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Admin;

public sealed class AdminOrganizationService : IAdminOrganizationService
{
    private readonly CodeMeetDbContext _db;

    public AdminOrganizationService(CodeMeetDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AdminOrganizationDto>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _db.Organizations
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .ToListAsync(ct);

        return entities.Select(Map).ToList();
    }

    public async Task<AdminOrganizationDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        return entity is null ? null : Map(entity);
    }

    public async Task<AdminOrganizationDto> CreateAsync(AdminOrganizationUpsertRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Slug))
            throw new ArgumentException("Slug is required", nameof(request));

        var now = DateTime.UtcNow;

        var entity = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = request.Slug.Trim(),
            DataRetentionDays = request.DataRetentionDays <= 0 ? 365 : request.DataRetentionDays,
            CreatedUtc = now,
            UpdatedUtc = now
        };

        _db.Organizations.Add(entity);
        await _db.SaveChangesAsync(ct);

        return Map(entity);
    }

    public async Task<AdminOrganizationDto> UpdateAsync(Guid id, AdminOrganizationUpsertRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Organization {id} not found");

        if (!string.IsNullOrWhiteSpace(request.Name))
            entity.Name = request.Name.Trim();

        if (!string.IsNullOrWhiteSpace(request.Slug))
            entity.Slug = request.Slug.Trim();

        if (request.DataRetentionDays > 0)
            entity.DataRetentionDays = request.DataRetentionDays;

        entity.UpdatedUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return Map(entity);
    }

    private static AdminOrganizationDto Map(Organization e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Slug = e.Slug,
        DataRetentionDays = e.DataRetentionDays,
        CreatedUtc = e.CreatedUtc,
        UpdatedUtc = e.UpdatedUtc
    };
}
