using CodeMeet.BLL.Dtos.Admin;

namespace CodeMeet.BLL.Services.Admin;

public interface IAdminOrganizationService
{
    Task<IReadOnlyList<AdminOrganizationDto>> GetAllAsync(CancellationToken ct = default);
    Task<AdminOrganizationDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AdminOrganizationDto> CreateAsync(AdminOrganizationUpsertRequest request, CancellationToken ct = default);
    Task<AdminOrganizationDto> UpdateAsync(Guid id, AdminOrganizationUpsertRequest request, CancellationToken ct = default);
}
