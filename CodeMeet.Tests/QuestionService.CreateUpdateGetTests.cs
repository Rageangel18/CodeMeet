using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeMeet.BLL.DTOs.Questions;
using CodeMeet.BLL.Services.Implementations;
using CodeMeet.DAL;
using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CodeMeet.Tests.BLL.Services;

[TestFixture]
public class QuestionService_CreateUpdateGetTests
{
    private static CodeMeetDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CodeMeetDbContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return new CodeMeetDbContext(options);
    }

    private static Organization NewOrg(Guid id)
    {
        return new Organization
        {
            Id = id,
            Name = $"Org {id:N}",
            Slug = $"org-{id:N}",
            DataRetentionDays = 30,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = null
        };
    }

    private static User NewUser(Guid id, DateTime createdUtc)
    {
        return new User
        {
            Id = id,
            Email = $"user-{id:N}@example.local",
            DisplayName = null,
            CreatedUtc = createdUtc,
            IsActive = true,
            PasswordHash = null,
            PasswordSalt = null,
            PasswordCreatedUtc = null
        };
    }

    [Test]
    public void CreateAsync_OrgIdEmpty_ThrowsArgumentException()
    {
        using var db = CreateDbContext(nameof(CreateAsync_OrgIdEmpty_ThrowsArgumentException));
        var sut = new QuestionService(db);

        var req = new CreateQuestionRequest
        {
            OrgId = Guid.Empty,
            CreatedByUserId = Guid.NewGuid(),
            Title = "T",
            Body = "B",
            Level = QuestionLevel.Medium
        };

        Assert.ThrowsAsync<ArgumentException>(() => sut.CreateAsync(req, CancellationToken.None));
    }

    [Test]
    public void CreateAsync_CreatedByUserIdEmpty_ThrowsArgumentException()
    {
        using var db = CreateDbContext(nameof(CreateAsync_CreatedByUserIdEmpty_ThrowsArgumentException));
        var sut = new QuestionService(db);

        var req = new CreateQuestionRequest
        {
            OrgId = Guid.NewGuid(),
            CreatedByUserId = Guid.Empty,
            Title = "T",
            Body = "B",
            Level = QuestionLevel.Medium
        };

        Assert.ThrowsAsync<ArgumentException>(() => sut.CreateAsync(req, CancellationToken.None));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void CreateAsync_TitleMissing_ThrowsArgumentException(string? title)
    {
        using var db = CreateDbContext(nameof(CreateAsync_TitleMissing_ThrowsArgumentException));
        var sut = new QuestionService(db);

        var req = new CreateQuestionRequest
        {
            OrgId = Guid.NewGuid(),
            CreatedByUserId = Guid.NewGuid(),
            Title = title!,
            Body = "B",
            Level = QuestionLevel.Medium
        };

        Assert.ThrowsAsync<ArgumentException>(() => sut.CreateAsync(req, CancellationToken.None));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void CreateAsync_BodyMissing_ThrowsArgumentException(string? body)
    {
        using var db = CreateDbContext(nameof(CreateAsync_BodyMissing_ThrowsArgumentException));
        var sut = new QuestionService(db);

        var req = new CreateQuestionRequest
        {
            OrgId = Guid.NewGuid(),
            CreatedByUserId = Guid.NewGuid(),
            Title = "T",
            Body = body!,
            Level = QuestionLevel.Medium
        };

        Assert.ThrowsAsync<ArgumentException>(() => sut.CreateAsync(req, CancellationToken.None));
    }

    [Test]
    public async Task CreateAsync_OrgNotFound_ThrowsInvalidOperationException()
    {
        using var db = CreateDbContext(nameof(CreateAsync_OrgNotFound_ThrowsInvalidOperationException));
        var sut = new QuestionService(db);

        var req = new CreateQuestionRequest
        {
            OrgId = Guid.NewGuid(),
            CreatedByUserId = Guid.NewGuid(),
            Title = "T",
            Body = "B",
            Level = QuestionLevel.Medium
        };

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateAsync(req, CancellationToken.None));
    }

    [Test]
    public async Task CreateAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        using var db = CreateDbContext(nameof(CreateAsync_UserNotFound_ThrowsInvalidOperationException));
        var sut = new QuestionService(db);

        var orgId = Guid.NewGuid();
        db.Organizations.Add(NewOrg(orgId));
        await db.SaveChangesAsync();

        var req = new CreateQuestionRequest
        {
            OrgId = orgId,
            CreatedByUserId = Guid.NewGuid(),
            Title = "T",
            Body = "B",
            Level = QuestionLevel.Medium
        };

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateAsync(req, CancellationToken.None));
    }

    [Test]
    public async Task CreateAsync_ValidRequest_TrimsFields_Persists_AndReturnsDto()
    {
        using var db = CreateDbContext(nameof(CreateAsync_ValidRequest_TrimsFields_Persists_AndReturnsDto));
        var sut = new QuestionService(db);

        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        db.Organizations.Add(NewOrg(orgId));
        db.Users.Add(NewUser(userId, DateTime.UtcNow.AddDays(-1)));
        await db.SaveChangesAsync();

        var before = DateTime.UtcNow;

        var req = new CreateQuestionRequest
        {
            OrgId = orgId,
            CreatedByUserId = userId,
            Title = "  Title  ",
            Body = "  Body  ",
            Level = QuestionLevel.Hard
        };

        var dto = await sut.CreateAsync(req, CancellationToken.None);

        var after = DateTime.UtcNow;

        Assert.That(dto.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(dto.OrgId, Is.EqualTo(orgId));
        Assert.That(dto.CreatedByUserId, Is.EqualTo(userId));
        Assert.That(dto.Title, Is.EqualTo("Title"));
        Assert.That(dto.Body, Is.EqualTo("Body"));
        Assert.That(dto.Level, Is.EqualTo(QuestionLevel.Hard));
        Assert.That(dto.CreatedUtc, Is.GreaterThanOrEqualTo(before).And.LessThanOrEqualTo(after));
        Assert.That(dto.UpdatedUtc, Is.Null);

        var fromDb = await db.Questions.FirstOrDefaultAsync(x => x.Id == dto.Id);
        Assert.That(fromDb, Is.Not.Null);
        Assert.That(fromDb!.Title, Is.EqualTo("Title"));
        Assert.That(fromDb.Body, Is.EqualTo("Body"));
        Assert.That(fromDb.Level, Is.EqualTo(QuestionLevel.Hard));
        Assert.That(fromDb.UpdatedUtc, Is.Null);
    }

    [Test]
    public void UpdateAsync_IdEmpty_ThrowsArgumentException()
    {
        using var db = CreateDbContext(nameof(UpdateAsync_IdEmpty_ThrowsArgumentException));
        var sut = new QuestionService(db);

        var req = new UpdateQuestionRequest
        {
            Id = Guid.Empty
        };

        Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateAsync(req, CancellationToken.None));
    }

    [Test]
    public async Task UpdateAsync_NotFound_ReturnsNull()
    {
        using var db = CreateDbContext(nameof(UpdateAsync_NotFound_ReturnsNull));
        var sut = new QuestionService(db);

        var req = new UpdateQuestionRequest
        {
            Id = Guid.NewGuid(),
            Title = "X"
        };

        var result = await sut.UpdateAsync(req, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_NoChanges_DoesNotSetUpdatedUtc_StillReturnsDto()
    {
        using var db = CreateDbContext(nameof(UpdateAsync_NoChanges_DoesNotSetUpdatedUtc_StillReturnsDto));
        var sut = new QuestionService(db);

        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        db.Organizations.Add(NewOrg(orgId));
        db.Users.Add(NewUser(userId, DateTime.UtcNow.AddDays(-1)));

        var qId = Guid.NewGuid();
        db.Questions.Add(new Question
        {
            Id = qId,
            OrgId = orgId,
            Title = "Title",
            Body = "Body",
            Level = QuestionLevel.Medium,
            CreatedByUserId = userId,
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            UpdatedUtc = null
        });

        await db.SaveChangesAsync();

        var req = new UpdateQuestionRequest
        {
            Id = qId,
            Title = "Title",
            Body = "Body",
            Level = QuestionLevel.Medium
        };

        var dto = await sut.UpdateAsync(req, CancellationToken.None);

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.UpdatedUtc, Is.Null);

        var fromDb = await db.Questions.FirstAsync(x => x.Id == qId);
        Assert.That(fromDb.UpdatedUtc, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_ChangesTitleBodyLevel_SetsUpdatedUtc_AndPersists()
    {
        using var db = CreateDbContext(nameof(UpdateAsync_ChangesTitleBodyLevel_SetsUpdatedUtc_AndPersists));
        var sut = new QuestionService(db);

        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        db.Organizations.Add(NewOrg(orgId));
        db.Users.Add(NewUser(userId, DateTime.UtcNow.AddDays(-1)));

        var qId = Guid.NewGuid();
        db.Questions.Add(new Question
        {
            Id = qId,
            OrgId = orgId,
            Title = "Old",
            Body = "OldBody",
            Level = QuestionLevel.Easy,
            CreatedByUserId = userId,
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            UpdatedUtc = null
        });

        await db.SaveChangesAsync();

        var before = DateTime.UtcNow;

        var req = new UpdateQuestionRequest
        {
            Id = qId,
            Title = "  New  ",
            Body = "  NewBody  ",
            Level = QuestionLevel.Hard
        };

        var dto = await sut.UpdateAsync(req, CancellationToken.None);

        var after = DateTime.UtcNow;

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Title, Is.EqualTo("New"));
        Assert.That(dto.Body, Is.EqualTo("NewBody"));
        Assert.That(dto.Level, Is.EqualTo(QuestionLevel.Hard));
        Assert.That(dto.UpdatedUtc, Is.Not.Null);
        Assert.That(dto.UpdatedUtc, Is.GreaterThanOrEqualTo(before).And.LessThanOrEqualTo(after));

        var fromDb = await db.Questions.FirstAsync(x => x.Id == qId);
        Assert.That(fromDb.Title, Is.EqualTo("New"));
        Assert.That(fromDb.Body, Is.EqualTo("NewBody"));
        Assert.That(fromDb.Level, Is.EqualTo(QuestionLevel.Hard));
        Assert.That(fromDb.UpdatedUtc, Is.Not.Null);
    }

    [Test]
    public void GetByIdAsync_IdEmpty_ThrowsArgumentException()
    {
        using var db = CreateDbContext(nameof(GetByIdAsync_IdEmpty_ThrowsArgumentException));
        var sut = new QuestionService(db);

        Assert.ThrowsAsync<ArgumentException>(() => sut.GetByIdAsync(Guid.Empty, CancellationToken.None));
    }

    [Test]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        using var db = CreateDbContext(nameof(GetByIdAsync_NotFound_ReturnsNull));
        var sut = new QuestionService(db);

        var result = await sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_Found_ReturnsMappedDto()
    {
        using var db = CreateDbContext(nameof(GetByIdAsync_Found_ReturnsMappedDto));
        var sut = new QuestionService(db);

        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        db.Organizations.Add(NewOrg(orgId));
        db.Users.Add(NewUser(userId, DateTime.UtcNow.AddDays(-2)));

        var qId = Guid.NewGuid();
        db.Questions.Add(new Question
        {
            Id = qId,
            OrgId = orgId,
            Title = "T",
            Body = "B",
            Level = QuestionLevel.Medium,
            CreatedByUserId = userId,
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            UpdatedUtc = null
        });

        await db.SaveChangesAsync();

        var dto = await sut.GetByIdAsync(qId, CancellationToken.None);

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Id, Is.EqualTo(qId));
        Assert.That(dto.OrgId, Is.EqualTo(orgId));
        Assert.That(dto.CreatedByUserId, Is.EqualTo(userId));
        Assert.That(dto.Title, Is.EqualTo("T"));
        Assert.That(dto.Body, Is.EqualTo("B"));
        Assert.That(dto.Level, Is.EqualTo(QuestionLevel.Medium));
        Assert.That(dto.UpdatedUtc, Is.Null);
    }
}
