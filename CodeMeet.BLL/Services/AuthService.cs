using System.Security.Cryptography;
using CodeMeet.BLL.DTOs.Auth;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services;

public class AuthService : IAuthService
{
    private readonly CodeMeetDbContext _dbContext;

    public AuthService(CodeMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            return new AuthResult { Success = false, Error = "Email is required." };
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return new AuthResult
            {
                Success = false,
                Error = "Password must be at least 8 characters long."
            };
        }

        var exists = await _dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == email, ct);

        if (exists)
        {
            return new AuthResult
            {
                Success = false,
                Error = "User with this email already exists."
            };
        }

        var (hash, salt) = HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? email
                : request.DisplayName.Trim(),
            CreatedUtc = DateTime.UtcNow,
            IsActive = true,
            PasswordHash = hash,
            PasswordSalt = salt,
            PasswordCreatedUtc = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(ct);

        return new AuthResult
        {
            Success = true,
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        };
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResult { Success = false, Error = "Email and password are required." };
        }

        var user = await _dbContext.Users
            .Include(u => u.OrganizationMemberships)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email && u.IsActive, ct);

        if (user is null || string.IsNullOrEmpty(user.PasswordHash) || string.IsNullOrEmpty(user.PasswordSalt))
        {
            return new AuthResult { Success = false, Error = "Invalid email or password." };
        }

        if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return new AuthResult { Success = false, Error = "Invalid email or password." };
        }

        var orgId = user.OrganizationMemberships
    .Where(m => m.IsActive)
    .Select(m => m.OrgId)
    .FirstOrDefault();
        var roles = user.OrganizationMemberships
            .Where(m => m.IsActive)
            .Select(m => m.Role.ToString())
            .ToList();

        return new AuthResult
        {
            Success = true,
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            OrgId = orgId,
            Roles = roles
        };
    }

    private static (string Hash, string Salt) HashPassword(string password)
    {

        var saltBytes = RandomNumberGenerator.GetBytes(16);

        var hashBytes = KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100_000,
            numBytesRequested: 32);

        var hash = Convert.ToBase64String(hashBytes);
        var salt = Convert.ToBase64String(saltBytes);

        return (hash, salt);
    }

    private static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);
        var hashBytes = Convert.FromBase64String(storedHash);

        var candidateBytes = KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100_000,
            numBytesRequested: hashBytes.Length);

        return CryptographicOperations.FixedTimeEquals(candidateBytes, hashBytes);
    }
}
