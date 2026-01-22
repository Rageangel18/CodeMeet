using CodeMeet.BLL.DTOs.Feedback;

namespace CodeMeet.BLL.Services.Interfaces;

public interface IFeedbackService
{
    Task<FeedbackDto> AddAsync(CreateFeedbackRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<FeedbackDto>> GetBySessionAsync(Guid sessionId, CancellationToken ct = default);
}
