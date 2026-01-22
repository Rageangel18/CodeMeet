using CodeMeet.BLL.DTOs.Audit;

namespace CodeMeet.BLL.Services.Interfaces;

public interface IAuditLogService
{
    Task<IReadOnlyList<AuditLogEntryDto>> QueryAsync(AuditLogQuery query, CancellationToken ct = default);
}
