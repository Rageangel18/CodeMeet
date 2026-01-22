using CodeMeet.BLL.DTOs.Snippets;

namespace CodeMeet.BLL.Services.Interfaces;

public interface ISnippetCommentService
{
    Task<SnippetCommentDto> AddAsync(CreateSnippetCommentRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<SnippetCommentDto>> GetBySnippetAsync(Guid snippetId, CancellationToken ct = default);
}
