using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeMeet.BLL.Services.Implementations;
using CodeMeet.DAL;
using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CodeMeet.Tests.BLL.Services;

[TestFixture]
public class SessionService_StatusScheduleAndListsTests
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
    public void UpdateScheduleAsync_SessionIdEmpty_ThrowsArgumentException()
    {
        using var db = CreateDbContext(nameof(UpdateScheduleAsync_SessionIdEmpty_ThrowsArgumentException));
        var sut = new SessionService(db);

        Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateScheduleAsync(Guid.Empty, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), CancellationToken.None));
    }

    [Test]
    public void UpdateScheduleAsync_NotFound_ThrowsInvalidOperationException()
    {
        using var db = CreateDbContext(nameof(UpdateScheduleAsync_NotFound_ThrowsInvalidOperationException));
        var sut = new SessionService(db);

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.UpdateScheduleAsync(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddHours(1), CancellationToken.None));
    }

    [Test]
    public async Task UpdateScheduleAsync_StartProvidedEndNull_DefaultsToOneHour_AndPersists()
    {
        using var db = CreateDbContext(nameof(UpdateScheduleAsync_StartProvidedEndNull_DefaultsToOneHour_AndPersists));
        var sut = new SessionService(db);

        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Organizations.Add(NewOrg(orgId));
        db.Users.Add(NewUser(userId, DateTime.UtcNow.AddDays(-1)));

        var sessionId = Guid.NewGuid();
        db.Sessions.Add(new Session
        {
            Id = sessionId,
            OrgId = orgId,
            Title = "S",
            Status = SessionStatus.Draft,
            CreatedByUserId = userId,
            IsExecEnabled = true,
            DefaultLanguage = CodeLanguage.CSharp,
            CreatedUtc = DateTime.UtcNow.AddDays(-1)
        });

        await db.SaveChangesAsync();

        var start = DateTime.UtcNow.AddHours(5);

        await sut.UpdateScheduleAsync(sessionId, start, null, CancellationToken.None);

        var s = await db.Sessions.FirstAsync(x => x.Id == sessionId);
        Assert.That(s.ScheduledStartUtc, Is.EqualTo(start));
        Assert.That(s.ScheduledEndUtc, Is.EqualTo(start.AddHours(1)));
        Assert.That(s.UpdatedUtc, Is.Not.Null);
    }

    [Test]
    public async Task UpdateScheduleAsync_EndBeforeOrEqualStart_ThrowsArgumentException()
    {
        using var db = CreateDbContext(nameof(UpdateScheduleAsync_EndBeforeOrEqualStart_ThrowsArgumentException));
        var sut = new SessionService(db);

        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Organizations.Add(NewOrg(orgId));
        db.Users.Add(NewUser(userId, DateTime.UtcNow.AddDays(-1)));

        var sessionId = Guid.NewGuid();
        db.Sessions.Add(new Session
        {
            Id = sessionId,
            OrgId = orgId,
            Title = "S",
            Status = SessionStatus.Draft,
            CreatedByUserId = userId,
            IsExecEnabled = true,
            DefaultLanguage = CodeLanguage.CSharp,
            CreatedUtc = DateTime.UtcNow.AddDays(-1)
        });

        await db.SaveChangesAsync();

        var start = DateTime.UtcNow.AddHours(5);
        var end = start;

        Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateScheduleAsync(sessionId, start, end, CancellationToken.None));
    }

    [Test]
    public void ChangeStatusAsync_SessionIdEmpty_ThrowsArgumentException()
    {
        using var db = CreateDbContext(nameof(ChangeStatusAsync_SessionIdEmpty_ThrowsArgumentException));
        var sut = new SessionService(db);

        Assert.ThrowsAsync<ArgumentException>(() => sut.ChangeStatusAsync(Guid.Empty, SessionStatus.Scheduled, CancellationToken.None));
    }

    [Test]
    public void ChangeStatusAsync_NotFound_ThrowsInvalidOperationException()
    {
        using var db = CreateDbContext(nameof(ChangeStatusAsync_NotFound_ThrowsInvalidOperationException));
        var sut = new SessionService(db);

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.ChangeStatusAsync(Guid.NewGuid(), SessionStatus.Scheduled, CancellationToken.None));
    }

    [Test]
    public async Task ChangeStatusAsync_InvalidTransition_ThrowsInvalidOperationException()
    {
        using var db = CreateDbContext(nameof(ChangeStatusAsync_InvalidTransition_ThrowsInvalidOperationException));
        var sut = new SessionService(db);

        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Organizations.Add(NewOrg(orgId));
        db.Users.Add(NewUser(userId, DateTime.UtcNow.AddDays(-1)));

        var sessionId = Guid.NewGuid();
        db.Sessions.Add(new Session
        {
            Id = sessionId,
            OrgId = orgId,
            Title = "S",
            Status = SessionStatus.Draft,
            CreatedByUserId = userId,
            IsExecEnabled = true,
            DefaultLanguage = CodeLanguage.CSharp,
            CreatedUtc = DateTime.UtcNow.AddDays(-1)
        });

        await db.SaveChangesAsync();

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.ChangeStatusAsync(sessionId, SessionStatus.Live, CancellationToken.None));
    }

    [Test]
    public async Task ChangeStatusAsync_ValidTransition_PersistsStatus()
    {
        using var db = CreateDbContext(nameof(ChangeStatusAsync_ValidTransition_PersistsStatus));
        var sut = new SessionService(db);

        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Organizations.Add(NewOrg(orgId));
        db.Users.Add(NewUser(userId, DateTime.UtcNow.AddDays(-1)));

        var sessionId = Guid.NewGuid();
        db.Sessions.Add(new Session
        {
            Id = sessionId,
            OrgId = orgId,
            Title = "S",
            Status = SessionStatus.Draft,
            CreatedByUserId = userId,
            IsExecEnabled = true,
            DefaultLanguage = CodeLanguage.CSharp,
            CreatedUtc = DateTime.UtcNow.AddDays(-1)
        });

        await db.SaveChangesAsync();

        await sut.ChangeStatusAsync(sessionId, SessionStatus.Scheduled, CancellationToken.None);

        var s = await db.Sessions.FirstAsync(x => x.Id == sessionId);
        Assert.That(s.Status, Is.EqualTo(SessionStatus.Scheduled));
        Assert.That(s.UpdatedUtc, Is.Not.Null);
    }

    [Test]
    public void StartAsync_SessionIdEmpty_ThrowsArgumentException()
    {
        using var db = CreateDbContext(nameof(StartAsync_SessionIdEmpty_ThrowsArgumentException));
        var sut = new SessionService(db);

        Assert.ThrowsAsync<ArgumentException>(() => sut.StartAsync(Guid.Empty, CancellationToken.None));
    }

    [Test]
    public void StartAsync_NotFound_ThrowsInvalidOperationException()
    {
        using var db = CreateDbContext(nameof(StartAsync_NotFound_ThrowsInvalidOperationException));
        var sut = new SessionService(db);

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.StartAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public async Task StartAsync_StatusNotScheduled_ThrowsInvalidOperationException()
    {
        using var db = CreateDbContext(nameof(StartAsync_StatusNotScheduled_ThrowsInvalidOperationException));
        var sut = new SessionService(db);

        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Organizations.Add(NewOrg(orgId));
        db.Users.Add(NewUser(userId, DateTime.UtcNow.AddDays(-1)));

        var sessionId = Guid.NewGuid();
        db.Sessions.Add(new Session
        {
            Id = sessionId,
            OrgId = orgId,
            Title = "S",
            Status = SessionStatus.Draft,
            ScheduledStartUtc = DateTime.UtcNow.AddHours(-1),
            CreatedByUserId = userId,
            IsExecEnabled = true,
            DefaultLanguage = CodeLanguage.CSharp,
            CreatedUtc = DateTime.UtcNow.AddDays(-1)
        });

        await db.SaveChangesAsync();

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.StartAsync(sessionId, CancellationToken.None));
    }

    [Test]
    public async Task StartAsync_ScheduledStartInFuture_ThrowsInvalidOperationException()
    {
        using var db = CreateDbContext(nameof(StartAsync_ScheduledStartInFuture_ThrowsInvalidOperationException));
        var sut = new SessionService(db);

        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Organizations.Add(NewOrg(orgId));
        db.Users.Add(NewUser(userId, DateTime.UtcNow.AddDays(-1)));

        var sessionId = Guid.NewGuid();
        db.Sessions.Add(new Session
        {
            Id = sessionId,
            OrgId = orgId,
            Title = "S",
            Status = SessionStatus.Scheduled,
            ScheduledStartUtc = DateTime.UtcNow.AddHours(2),
            ScheduledEndUtc = DateTime.UtcNow.AddHours(3),
            CreatedByUserId = userId,
            IsExecEnabled = true,
            DefaultLanguage = CodeLanguage.CSharp,
            CreatedUtc = DateTime.UtcNow.AddDays(-1)
        });

        await db.SaveChangesAsync();

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.StartAsync(sessionId, CancellationToken.None));
    }

    [Test]
    public async Task StartAsync_EndNull_SetsDefaultEnd_AndSetsLive()
    {
        using var db = CreateDbContext(nameof(StartAsync_EndNull_SetsDefaultEnd_AndSetsLive));
        var sut = new SessionService(db);

        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Organizations.Add(NewOrg(orgId));
        db.Users.Add(NewUser(userId, DateTime.UtcNow.AddDays(-1)));

        var sessionId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddHours(-1);

        db.Sessions.Add(new Session
        {
            Id = sessionId,
            OrgId = orgId,
            Title = "S",
            Status = SessionStatus.Scheduled,
            ScheduledStartUtc = start,
            ScheduledEndUtc = null,
            CreatedByUserId = userId,
            IsExecEnabled = true,
            DefaultLanguage = CodeLanguage.CSharp,
            CreatedUtc = DateTime.UtcNow.AddDays(-1)
        });

        await db.SaveChangesAsync();

        await sut.StartAsync(sessionId, CancellationToken.None);

        var s = await db.Sessions.FirstAsync(x => x.Id == sessionId);
        Assert.That(s.Status, Is.EqualTo(SessionStatus.Live));
        Assert.That(s.ScheduledEndUtc, Is.Not.Null);
        Assert.That(s.ScheduledEndUtc, Is.EqualTo(start.AddHours(1)));
        Assert.That(s.UpdatedUtc, Is.Not.Null);
    }

    [Test]
    public void GetByOrgAsync_OrgIdEmpty_ThrowsArgumentException()
    {
        using var db = CreateDbContext(nameof(GetByOrgAsync_OrgIdEmpty_ThrowsArgumentException));
        var sut = new SessionService(db);

        Assert.ThrowsAsync<ArgumentException>(() => sut.GetByOrgAsync(Guid.Empty, null, CancellationToken.None));
    }

    [Test]
    public async Task GetByOrgAsync_PersistsCompleted_ForScheduledEndedSessions_AndReturnsCompleted()
    {
        using var db = CreateDbContext(nameof(GetByOrgAsync_PersistsCompleted_ForScheduledEndedSessions_AndReturnsCompleted));
        var sut = new SessionService(db);

        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Organizations.Add(NewOrg(orgId));
        db.Users.Add(NewUser(userId, DateTime.UtcNow.AddDays(-1)));

        var endedSessionId = Guid.NewGuid();
        db.Sessions.Add(new Session
        {
            Id = endedSessionId,
            OrgId = orgId,
            Title = "Ended",
            Status = SessionStatus.Scheduled,
            ScheduledStartUtc = DateTime.UtcNow.AddHours(-3),
            ScheduledEndUtc = DateTime.UtcNow.AddHours(-2),
            CreatedByUserId = userId,
            IsExecEnabled = true,
            DefaultLanguage = CodeLanguage.CSharp,
            CreatedUtc = DateTime.UtcNow.AddHours(-4)
        });

        await db.SaveChangesAsync();

        var list = await sut.GetByOrgAsync(orgId, null, CancellationToken.None);

        var item = list.FirstOrDefault(x => x.SessionId == endedSessionId);
        Assert.That(item, Is.Not.Null);
        Assert.That(item!.Status, Is.EqualTo(SessionStatus.Completed));

        var tracked = await db.Sessions.FirstAsync(s => s.Id == endedSessionId);
        Assert.That(tracked.Status, Is.EqualTo(SessionStatus.Completed));
        Assert.That(tracked.UpdatedUtc, Is.Not.Null);
    }

    [Test]
    public async Task GetByOrgAsync_StatusFilter_ReturnsOnlyMatchingEffectiveStatus()
    {
        using var db = CreateDbContext(nameof(GetByOrgAsync_StatusFilter_ReturnsOnlyMatchingEffectiveStatus));
        var sut = new SessionService(db);

        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Organizations.Add(NewOrg(orgId));
        db.Users.Add(NewUser(userId, DateTime.UtcNow.AddDays(-1)));

        var completedId = Guid.NewGuid();
        var draftId = Guid.NewGuid();

        db.Sessions.AddRange(
            new Session
            {
                Id = completedId,
                OrgId = orgId,
                Title = "Ended",
                Status = SessionStatus.Scheduled,
                ScheduledStartUtc = DateTime.UtcNow.AddHours(-3),
                ScheduledEndUtc = DateTime.UtcNow.AddHours(-2),
                CreatedByUserId = userId,
                IsExecEnabled = true,
                DefaultLanguage = CodeLanguage.CSharp,
                CreatedUtc = DateTime.UtcNow.AddHours(-4)
            },
            new Session
            {
                Id = draftId,
                OrgId = orgId,
                Title = "Draft",
                Status = SessionStatus.Draft,
                ScheduledStartUtc = DateTime.UtcNow.AddHours(-3),
                ScheduledEndUtc = DateTime.UtcNow.AddHours(-2),
                CreatedByUserId = userId,
                IsExecEnabled = true,
                DefaultLanguage = CodeLanguage.CSharp,
                CreatedUtc = DateTime.UtcNow.AddHours(-4)
            }
        );

        await db.SaveChangesAsync();

        var onlyCompleted = await sut.GetByOrgAsync(orgId, SessionStatus.Completed, CancellationToken.None);

        Assert.That(onlyCompleted.Any(x => x.SessionId == completedId), Is.True);
        Assert.That(onlyCompleted.Any(x => x.SessionId == draftId), Is.False);
    }
}
