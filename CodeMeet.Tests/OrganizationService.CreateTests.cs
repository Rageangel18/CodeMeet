using System;
using System.Threading;
using System.Threading.Tasks;
using CodeMeet.BLL.DTOs.Organizations;
using CodeMeet.BLL.Services.Implementations;
using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CodeMeet.Tests.BLL.Services;

[TestFixture]
public class OrganizationService_CreateTests
{
    private static CodeMeetDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CodeMeetDbContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return new CodeMeetDbContext(options);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void CreateAsync_NameMissing_ThrowsArgumentException(string? name)
    {
        using var db = CreateDbContext(nameof(CreateAsync_NameMissing_ThrowsArgumentException));
        var sut = new OrganizationService(db);

        var req = new CreateOrganizationRequest
        {
            Name = name!,
            Slug = "org-slug",
            DataRetentionDays = null
        };

        Assert.ThrowsAsync<ArgumentException>(() => sut.CreateAsync(req, CancellationToken.None));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void CreateAsync_SlugMissing_ThrowsArgumentException(string? slug)
    {
        using var db = CreateDbContext(nameof(CreateAsync_SlugMissing_ThrowsArgumentException));
        var sut = new OrganizationService(db);

        var req = new CreateOrganizationRequest
        {
            Name = "Org",
            Slug = slug!,
            DataRetentionDays = null
        };

        Assert.ThrowsAsync<ArgumentException>(() => sut.CreateAsync(req, CancellationToken.None));
    }

    [Test]
    public async Task CreateAsync_SlugAlreadyExists_ThrowsInvalidOperationException()
    {
        using var db = CreateDbContext(nameof(CreateAsync_SlugAlreadyExists_ThrowsInvalidOperationException));
        var sut = new OrganizationService(db);

        db.Organizations.Add(new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Existing",
            Slug = "dup",
            DataRetentionDays = 30,
            CreatedUtc = DateTime.UtcNow.AddDays(-10)
        });

        await db.SaveChangesAsync();

        var req = new CreateOrganizationRequest
        {
            Name = "New Org",
            Slug = "dup",
            DataRetentionDays = 365
        };

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateAsync(req, CancellationToken.None));
    }

    [Test]
    public async Task CreateAsync_ValidRequest_TrimsNameAndSlug_DefaultsRetention_AndPersists()
    {
        using var db = CreateDbContext(nameof(CreateAsync_ValidRequest_TrimsNameAndSlug_DefaultsRetention_AndPersists));
        var sut = new OrganizationService(db);

        var before = DateTime.UtcNow;

        var req = new CreateOrganizationRequest
        {
            Name = "  My Org  ",
            Slug = "  my-org  ",
            DataRetentionDays = null
        };

        var dto = await sut.CreateAsync(req, CancellationToken.None);

        var after = DateTime.UtcNow;

        Assert.That(dto.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(dto.Name, Is.EqualTo("My Org"));
        Assert.That(dto.Slug, Is.EqualTo("my-org"));
        Assert.That(dto.DataRetentionDays, Is.EqualTo(365));
        Assert.That(dto.CreatedUtc, Is.GreaterThanOrEqualTo(before).And.LessThanOrEqualTo(after));

        var fromDb = await db.Organizations.FirstOrDefaultAsync(o => o.Id == dto.Id);
        Assert.That(fromDb, Is.Not.Null);
        Assert.That(fromDb!.Name, Is.EqualTo("My Org"));
        Assert.That(fromDb.Slug, Is.EqualTo("my-org"));
        Assert.That(fromDb.DataRetentionDays, Is.EqualTo(365));
    }

    [Test]
    public async Task CreateAsync_ValidRequest_UsesProvidedRetention()
    {
        using var db = CreateDbContext(nameof(CreateAsync_ValidRequest_UsesProvidedRetention));
        var sut = new OrganizationService(db);

        var req = new CreateOrganizationRequest
        {
            Name = "Org",
            Slug = "org",
            DataRetentionDays = 14
        };

        var dto = await sut.CreateAsync(req, CancellationToken.None);

        Assert.That(dto.DataRetentionDays, Is.EqualTo(14));

        var fromDb = await db.Organizations.FirstAsync(o => o.Id == dto.Id);
        Assert.That(fromDb.DataRetentionDays, Is.EqualTo(14));
    }
}
