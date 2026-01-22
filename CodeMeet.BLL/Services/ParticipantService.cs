using CodeMeet.BLL.DTOs.Participants;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Implementations;

public class ParticipantService : IParticipantService
{
    private readonly CodeMeetDbContext _dbContext;

    public ParticipantService(CodeMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ParticipantDto> AddOrUpdateAsync(AddOrUpdateParticipantRequest request, CancellationToken ct = default)
    {
        if (request.SessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(request));

        if (request.UserId is null && string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Either UserId or Email must be provided");

        var sessionExists = await _dbContext.Sessions
            .AnyAsync(s => s.Id == request.SessionId, ct);

        if (!sessionExists)
            throw new InvalidOperationException("Session not found");

        var query = _dbContext.Participants
            .Where(p => p.SessionId == request.SessionId && p.Role == request.Role);

        if (request.UserId.HasValue)
            query = query.Where(p => p.UserId == request.UserId.Value);
        else if (!string.IsNullOrWhiteSpace(request.Email))
            query = query.Where(p => p.Email == request.Email);

        var entity = await query.FirstOrDefaultAsync(ct);
        var now = DateTime.UtcNow;

        if (entity is null)
        {
            entity = new Participant
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId,
                UserId = request.UserId,
                Role = request.Role,
                DisplayName = request.DisplayName,
                Email = request.Email,
                InvitedUtc = now,
                Status = "invited",
                IsGuest = !request.UserId.HasValue,
                CreatedUtc = now
            };

            _dbContext.Participants.Add(entity);
        }
        else
        {
            if (request.DisplayName is not null)
                entity.DisplayName = request.DisplayName;

            if (!string.IsNullOrWhiteSpace(request.Email))
                entity.Email = request.Email;

            if (request.UserId.HasValue)
            {
                entity.UserId = request.UserId.Value;
                entity.IsGuest = false;
            }
        }

        await _dbContext.SaveChangesAsync(ct);

        return Map(entity);
    }

    public async Task UpdateStatusAsync(UpdateParticipantStatusRequest request, CancellationToken ct = default)
    {
        if (request.ParticipantId == Guid.Empty)
            throw new ArgumentException("ParticipantId is required", nameof(request));

        var entity = await _dbContext.Participants
            .FirstOrDefaultAsync(p => p.Id == request.ParticipantId, ct);

        if (entity is null)
            throw new InvalidOperationException("Participant not found");

        var now = DateTime.UtcNow;

        entity.Status = request.Status;

        if (request.SetJoinedNow)
            entity.JoinedUtc = now;

        if (request.SetLeftNow)
            entity.LeftUtc = now;

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ParticipantDto>> GetBySessionAsync(Guid sessionId, ParticipantRole? role = null, CancellationToken ct = default)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        var query = _dbContext.Participants
            .AsNoTracking()
            .Where(p => p.SessionId == sessionId);

        if (role.HasValue)
            query = query.Where(p => p.Role == role.Value);

        var items = await query
            .OrderBy(p => p.CreatedUtc)
            .ToListAsync(ct);

        return items.Select(Map).ToList();
    }

    private static ParticipantDto Map(Participant p) => new()
    {
        Id = p.Id,
        SessionId = p.SessionId,
        UserId = p.UserId,
        Role = p.Role,
        DisplayName = p.DisplayName,
        Email = p.Email,
        InvitedUtc = p.InvitedUtc,
        JoinedUtc = p.JoinedUtc,
        LeftUtc = p.LeftUtc,
        Status = p.Status ?? string.Empty,
        IsGuest = p.IsGuest,
        CreatedUtc = p.CreatedUtc
    };
}
