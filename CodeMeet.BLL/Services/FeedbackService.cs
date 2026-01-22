using CodeMeet.BLL.DTOs.Feedback;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using CodeMeet.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.BLL.Services.Implementations;

public class FeedbackService : IFeedbackService
{
    private readonly CodeMeetDbContext _dbContext;

    public FeedbackService(CodeMeetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FeedbackDto> AddAsync(CreateFeedbackRequest request, CancellationToken ct = default)
    {
        if (request.SessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required", nameof(request));

        if (request.ReviewerUserId == Guid.Empty)
            throw new ArgumentException("ReviewerUserId is required", nameof(request));

        var sessionExists = await _dbContext.Sessions
            .AnyAsync(s => s.Id == request.SessionId, ct);

        if (!sessionExists)
            throw new InvalidOperationException("Session not found");

        var reviewerExists = await _dbContext.Users
            .AnyAsync(u => u.Id == request.ReviewerUserId, ct);

        if (!reviewerExists)
            throw new InvalidOperationException("Reviewer user not found");

        if (request.CandidateParticipantId.HasValue)
        {
            var candidateExists = await _dbContext.Participants
                .AnyAsync(p => p.Id == request.CandidateParticipantId.Value
                               && p.SessionId == request.SessionId, ct);

            if (!candidateExists)
                throw new InvalidOperationException("Candidate participant not found in this session");
        }

        var recommendation = request.Recommendation ?? Recommendation.Hold;

        var feedbackExists = await _dbContext.Feedback
        .AnyAsync(f =>
            f.SessionId == request.SessionId &&
            f.ReviewerUserId == request.ReviewerUserId &&
            f.CandidateParticipantId == request.CandidateParticipantId,
            ct);

        if (feedbackExists)
            throw new InvalidOperationException(
                "Feedback for this candidate from this reviewer already exists in this session.");

        // Вызов sp_AddFeedback (параметры через интерполяцию = параметризованный запрос)
        var feedbackId = await _dbContext.Database
            .SqlQuery<Guid>($"""
        select public.sp_add_feedback(
            {request.SessionId},
            {request.ReviewerUserId},
            {request.CandidateParticipantId},
            {request.ScoreOverall},
            {request.ScoreTech},
            {request.ScoreComm},
            {recommendation.ToString()},
            {request.Strengths},
            {request.Concerns}
        ) as "Value"
        """)
            .SingleAsync(ct);

        var entity = await _dbContext.Feedback
            .AsNoTracking()
            .Where(f => f.SessionId == request.SessionId
                        && f.ReviewerUserId == request.ReviewerUserId
                        && f.CandidateParticipantId == request.CandidateParticipantId)
            .OrderByDescending(f => f.CreatedUtc)
            .FirstAsync(ct);

        return Map(entity);
    }

    public async Task<IReadOnlyList<FeedbackDto>> GetBySessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        var items = await _dbContext.Feedback
            .AsNoTracking()
            .Where(f => f.SessionId == sessionId)
            .OrderByDescending(f => f.CreatedUtc)
            .ToListAsync(ct);

        return items.Select(Map).ToList();
    }

    private static FeedbackDto Map(Feedback f) =>
        new()
        {
            Id = f.Id,
            SessionId = f.SessionId,
            ReviewerUserId = f.ReviewerUserId,
            CandidateParticipantId = f.CandidateParticipantId,
            ScoreOverall = f.ScoreOverall,
            ScoreTech = f.ScoreTech,
            ScoreComm = f.ScoreComm,
            Recommendation = f.Recommendation,
            Strengths = f.Strengths,
            Concerns = f.Concerns,
            CreatedUtc = f.CreatedUtc
        };
}
