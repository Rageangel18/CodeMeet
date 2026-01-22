using CodeMeet.BLL.DTOs.Exports;

namespace CodeMeet.BLL.Services.Interfaces;

public interface IExportService
{
    Task<ExportDto> CreateAsync(CreateExportRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ExportDto>> GetByOrgAsync(Guid orgId, CancellationToken ct = default);
    Task<IReadOnlyList<ExportDto>> GetBySessionAsync(Guid sessionId, CancellationToken ct = default);
}
        