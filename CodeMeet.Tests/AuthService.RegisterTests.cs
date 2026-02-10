using System;
using System.Threading;
using System.Threading.Tasks;
using CodeMeet.BLL.DTOs.Auth;
using CodeMeet.BLL.Services;
using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CodeMeet.Tests.BLL.Services;

[TestFixture]
public class AuthService_RegisterTests
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
    public async Task RegisterAsync_EmailMissing_ReturnsFailure(string? email)
    {
        using var db = CreateDbContext(nameof(RegisterAsync_EmailMissing_ReturnsFailure));
        var sut = new AuthService(db);

        var result = await sut.RegisterAsync(new RegisterRequest
        {
            Email = email,
            Password = "password123",
            DisplayName = "Name"
        }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("Email is required."));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task RegisterAsync_PasswordMissing_ReturnsFailure(string? password)
    {
        using var db = CreateDbContext(nameof(RegisterAsync_PasswordMissing_ReturnsFailure));
        var sut = new AuthService(db);

        var result = await sut.RegisterAsync(new RegisterRequest
        {
            Email = "test@example.com",
            Password = password,
            DisplayName = "Name"
        }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("Password must be at least 8 characters long."));
    }

    [Test]
    public async Task RegisterAsync_PasswordTooShort_ReturnsFailure()
    {
        using var db = CreateDbContext(nameof(RegisterAsync_PasswordTooShort_ReturnsFailure));
        var sut = new AuthService(db);

        var result = await sut.RegisterAsync(new RegisterRequest
        {
            Email = "test@example.com",
            Password = "1234567",
            DisplayName = "Name"
        }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("Password must be at least 8 characters long."));
    }

    [Test]
    public async Task RegisterAsync_EmailAlreadyExists_ReturnsFailure()
    {
        using var db = CreateDbContext(nameof(RegisterAsync_EmailAlreadyExists_ReturnsFailure));
        var sut = new AuthService(db);

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            DisplayName = "Existing",
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
            PasswordHash = "x",
            PasswordSalt = Convert.ToBase64String(new byte[16]),
            PasswordCreatedUtc = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var result = await sut.RegisterAsync(new RegisterRequest
        {
            Email = "  EXISTING@EXAMPLE.COM  ",
            Password = "password123",
            DisplayName = "New"
        }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo("User with this email already exists."));
    }

    [Test]
    public async Task RegisterAsync_ValidRequest_NormalizesEmail_SetsDefaultDisplayName_WhenMissing()
    {
        using var db = CreateDbContext(nameof(RegisterAsync_ValidRequest_NormalizesEmail_SetsDefaultDisplayName_WhenMissing));
        var sut = new AuthService(db);

        var result = await sut.RegisterAsync(new RegisterRequest
        {
            Email = "  TeSt@Example.Com ",
            Password = "password123",
            DisplayName = "   "
        }, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.UserId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.Email, Is.EqualTo("test@example.com"));
        Assert.That(result.DisplayName, Is.EqualTo("test@example.com"));

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == result.UserId);
        Assert.That(user, Is.Not.Null);

        Assert.That(user!.Email, Is.EqualTo("test@example.com"));
        Assert.That(user.DisplayName, Is.EqualTo("test@example.com"));
        Assert.That(user.IsActive, Is.True);
        Assert.That(user.PasswordHash, Is.Not.Null.And.Not.Empty);
        Assert.That(user.PasswordSalt, Is.Not.Null.And.Not.Empty);
        Assert.That(user.PasswordCreatedUtc, Is.Not.Null);
    }

    [Test]
    public async Task RegisterAsync_ValidRequest_TrimsDisplayName_WhenProvided()
    {
        using var db = CreateDbContext(nameof(RegisterAsync_ValidRequest_TrimsDisplayName_WhenProvided));
        var sut = new AuthService(db);

        var result = await sut.RegisterAsync(new RegisterRequest
        {
            Email = "user@example.com",
            Password = "password123",
            DisplayName = "  Yuri  "
        }, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.DisplayName, Is.EqualTo("Yuri"));

        var user = await db.Users.FirstAsync(u => u.Id == result.UserId);
        Assert.That(user.DisplayName, Is.EqualTo("Yuri"));
    }
}
