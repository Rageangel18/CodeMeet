using CodeMeet.BLL.DTOs.Organizations;

namespace CodeMeet.BLL.Services.Interfaces;

public interface IOrganizationService
{
    Task<OrganizationDto> CreateAsync(CreateOrganizationRequest request, CancellationToken ct = default);
    Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<OrganizationDto>> GetListAsync(CancellationToken ct = default);
}
