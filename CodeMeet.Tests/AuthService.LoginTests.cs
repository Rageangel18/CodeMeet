using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeMeet.BLL.DTOs.Auth;
using CodeMeet.BLL.Services;
using CodeMeet.DAL;
using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CodeMeet.Tests.BLL.Services;

[TestFixture]
public class AuthService_LoginTests
{
    private static CodeMeetDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CodeMeetDbContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return new CodeMeetDbContext(options);
    }

    private static Organization NewOrg(Guid id, string slugSuffix)
    {
        return new Organization
        {
            Id = id,
            Name = $"Org {slugSuffix}",
            Slug = $"org-{slugSuffix}",
            DataRetentionDays = 30,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = null
        };
    }

    [TestCase(null, "password123")]
    [TestCase("", "password123")]
    [TestCase("   ", "password123")]
    [TestCase("user@example.com", null)]
    [TestCase("user@example.com", "")]
    [TestCase("user@example.com", "   ")]
    public async Task LoginAsync_EmailOrPasswordMissing_ReturnsFailure(string? email, string? password)
    {
        using var db = CreateDbContext(nameof(LoginAsync_EmailOrPasswordMissing_ReturnsFailure));
        var sut = new AuthService(db);

        var result = await sut.LoginAsync(new LoginRequest
        {
            Email = email,
            Password = password
        }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("Email and password are required."));
    }

    [Test]
    public async Task LoginAsync_UserNotFound_ReturnsInvalidEmailOrPassword()
    {
        using var db = CreateDbContext(nameof(LoginAsync_UserNotFound_ReturnsInvalidEmailOrPassword));
        var sut = new AuthService(db);

        var result = await sut.LoginAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "password123"
        }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("Invalid email or password."));
    }

    [Test]
    public async Task LoginAsync_UserInactive_ReturnsInvalidEmailOrPassword()
    {
        using var db = CreateDbContext(nameof(LoginAsync_UserInactive_ReturnsInvalidEmailOrPassword));
        var sut = new AuthService(db);

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            DisplayName = "User",
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            IsActive = false,
            PasswordHash = "x",
            PasswordSalt = Convert.ToBase64String(new byte[16]),
            PasswordCreatedUtc = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var result = await sut.LoginAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "password123"
        }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("Invalid email or password."));
    }

    [Test]
    public async Task LoginAsync_NoHashOrSalt_ReturnsInvalidEmailOrPassword()
    {
        using var db = CreateDbContext(nameof(LoginAsync_NoHashOrSalt_ReturnsInvalidEmailOrPassword));
        var sut = new AuthService(db);

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            DisplayName = "User",
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
            PasswordHash = null,
            PasswordSalt = null,
            PasswordCreatedUtc = null
        });
        await db.SaveChangesAsync();

        var result = await sut.LoginAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "password123"
        }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("Invalid email or password."));
    }

    [Test]
    public async Task LoginAsync_WrongPassword_ReturnsInvalidEmailOrPassword()
    {
        using var db = CreateDbContext(nameof(LoginAsync_WrongPassword_ReturnsInvalidEmailOrPassword));
        var sut = new AuthService(db);

        var register = await sut.RegisterAsync(new RegisterRequest
        {
            Email = "user@example.com",
            Password = "password123",
            DisplayName = "User"
        }, CancellationToken.None);

        Assert.That(register.Success, Is.True);

        var result = await sut.LoginAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "wrong-password"
        }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("Invalid email or password."));
    }

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess_AndMapsOrgIdAndRolesFromActiveMemberships()
    {
        using var db = CreateDbContext(nameof(LoginAsync_ValidCredentials_ReturnsSuccess_AndMapsOrgIdAndRolesFromActiveMemberships));
        var sut = new AuthService(db);

        var register = await sut.RegisterAsync(new RegisterRequest
        {
            Email = "  USER@Example.Com ",
            Password = "password123",
            DisplayName = "User"
        }, CancellationToken.None);

        Assert.That(register.Success, Is.True);

        var userId = register.UserId!.Value;

        var org1 = Guid.NewGuid();
        var org2 = Guid.NewGuid();

        db.Organizations.AddRange(
            NewOrg(org1, "one"),
            NewOrg(org2, "two")
        );

        db.OrganizationUsers.AddRange(
            new OrganizationUser
            {
                OrgId = org1,
                UserId = userId,
                Role = default,
                IsActive = true,
                CreatedUtc = DateTime.UtcNow.AddDays(-1)
            },
            new OrganizationUser
            {
                OrgId = org2,
                UserId = userId,
                Role = default,
                IsActive = true,
                CreatedUtc = DateTime.UtcNow.AddDays(-1)
            },
            new OrganizationUser
            {
                OrgId = Guid.NewGuid(),
                UserId = userId,
                Role = default,
                IsActive = false,
                CreatedUtc = DateTime.UtcNow.AddDays(-1)
            }
        );

        await db.SaveChangesAsync();

        var result = await sut.LoginAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "password123"
        }, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.UserId, Is.EqualTo(userId));
        Assert.That(result.Email, Is.EqualTo("user@example.com"));
        Assert.That(result.DisplayName, Is.EqualTo("User"));

        Assert.That(result.OrgId, Is.EqualTo(org1));
        Assert.That(result.Roles, Is.Not.Null);
        Assert.That(result.Roles!.Count, Is.EqualTo(2));
        Assert.That(result.Roles.All(r => r == default(OrgRole).ToString()), Is.True);
    }

    [Test]
    public async Task LoginAsync_IgnoresInactiveMemberships()
    {
        using var db = CreateDbContext(nameof(LoginAsync_IgnoresInactiveMemberships));
        var sut = new AuthService(db);

        var register = await sut.RegisterAsync(new RegisterRequest
        {
            Email = "user@example.com",
            Password = "password123",
            DisplayName = "User"
        }, CancellationToken.None);

        var userId = register.UserId!.Value;

        var activeOrgId = Guid.NewGuid();
        var inactiveOrgId = Guid.NewGuid();

        db.Organizations.AddRange(
            NewOrg(activeOrgId, "active"),
            NewOrg(inactiveOrgId, "inactive")
        );

        db.OrganizationUsers.AddRange(
            new OrganizationUser
            {
                OrgId = inactiveOrgId,
                UserId = userId,
                Role = default,
                IsActive = false,
                CreatedUtc = DateTime.UtcNow.AddDays(-1)
            },
            new OrganizationUser
            {
                OrgId = activeOrgId,
                UserId = userId,
                Role = default,
                IsActive = true,
                CreatedUtc = DateTime.UtcNow.AddDays(-1)
            }
        );

        await db.SaveChangesAsync();

        var result = await sut.LoginAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "password123"
        }, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.OrgId, Is.EqualTo(activeOrgId));
        Assert.That(result.Roles, Is.Not.Null);
        Assert.That(result.Roles!.Count, Is.EqualTo(1));
    }
}
