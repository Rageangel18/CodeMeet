using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeMeet.BLL.Services.Implementations;
using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CodeMeet.Tests.BLL.Services;

[TestFixture]
public class OrganizationService_GetTests
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
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        using var db = CreateDbContext(nameof(GetByIdAsync_NotFound_ReturnsNull));
        var sut = new OrganizationService(db);

        var dto = await sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(dto, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_Found_ReturnsMappedDto()
    {
        using var db = CreateDbContext(nameof(GetByIdAsync_Found_ReturnsMappedDto));
        var sut = new OrganizationService(db);

        var id = Guid.NewGuid();
        db.Organizations.Add(new Organization
        {
            Id = id,
            Name = "Org",
            Slug = "org",
            DataRetentionDays = 99,
            CreatedUtc = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var dto = await sut.GetByIdAsync(id, CancellationToken.None);

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Id, Is.EqualTo(id));
        Assert.That(dto.Name, Is.EqualTo("Org"));
        Assert.That(dto.Slug, Is.EqualTo("org"));
        Assert.That(dto.DataRetentionDays, Is.EqualTo(99));
    }

    [Test]
    public async Task GetListAsync_ReturnsOrderedByName_AndMapped()
    {
        using var db = CreateDbContext(nameof(GetListAsync_ReturnsOrderedByName_AndMapped));
        var sut = new OrganizationService(db);

        db.Organizations.AddRange(
            new Organization { Id = Guid.NewGuid(), Name = "Zeta", Slug = "z", DataRetentionDays = 30, CreatedUtc = DateTime.UtcNow.AddDays(-1) },
            new Organization { Id = Guid.NewGuid(), Name = "Alpha", Slug = "a", DataRetentionDays = 30, CreatedUtc = DateTime.UtcNow.AddDays(-1) },
            new Organization { Id = Guid.NewGuid(), Name = "Beta", Slug = "b", DataRetentionDays = 30, CreatedUtc = DateTime.UtcNow.AddDays(-1) }
        );

        await db.SaveChangesAsync();

        var list = await sut.GetListAsync(CancellationToken.None);

        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list.Select(x => x.Name).ToArray(), Is.EqualTo(new[] { "Alpha", "Beta", "Zeta" }));
        Assert.That(list.All(x => x.Id != Guid.Empty), Is.True);
        Assert.That(list.All(x => !string.IsNullOrWhiteSpace(x.Slug)), Is.True);
    }
}
