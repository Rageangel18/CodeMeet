using CodeMeet.BLL.DTOs.Questions;
using CodeMeet.DAL.Enums;

namespace CodeMeet.BLL.Services.Interfaces;

public interface IQuestionService
{
    Task<QuestionDto> CreateAsync(CreateQuestionRequest request, CancellationToken ct = default);
    Task<QuestionDto?> UpdateAsync(UpdateQuestionRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<QuestionDto>> GetByOrgAsync(Guid orgId, QuestionLevel? level = null, CancellationToken ct = default);

    Task AssignToSessionAsync(Guid sessionId, IReadOnlyCollection<Guid> questionIds, CancellationToken ct = default);
    Task<IReadOnlyList<QuestionDto>> GetForSessionAsync(Guid sessionId, CancellationToken ct = default);
    Task<QuestionDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<QuestionDto>> SearchInOrgAsync(
        Guid orgId,
        string? search = null,
        IReadOnlyCollection<Guid>? tagIds = null,
        QuestionLevel? level = null,
        CancellationToken ct = default);
}
