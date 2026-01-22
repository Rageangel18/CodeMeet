using CodeMeet.BLL.DTOs.Snippets;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Implementations;

public class SnippetService : ISnippetService
{
    private readonly CodeMeetDbContext _dbContext;

    public SnippetService(CodeMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SnippetDto> CreateAsync(CreateSnippetRequest request, CancellationToken ct = default)
    {
        if (request.SessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Content is required", nameof(request));

        var sessionExists = await _dbContext.Sessions
            .AnyAsync(s => s.Id == request.SessionId, ct);

        if (!sessionExists)
            throw new InvalidOperationException("Session not found");

        if (request.AuthorUserId.HasValue)
        {
            var userExists = await _dbContext.Users
                .AnyAsync(u => u.Id == request.AuthorUserId.Value, ct);

            if (!userExists)
                throw new InvalidOperationException("Author user not found");
        }

        var now = DateTime.UtcNow;

        if (request.MarkAsFinal)
        {
            var existingFinal = await _dbContext.Snippets
                .Where(s => s.SessionId == request.SessionId && s.IsFinalSolution)
                .ToListAsync(ct);

            foreach (var sn in existingFinal)
                sn.IsFinalSolution = false;
        }

        var snippet = new Snippet
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            AuthorUserId = request.AuthorUserId,
            Language = request.Language ?? CodeLanguage.CSharp,
            Filename = request.Filename,
            Content = request.Content,
            IsFinalSolution = request.MarkAsFinal,
            CreatedUtc = now
        };

        _dbContext.Snippets.Add(snippet);

        await _dbContext.SaveChangesAsync(ct);

        return Map(snippet);
    }

    public async Task SetFinalAsync(Guid snippetId, CancellationToken ct = default)
    {
        if (snippetId == Guid.Empty)
            throw new ArgumentException("SnippetId is required", nameof(snippetId));

        var snippet = await _dbContext.Snippets
            .FirstOrDefaultAsync(s => s.Id == snippetId, ct);

        if (snippet is null)
            throw new InvalidOperationException("Snippet not found");

        var sessionId = snippet.SessionId;

        var existingFinal = await _dbContext.Snippets
            .Where(s => s.SessionId == sessionId && s.IsFinalSolution)
            .ToListAsync(ct);

        foreach (var sn in existingFinal)
            sn.IsFinalSolution = false;

        snippet.IsFinalSolution = true;

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SnippetDto>> GetBySessionAsync(Guid sessionId, bool onlyFinal = false, CancellationToken ct = default)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        var query = _dbContext.Snippets
            .AsNoTracking()
            .Where(s => s.SessionId == sessionId);

        if (onlyFinal)
            query = query.Where(s => s.IsFinalSolution);

        var list = await query
            .OrderBy(s => s.CreatedUtc)
            .ToListAsync(ct);

        return list.Select(Map).ToList();
    }

    private static SnippetDto Map(Snippet s) => new()
    {
        Id = s.Id,
        SessionId = s.SessionId,
        AuthorUserId = s.AuthorUserId,
        Language = s.Language,
        Filename = s.Filename,
        Content = s.Content,
        IsFinalSolution = s.IsFinalSolution,
        CreatedUtc = s.CreatedUtc
    };
}
