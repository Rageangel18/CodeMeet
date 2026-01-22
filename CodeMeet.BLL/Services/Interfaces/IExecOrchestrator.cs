using CodeMeet.BLL.DTOs.Exec;

namespace CodeMeet.BLL.Services.Interfaces;

public interface IExecOrchestrator
{
    Task<ExecRequestDto> EnqueueAsync(CreateExecRequestRequest request, CancellationToken ct = default);
}
