using CodeMeet.BLL.DTOs.Chat;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Implementations;

public class ChatService : IChatService
{
    private readonly CodeMeetDbContext _dbContext;

    public ChatService(CodeMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ChatMessageDto> SendAsync(CreateChatMessageRequest request, CancellationToken ct = default)
    {
        if (request.SessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Text))
            throw new ArgumentException("Text is required", nameof(request));

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

        var entity = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            AuthorUserId = request.AuthorUserId,
            Text = request.Text,
            CreatedUtc = now
        };

        _dbContext.ChatMessages.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        return Map(entity);
    }

    public async Task<IReadOnlyList<ChatMessageDto>> GetBySessionAsync(Guid sessionId, DateTime? fromUtc = null, CancellationToken ct = default)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        var query = _dbContext.ChatMessages
            .AsNoTracking()
            .Where(m => m.SessionId == sessionId);

        if (fromUtc.HasValue)
            query = query.Where(m => m.CreatedUtc > fromUtc.Value);

        var list = await query
            .OrderBy(m => m.CreatedUtc)
            .ToListAsync(ct);

        return list.Select(Map).ToList();
    }

    private static ChatMessageDto Map(ChatMessage m) => new()
    {
        Id = m.Id,
        SessionId = m.SessionId,
        AuthorUserId = m.AuthorUserId,
        Text = m.Text,
        CreatedUtc = m.CreatedUtc
    };
}
