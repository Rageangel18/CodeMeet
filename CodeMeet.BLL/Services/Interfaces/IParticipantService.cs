using CodeMeet.BLL.DTOs.Participants;
using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.Services.Interfaces;

public interface IParticipantService
{
    Task<ParticipantDto> AddOrUpdateAsync(AddOrUpdateParticipantRequest request, CancellationToken ct = default);
    Task UpdateStatusAsync(UpdateParticipantStatusRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ParticipantDto>> GetBySessionAsync(Guid sessionId, ParticipantRole? role = null, CancellationToken ct = default);
}
