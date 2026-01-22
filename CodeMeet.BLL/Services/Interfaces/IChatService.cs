using CodeMeet.BLL.DTOs.Chat;

namespace CodeMeet.BLL.Services.Interfaces;

public interface IChatService
{
    Task<ChatMessageDto> SendAsync(CreateChatMessageRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ChatMessageDto>> GetBySessionAsync(Guid sessionId, DateTime? fromUtc = null, CancellationToken ct = default);
}
