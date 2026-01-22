using CodeMeet.BLL.DTOs.Sessions;

namespace CodeMeet.BLL.Services.Interfaces;

public interface ISessionReportService
{
    Task<SessionSummaryDto?> GetSummaryAsync(Guid sessionId, CancellationToken ct = default);
}
