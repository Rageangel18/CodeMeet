using CodeMeet.BLL.DTOs.Exec;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.Contracts.Exec;
using CodeMeet.Contracts.Exec.Messages;
using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.Services.Implementations;

public sealed class ExecOrchestrator : IExecOrchestrator
{
    private readonly IExecRequestService _execRequests;
    private readonly IExecRunPublisher _publisher;

    public ExecOrchestrator(IExecRequestService execRequests, IExecRunPublisher publisher)
    {
        _execRequests = execRequests;
        _publisher = publisher;
    }

    public async Task<ExecRequestDto> EnqueueAsync(CreateExecRequestRequest request, CancellationToken ct = default)
    {
        var created = await _execRequests.CreateAsync(request, ct);

        try
        {
            await _publisher.PublishRunAsync(new ExecRunRequestedMessage
            {
                ExecRequestId = created.Id,
                Language = created.Language,
                Code = created.Code
            }, ct);

            // Пока в DAL есть Running - помечаем.
            // Если вы урежете ExecStatus до Completed/Failed, этот шаг нужно будет заменить на StartedUtc/IsRunning и т.п.
            await _execRequests.MarkRunningAsync(created.Id, ct);

            return created;
        }
        catch (Exception ex)
        {
            // Best-effort - фиксируем в БД, что постановка в очередь не удалась
            await _execRequests.CompleteAsync(new CompleteExecRequestRequest
            {
                Id = created.Id,
                Status = ExecStatus.Failed,
                ExitCode = null,
                Stdout = null,
                Stderr = $"Failed to enqueue execution request: {ex.Message}",
                DurationMs = null,
                Truncated = false
            }, ct);

            throw;
        }
    }
}
