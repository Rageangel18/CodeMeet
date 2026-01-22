using System.Security.Cryptography;
using System.Text;
using CodeMeet.BLL.DTOs.Invitations;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Implementations;

public class InvitationService : IInvitationService
{
    private readonly CodeMeetDbContext _dbContext;

    public InvitationService(CodeMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InvitationDto> CreateAsync(CreateInvitationRequest request, CancellationToken ct = default)
    {
        if (request.SessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required", nameof(request));

        var sessionExists = await _dbContext.Sessions
            .AnyAsync(s => s.Id == request.SessionId, ct);

        if (!sessionExists)
            throw new InvalidOperationException("Session not found");

        var exists = await _dbContext.Invitations
            .AnyAsync(i => i.SessionId == request.SessionId && i.Email == request.Email, ct);

        if (exists)
            throw new InvalidOperationException("Invitation already exists for this session and email");

        var now = DateTime.UtcNow;
        var lifetime = request.Lifetime ?? TimeSpan.FromDays(7);

        var token = GenerateToken(request.SessionId, request.Email, now);

        var inv = new Invitation
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            Email = request.Email.Trim(),
            Role = request.Role,
            Token = token,
            ExpiresUtc = now.Add(lifetime),
            CreatedUtc = now
        };

        _dbContext.Invitations.Add(inv);

        var participantExists = await _dbContext.Participants
            .AnyAsync(p => p.SessionId == request.SessionId && p.Email == request.Email && p.Role == request.Role, ct);

        if (!participantExists)
        {
            _dbContext.Participants.Add(new Participant
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId,
                Role = request.Role,
                Email = request.Email.Trim(),
                Status = "invited",
                InvitedUtc = now,
                IsGuest = true,
                CreatedUtc = now
            });
        }

        await _dbContext.SaveChangesAsync(ct);

        return Map(inv);
    }

    public async Task<bool> AcceptAsync(AcceptInvitationRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ArgumentException("Token is required", nameof(request));

        if (request.UserId == Guid.Empty)
            throw new ArgumentException("UserId is required", nameof(request));

        var inv = await _dbContext.Invitations
            .FirstOrDefaultAsync(i => i.Token == request.Token, ct);

        if (inv is null)
            return false;

        var now = DateTime.UtcNow;

        if (inv.ExpiresUtc < now)
            throw new InvalidOperationException("Invitation has expired");

        inv.AcceptedUserId = request.UserId;
        inv.AcceptedUtc = now;

        var participant = await _dbContext.Participants
            .FirstOrDefaultAsync(p =>
                p.SessionId == inv.SessionId &&
                p.Email == inv.Email &&
                p.Role == inv.Role, ct);

        if (participant is not null)
        {
            participant.UserId = request.UserId;
            participant.IsGuest = false;
            participant.Status = "accepted";
        }

        await _dbContext.SaveChangesAsync(ct);

        return true;
    }

    public async Task<IReadOnlyList<InvitationDto>> GetBySessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        var items = await _dbContext.Invitations
            .AsNoTracking()
            .Where(i => i.SessionId == sessionId)
            .OrderByDescending(i => i.CreatedUtc)
            .ToListAsync(ct);

        return items.Select(Map).ToList();
    }

    private static string GenerateToken(Guid sessionId, string email, DateTime now)
    {
        var raw = $"{sessionId}:{email}:{now:O}:{Guid.NewGuid()}";
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static InvitationDto Map(Invitation i) => new()
    {
        Id = i.Id,
        SessionId = i.SessionId,
        Email = i.Email,
        Role = i.Role,
        Token = i.Token,
        ExpiresUtc = i.ExpiresUtc,
        CreatedUtc = i.CreatedUtc,
        SentUtc = i.SentUtc,
        AcceptedUserId = i.AcceptedUserId,
        AcceptedUtc = i.AcceptedUtc
    };
}
