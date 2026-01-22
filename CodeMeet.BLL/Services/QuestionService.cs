using CodeMeet.BLL.DTOs.Questions;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Implementations;

public class QuestionService : IQuestionService
{
    private readonly CodeMeetDbContext _dbContext;

    public QuestionService(CodeMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<QuestionDto> CreateAsync(CreateQuestionRequest request, CancellationToken ct = default)
    {
        if (request.OrgId == Guid.Empty)
            throw new ArgumentException("OrgId is required", nameof(request));

        if (request.CreatedByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Body))
            throw new ArgumentException("Body is required", nameof(request));

        var orgExists = await _dbContext.Organizations
            .AnyAsync(o => o.Id == request.OrgId, ct);

        if (!orgExists)
            throw new InvalidOperationException("Organization not found");

        var userExists = await _dbContext.Users
            .AnyAsync(u => u.Id == request.CreatedByUserId, ct);

        if (!userExists)
            throw new InvalidOperationException("User not found");

        var now = DateTime.UtcNow;

        var q = new Question
        {
            Id = Guid.NewGuid(),
            OrgId = request.OrgId,
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            Level = request.Level,
            CreatedByUserId = request.CreatedByUserId,
            CreatedUtc = now
        };

        _dbContext.Questions.Add(q);
        await _dbContext.SaveChangesAsync(ct);

        return Map(q);
    }

    public async Task<QuestionDto?> UpdateAsync(UpdateQuestionRequest request, CancellationToken ct = default)
    {
        if (request.Id == Guid.Empty)
            throw new ArgumentException("Id is required", nameof(request));

        var q = await _dbContext.Questions
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (q is null)
            return null;

        var changed = false;

        if (!string.IsNullOrWhiteSpace(request.Title) && request.Title != q.Title)
        {
            q.Title = request.Title.Trim();
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(request.Body) && request.Body != q.Body)
        {
            q.Body = request.Body.Trim();
            changed = true;
        }

        if (request.Level.HasValue && request.Level.Value != q.Level)
        {
            q.Level = request.Level.Value;
            changed = true;
        }

        if (changed)
        {
            q.UpdatedUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(ct);
        }

        return Map(q);
    }

    public async Task<IReadOnlyList<QuestionDto>> GetByOrgAsync(Guid orgId, QuestionLevel? level = null, CancellationToken ct = default)
    {
        if (orgId == Guid.Empty)
            throw new ArgumentException("OrgId is required", nameof(orgId));

        var query = _dbContext.Questions
            .AsNoTracking()
            .Where(q => q.OrgId == orgId);

        if (level.HasValue)
            query = query.Where(q => q.Level == level.Value);

        var list = await query
            .OrderByDescending(q => q.CreatedUtc)
            .ToListAsync(ct);

        return list.Select(Map).ToList();
    }

    public async Task AssignToSessionAsync(Guid sessionId, IReadOnlyCollection<Guid> questionIds, CancellationToken ct = default)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        var session = await _dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

        if (session is null)
            throw new InvalidOperationException("Session not found");

        if (questionIds.Count > 0)
        {
            var count = await _dbContext.Questions
                .CountAsync(q => questionIds.Contains(q.Id) && q.OrgId == session.OrgId, ct);

            if (count != questionIds.Count)
                throw new InvalidOperationException("Some questions do not belong to the same organization as the session");
        }

        var existing = await _dbContext.SessionQuestions
            .Where(sq => sq.SessionId == sessionId)
            .ToListAsync(ct);

        _dbContext.SessionQuestions.RemoveRange(existing);

        var index = 0;
        foreach (var qId in questionIds)
        {
            _dbContext.SessionQuestions.Add(new SessionQuestion
            {
                SessionId = sessionId,
                QuestionId = qId,
                OrderIndex = index++
            });
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<QuestionDto>> GetForSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        var query =
            from sq in _dbContext.SessionQuestions
            join q in _dbContext.Questions on sq.QuestionId equals q.Id
            where sq.SessionId == sessionId
            orderby sq.OrderIndex
            select q;

        var list = await query.AsNoTracking().ToListAsync(ct);

        return list.Select(Map).ToList();
    }

    public async Task<QuestionDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id is required", nameof(id));

        var entity = await _dbContext.Questions
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id, ct);

        if (entity is null)
            return null;

        return Map(entity);
    }

    public async Task<IReadOnlyList<QuestionDto>> SearchInOrgAsync(
    Guid orgId,
    string? search = null,
    IReadOnlyCollection<Guid>? tagIds = null,
    QuestionLevel? level = null,
    CancellationToken ct = default)
    {
        if (orgId == Guid.Empty)
            throw new ArgumentException("OrgId is required", nameof(orgId));

        var query = _dbContext.Questions
            .AsNoTracking()
            .Where(q => q.OrgId == orgId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(q =>
                q.Title.Contains(term) ||
                q.Body.Contains(term));
        }

        if (level.HasValue)
        {
            query = query.Where(q => q.Level == level.Value);
        }

        if (tagIds is { Count: > 0 })
        {
            query =
                (from q in query
                 join qt in _dbContext.QuestionTags on q.Id equals qt.QuestionId
                 where tagIds.Contains(qt.TagId)
                 select q)
                .Distinct();
        }

        var list = await query
            .OrderByDescending(q => q.CreatedUtc)
            .ToListAsync(ct);

        return list.Select(Map).ToList();
    }

    private static QuestionDto Map(Question q) => new()
    {
        Id = q.Id,
        OrgId = q.OrgId,
        Title = q.Title,
        Body = q.Body,
        Level = q.Level,
        CreatedByUserId = q.CreatedByUserId,
        CreatedUtc = q.CreatedUtc,
        UpdatedUtc = q.UpdatedUtc
    };
}
