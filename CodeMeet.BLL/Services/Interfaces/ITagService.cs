using CodeMeet.BLL.DTOs.Tags;

namespace CodeMeet.BLL.Services.Interfaces;

public interface ITagService
{
    Task<TagDto> CreateAsync(CreateTagRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<TagDto>> GetByOrgAsync(Guid orgId, CancellationToken ct = default);

    Task SetTagsForQuestionAsync(Guid questionId, IReadOnlyCollection<Guid> tagIds, CancellationToken ct = default);
    Task<IReadOnlyList<TagDto>> GetForQuestionAsync(Guid questionId, CancellationToken ct = default);
}
