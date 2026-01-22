using CodeMeet.BLL.DTOs.Organizations;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Implementations;

public class OrganizationService : IOrganizationService
{
    private readonly CodeMeetDbContext _dbContext;

    public OrganizationService(CodeMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrganizationDto> CreateAsync(CreateOrganizationRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Organization name is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Slug))
            throw new ArgumentException("Organization slug is required", nameof(request));

        var exists = await _dbContext.Organizations
            .AnyAsync(o => o.Slug == request.Slug, ct);

        if (exists)
            throw new InvalidOperationException($"Organization with slug '{request.Slug}' already exists");

        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = request.Slug.Trim(),
            DataRetentionDays = request.DataRetentionDays ?? 365,
            CreatedUtc = DateTime.UtcNow
        };

        _dbContext.Organizations.Add(org);
        await _dbContext.SaveChangesAsync(ct);

        return MapToDto(org);
    }

    public async Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var org = await _dbContext.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        return org is null ? null : MapToDto(org);
    }

    public async Task<IReadOnlyList<OrganizationDto>> GetListAsync(CancellationToken ct = default)
    {
        var orgs = await _dbContext.Organizations
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .ToListAsync(ct);

        return orgs.Select(MapToDto).ToList();
    }

    private static OrganizationDto MapToDto(Organization org) =>
        new()
        {
            Id = org.Id,
            Name = org.Name,
            Slug = org.Slug,
            DataRetentionDays = org.DataRetentionDays,
            CreatedUtc = org.CreatedUtc
        };
}
