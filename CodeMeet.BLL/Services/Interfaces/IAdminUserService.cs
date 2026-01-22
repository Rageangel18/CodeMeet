using CodeMeet.BLL.Dtos.Admin;

namespace CodeMeet.BLL.Services.Admin;

public interface IAdminUserService
{
    Task<IReadOnlyList<AdminUserDto>> GetAllAsync(CancellationToken ct = default);
    Task<AdminUserDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AdminUserDto> UpdateAsync(Guid id, AdminUserUpdateRequest request, CancellationToken ct = default);
}
