using CodeMeet.BLL.DTOs.Invitations;

namespace CodeMeet.BLL.Services.Interfaces;

public interface IInvitationService
{
    Task<InvitationDto> CreateAsync(CreateInvitationRequest request, CancellationToken ct = default);
    Task<bool> AcceptAsync(AcceptInvitationRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<InvitationDto>> GetBySessionAsync(Guid sessionId, CancellationToken ct = default);
}
