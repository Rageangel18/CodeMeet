using CodeMeet.BLL.DTOs.Sessions;
using CodeMeet.BLL.DTOs.Participants;
using CodeMeet.BLL.DTOs.Questions;
using CodeMeet.BLL.DTOs.Feedback;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace CodeMeet.Endpoints;

public static class SessionsEndpoints
{
    public static IEndpointRouteBuilder MapSessionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sessions");

        group.MapPost("/", async (
            CreateSessionRequest request,
            ISessionService service,
            CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request, ct);
            return Results.Created($"/api/sessions/{created.Id}", created);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            ISessionService service,
            CancellationToken ct) =>
        {
            var session = await service.GetByIdAsync(id, ct);
            return session is null ? Results.NotFound() : Results.Ok(session);
        });

        group.MapGet("/{id:guid}/details", async (
            Guid id,
            ISessionService sessionService,
            IParticipantService participantService,
            IQuestionService questionService,
            IFeedbackService feedbackService,
            CancellationToken ct) =>
        {
            var session = await sessionService.GetByIdAsync(id, ct);
            if (session is null)
                return Results.NotFound();

            var participants = await participantService.GetBySessionAsync(id, role: null, ct);
            var questions = await questionService.GetForSessionAsync(id, ct);
            var feedback = await feedbackService.GetBySessionAsync(id, ct);

            var dto = new SessionDetailsResponse
            {
                Session = session,
                Participants = participants,
                Questions = questions,
                Feedback = feedback
            };

            return Results.Ok(dto);
        });

        group.MapPut("/{id:guid}/schedule", async (
            Guid id,
            SessionScheduleRequest body,
            ISessionService service,
            CancellationToken ct) =>
        {
            await service.UpdateScheduleAsync(id, body.ScheduledStartUtc, body.ScheduledEndUtc, ct);
            return Results.NoContent();
        });

        group.MapPut("/{id:guid}/status", async (
            Guid id,
            SessionStatusChangeRequest body,
            ISessionService service,
            CancellationToken ct) =>
        {
            await service.ChangeStatusAsync(id, body.Status, ct);
            return Results.NoContent();
        });

        // НОВОЕ: старт сессии - только если Scheduled
        group.MapPost("/{id:guid}/start", async (
            Guid id,
            ISessionService service,
            CancellationToken ct) =>
        {
            try
            {
                // если ты добавила StartAsync в интерфейс - используй это
                // await service.StartAsync(id, ct);

                // если StartAsync нет - можно сделать так:
                await service.ChangeStatusAsync(id, SessionStatus.Live, ct);

                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                // например: "Only Scheduled sessions can be started."
                return Results.Conflict(ex.Message);
            }
        })
        .RequireAuthorization();

        group.MapPost("/{id:guid}/decision", async (
            Guid id,
            SessionDecisionRequest body,
            IFeedbackService feedbackService,
            CancellationToken ct) =>
        {
            var createFeedback = new CreateFeedbackRequest
            {
                SessionId = id,
                ReviewerUserId = body.ReviewerUserId,
                CandidateParticipantId = body.CandidateParticipantId,
                ScoreOverall = null,
                ScoreTech = null,
                ScoreComm = null,
                Recommendation = body.Recommendation,
                Strengths = body.Strengths,
                Concerns = body.Concerns
            };

            var created = await feedbackService.AddAsync(createFeedback, ct);
            return Results.Created($"/api/feedback/{created.Id}", created);
        });

        group.MapGet("/{id:guid}/summary", async (
            Guid id,
            ISessionReportService reportService,
            CancellationToken ct) =>
        {
            var summary = await reportService.GetSummaryAsync(id, ct);
            return summary is null ? Results.NotFound() : Results.Ok(summary);
        });

        app.MapGet("/api/orgs/{orgId:guid}/sessions", async (
            Guid orgId,
            SessionStatus? status,
            ISessionService sessionService,
            CancellationToken ct) =>
        {
            var sessions = await sessionService.GetByOrgAsync(orgId, status, ct);
            return Results.Ok(sessions);
        })
        .RequireAuthorization();

        app.MapGet("/api/my/sessions", async (
            HttpContext httpContext,
            SessionStatus? status,
            ISessionService sessionService,
            CancellationToken ct) =>
        {
            var user = httpContext.User;
            if (user?.Identity is null || !user.Identity.IsAuthenticated)
                return Results.Unauthorized();

            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim is null || !Guid.TryParse(idClaim.Value, out var userId))
                return Results.BadRequest("User id claim is missing.");

            var sessions = await sessionService.GetByUserParticipationAsync(userId, status, ct);
            return Results.Ok(sessions);
        })
        .RequireAuthorization();

        return app;
    }

    private sealed class SessionScheduleRequest
    {
        public DateTime? ScheduledStartUtc { get; set; }
        public DateTime? ScheduledEndUtc { get; set; }
    }

    private sealed class SessionStatusChangeRequest
    {
        public SessionStatus Status { get; set; }
    }

    private sealed class SessionDetailsResponse
    {
        public SessionDto Session { get; set; } = null!;
        public IReadOnlyList<ParticipantDto> Participants { get; set; } = Array.Empty<ParticipantDto>();
        public IReadOnlyList<QuestionDto> Questions { get; set; } = Array.Empty<QuestionDto>();
        public IReadOnlyList<FeedbackDto> Feedback { get; set; } = Array.Empty<FeedbackDto>();
    }

    private sealed class SessionDecisionRequest
    {
        public Guid CandidateParticipantId { get; set; }
        public Guid ReviewerUserId { get; set; }
        public Recommendation Recommendation { get; set; }
        public string? Strengths { get; set; }
        public string? Concerns { get; set; }
    }
}
