using CodeMeet.BLL.DTOs.Sessions;
using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.Services.Interfaces;

public interface ISessionService
{
    Task<SessionDto> CreateAsync(CreateSessionRequest request, CancellationToken ct = default);
    Task<SessionDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    // Task<IReadOnlyList<SessionDto>> GetByOrgAsync(Guid orgId, SessionStatus? status = null, CancellationToken ct = default);
    Task<IReadOnlyList<SessionSummaryDto>> GetByOrgAsync(
        Guid orgId,
        SessionStatus? status,
        CancellationToken ct = default);

    Task<IReadOnlyList<SessionSummaryDto>> GetByUserParticipationAsync(
        Guid userId,
        SessionStatus? status,
        CancellationToken ct = default);
    Task UpdateScheduleAsync(Guid sessionId, DateTime? startUtc, DateTime? endUtc, CancellationToken ct = default);
    Task ChangeStatusAsync(Guid sessionId, SessionStatus newStatus, CancellationToken ct = default);

}
