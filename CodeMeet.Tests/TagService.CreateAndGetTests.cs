using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeMeet.BLL.DTOs.Tags;
using CodeMeet.BLL.Services.Implementations;
using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore.InMemory;

namespace CodeMeet.Tests.BLL.Services;

[TestFixture]
public class TagService_CreateAndGetTests
{
    private static CodeMeetDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CodeMeetDbContext>()
            .UseInMemoryDatabase(dbName)

            .EnableSensitiveDataLogging()
            .Options;

        return new CodeMeetDbContext(options);
    }

    [Test]
    public void CreateAsync_OrgIdEmpty_ThrowsArgumentException()
    {
        using var db = CreateDbContext(nameof(CreateAsync_OrgIdEmpty_ThrowsArgumentException));
        var sut = new TagService(db);

        var req = new CreateTagRequest
        {
            OrgId = Guid.Empty,
            Name = "tag"
        };

        Assert.ThrowsAsync<ArgumentException>(() => sut.CreateAsync(req, CancellationToken.None));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void CreateAsync_NameMissing_ThrowsArgumentException(string? name)
    {
        using var db = CreateDbContext(nameof(CreateAsync_NameMissing_ThrowsArgumentException));
        var sut = new TagService(db);

        var req = new CreateTagRequest
        {
            OrgId = Guid.NewGuid(),
            Name = name!
        };

        Assert.ThrowsAsync<ArgumentException>(() => sut.CreateAsync(req, CancellationToken.None));
    }

    [Test]
    public void CreateAsync_OrgNotFound_ThrowsInvalidOperationException()
    {
        using var db = CreateDbContext(nameof(CreateAsync_OrgNotFound_ThrowsInvalidOperationException));
        var sut = new TagService(db);

        var req = new CreateTagRequest
        {
            OrgId = Guid.NewGuid(),
            Name = "tag"
        };

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateAsync(req, CancellationToken.None));
    }

    [Test]
    public async Task CreateAsync_TagExistsInOrg_ThrowsInvalidOperationException()
    {
        using var db = CreateDbContext(nameof(CreateAsync_TagExistsInOrg_ThrowsInvalidOperationException));
        var sut = new TagService(db);

        var orgId = Guid.NewGuid();

        db.Organizations.Add(new Organization
        {
            Id = orgId,
            Name = "Test Org",
            Slug = "test-org",
            DataRetentionDays = 30,
            CreatedUtc = DateTime.UtcNow
        });

        db.Tags.Add(new Tag
        {
            Id = Guid.NewGuid(),
            OrgId = orgId,
            Name = "DevOps",
            CreatedUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var req = new CreateTagRequest
        {
            OrgId = orgId,
            Name = "DevOps"
        };

        Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.CreateAsync(req, CancellationToken.None)
        );
    }

    [Test]
    public async Task CreateAsync_ValidRequest_CreatesTag_ReturnsDto_AndPersists()
    {
        using var db = CreateDbContext(nameof(CreateAsync_ValidRequest_CreatesTag_ReturnsDto_AndPersists));
        var sut = new TagService(db);

        var orgId = Guid.NewGuid();
        db.Organizations.Add(new Organization
        {
            Id = orgId,
            Name = "Test Org",
            Slug = "test-org",
            DataRetentionDays = 30,
            CreatedUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var before = DateTime.UtcNow;

        var req = new CreateTagRequest
        {
            OrgId = orgId,
            Name = "  Backend  "
        };

        var dto = await sut.CreateAsync(req, CancellationToken.None);

        var after = DateTime.UtcNow;

        Assert.That(dto.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(dto.OrgId, Is.EqualTo(orgId));
        Assert.That(dto.Name, Is.EqualTo("Backend"));
        Assert.That(dto.CreatedUtc, Is.GreaterThanOrEqualTo(before).And.LessThanOrEqualTo(after));

        var fromDb = await db.Tags.FirstOrDefaultAsync(t => t.Id == dto.Id);
        Assert.That(fromDb, Is.Not.Null);
        Assert.That(fromDb!.OrgId, Is.EqualTo(orgId));
        Assert.That(fromDb.Name, Is.EqualTo("Backend"));
        Assert.That(fromDb.CreatedUtc, Is.EqualTo(dto.CreatedUtc));
    }


    [Test]
    public void GetByOrgAsync_OrgIdEmpty_ThrowsArgumentException()
    {
        using var db = CreateDbContext(nameof(GetByOrgAsync_OrgIdEmpty_ThrowsArgumentException));
        var sut = new TagService(db);

        Assert.ThrowsAsync<ArgumentException>(() => sut.GetByOrgAsync(Guid.Empty, CancellationToken.None));
    }

    [Test]
    public async Task GetByOrgAsync_ReturnsOnlyOrgTags_OrderedByName()
    {
        using var db = CreateDbContext(nameof(GetByOrgAsync_ReturnsOnlyOrgTags_OrderedByName));
        var sut = new TagService(db);

        var org1 = Guid.NewGuid();
        var org2 = Guid.NewGuid();

        db.Organizations.Add(new Organization
        {
            Id = org1,
            Name = "Org 1",
            Slug = "org-1",
            DataRetentionDays = 30,
            CreatedUtc = DateTime.UtcNow
        });

        db.Organizations.Add(new Organization
        {
            Id = org2,
            Name = "Org 2",
            Slug = "org-2",
            DataRetentionDays = 30,
            CreatedUtc = DateTime.UtcNow
        });

        db.Tags.AddRange(
            new Tag { Id = Guid.NewGuid(), OrgId = org1, Name = "Zeta", CreatedUtc = DateTime.UtcNow },
            new Tag { Id = Guid.NewGuid(), OrgId = org1, Name = "Alpha", CreatedUtc = DateTime.UtcNow },
            new Tag { Id = Guid.NewGuid(), OrgId = org2, Name = "OtherOrg", CreatedUtc = DateTime.UtcNow }
        );

        await db.SaveChangesAsync();

        var list = await sut.GetByOrgAsync(org1, CancellationToken.None);

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.Select(x => x.Name).ToArray(), Is.EqualTo(new[] { "Alpha", "Zeta" }));
        Assert.That(list.All(x => x.OrgId == org1), Is.True);
    }
}
