using CodeMeet.BLL.DTOs.Exec;
using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.Services.Interfaces;

public interface IExecRequestService
{
    Task<ExecRequestDto> CreateAsync(CreateExecRequestRequest request, CancellationToken ct = default);
    Task MarkRunningAsync(Guid id, CancellationToken ct = default);
    Task CompleteAsync(CompleteExecRequestRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<ExecRequestDto>> GetBySessionAsync(Guid sessionId, ExecStatus? status = null, CancellationToken ct = default);
    Task<ExecRequestDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
