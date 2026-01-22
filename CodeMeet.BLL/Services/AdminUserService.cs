using CodeMeet.BLL.Dtos.Admin;
using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Admin;

public sealed class AdminUserService : IAdminUserService
{
    private readonly CodeMeetDbContext _db;

    public AdminUserService(CodeMeetDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .ToListAsync(ct);

        return entities.Select(Map).ToList();
    }

    public async Task<AdminUserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        return entity is null ? null : Map(entity);
    }

    public async Task<AdminUserDto> UpdateAsync(Guid id, AdminUserUpdateRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (entity is null)
            throw new KeyNotFoundException($"User {id} not found");

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
            entity.DisplayName = request.DisplayName.Trim();

        entity.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);

        return Map(entity);
    }

    private static AdminUserDto Map(User e) => new()
    {
        Id = e.Id,
        Email = e.Email,
        DisplayName = e.DisplayName,
        IsActive = e.IsActive,
        CreatedUtc = e.CreatedUtc
    };
}
