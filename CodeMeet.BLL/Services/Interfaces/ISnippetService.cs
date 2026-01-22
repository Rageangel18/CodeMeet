using CodeMeet.BLL.DTOs.Snippets;

namespace CodeMeet.BLL.Services.Interfaces;

public interface ISnippetService
{
    Task<SnippetDto> CreateAsync(CreateSnippetRequest request, CancellationToken ct = default);
    Task SetFinalAsync(Guid snippetId, CancellationToken ct = default);
    Task<IReadOnlyList<SnippetDto>> GetBySessionAsync(Guid sessionId, bool onlyFinal = false, CancellationToken ct = default);
}
