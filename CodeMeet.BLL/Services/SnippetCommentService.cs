using CodeMeet.BLL.DTOs.Snippets;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Implementations;

public class SnippetCommentService : ISnippetCommentService
{
    private readonly CodeMeetDbContext _dbContext;

    public SnippetCommentService(CodeMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SnippetCommentDto> AddAsync(CreateSnippetCommentRequest request, CancellationToken ct = default)
    {
        if (request.SnippetId == Guid.Empty)
            throw new ArgumentException("SnippetId is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Text))
            throw new ArgumentException("Text is required", nameof(request));

        var snippetExists = await _dbContext.Snippets
            .AnyAsync(s => s.Id == request.SnippetId, ct);

        if (!snippetExists)
            throw new InvalidOperationException("Snippet not found");

        if (request.AuthorUserId.HasValue)
        {
            var userExists = await _dbContext.Users
                .AnyAsync(u => u.Id == request.AuthorUserId.Value, ct);

            if (!userExists)
                throw new InvalidOperationException("Author user not found");
        }

        var now = DateTime.UtcNow;

        var entity = new SnippetComment
        {
            Id = Guid.NewGuid(),
            SnippetId = request.SnippetId,
            AuthorUserId = request.AuthorUserId,
            LineStart = request.LineStart ?? 0,
            LineEnd = request.LineEnd ?? request.LineStart ?? 0,
            Text = request.Text,
            CreatedUtc = now
        };

        _dbContext.SnippetComments.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        return Map(entity);
    }

    public async Task<IReadOnlyList<SnippetCommentDto>> GetBySnippetAsync(Guid snippetId, CancellationToken ct = default)
    {
        if (snippetId == Guid.Empty)
            throw new ArgumentException("SnippetId is required", nameof(snippetId));

        var list = await _dbContext.SnippetComments
            .AsNoTracking()
            .Where(c => c.SnippetId == snippetId)
            .OrderBy(c => c.CreatedUtc)
            .ToListAsync(ct);

        return list.Select(Map).ToList();
    }

    private static SnippetCommentDto Map(SnippetComment c) => new()
    {
        Id = c.Id,
        SnippetId = c.SnippetId,
        AuthorUserId = c.AuthorUserId,
        LineStart = c.LineStart,
        LineEnd = c.LineEnd,
        Text = c.Text,
        CreatedUtc = c.CreatedUtc
    };
}
