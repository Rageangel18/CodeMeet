using CodeMeet.BLL.DTOs.Tags;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Implementations;

public class TagService : ITagService
{
    private readonly CodeMeetDbContext _dbContext;

    public TagService(CodeMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TagDto> CreateAsync(CreateTagRequest request, CancellationToken ct = default)
    {
        if (request.OrgId == Guid.Empty)
            throw new ArgumentException("OrgId is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required", nameof(request));

        var orgExists = await _dbContext.Organizations
            .AnyAsync(o => o.Id == request.OrgId, ct);

        if (!orgExists)
            throw new InvalidOperationException("Organization not found");

        var exists = await _dbContext.Tags
            .AnyAsync(t => t.OrgId == request.OrgId && t.Name == request.Name, ct);

        if (exists)
            throw new InvalidOperationException("Tag with this name already exists in organization");

        var now = DateTime.UtcNow;

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            OrgId = request.OrgId,
            Name = request.Name.Trim(),
            CreatedUtc = now
        };

        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync(ct);

        return Map(tag);
    }

    public async Task<IReadOnlyList<TagDto>> GetByOrgAsync(Guid orgId, CancellationToken ct = default)
    {
        if (orgId == Guid.Empty)
            throw new ArgumentException("OrgId is required", nameof(orgId));

        var items = await _dbContext.Tags
            .AsNoTracking()
            .Where(t => t.OrgId == orgId)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

        return items.Select(Map).ToList();
    }

    public async Task SetTagsForQuestionAsync(Guid questionId, IReadOnlyCollection<Guid> tagIds, CancellationToken ct = default)
    {
        if (questionId == Guid.Empty)
            throw new ArgumentException("QuestionId is required", nameof(questionId));

        var question = await _dbContext.Questions
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == questionId, ct);

        if (question is null)
            throw new InvalidOperationException("Question not found");

        if (tagIds.Count > 0)
        {
            var count = await _dbContext.Tags
                .CountAsync(t => tagIds.Contains(t.Id) && t.OrgId == question.OrgId, ct);

            if (count != tagIds.Count)
                throw new InvalidOperationException("Some tags do not belong to the same organization as the question");
        }

        var existing = await _dbContext.QuestionTags
            .Where(qt => qt.QuestionId == questionId)
            .ToListAsync(ct);

        _dbContext.QuestionTags.RemoveRange(existing);

        foreach (var tagId in tagIds)
        {
            _dbContext.QuestionTags.Add(new QuestionTag
            {
                QuestionId = questionId,
                TagId = tagId
            });
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TagDto>> GetForQuestionAsync(Guid questionId, CancellationToken ct = default)
    {
        if (questionId == Guid.Empty)
            throw new ArgumentException("QuestionId is required", nameof(questionId));

        var query =
            from qt in _dbContext.QuestionTags
            join t in _dbContext.Tags on qt.TagId equals t.Id
            where qt.QuestionId == questionId
            orderby t.Name
            select t;

        var list = await query.AsNoTracking().ToListAsync(ct);

        return list.Select(Map).ToList();
    }

    private static TagDto Map(Tag t) => new()
    {
        Id = t.Id,
        OrgId = t.OrgId,
        Name = t.Name,
        CreatedUtc = t.CreatedUtc
    };
}
