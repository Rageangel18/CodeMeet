using CodeMeet.BLL.DTOs.Questions;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using CodeMeet.DAL.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CodeMeet.Endpoints;

public static class QuestionsEndpoints
{
    public static IEndpointRouteBuilder MapQuestionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/questions");

        group.MapPost("/", async (
            CreateQuestionRequest body,
            IQuestionService service,
            CancellationToken ct) =>
        {
            var created = await service.CreateAsync(body, ct);
            return Results.Created($"/api/questions/{created.Id}", created);
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateQuestionRequest body,
            IQuestionService service,
            CancellationToken ct) =>
        {
            body.Id = id;
            var updated = await service.UpdateAsync(body, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        // список вопросов по организации
        app.MapGet("/api/orgs/{orgId:guid}/questions", async (
            Guid orgId,
            QuestionLevel? level,
            IQuestionService service,
            CancellationToken ct) =>
        {
            var list = await service.GetByOrgAsync(orgId, level, ct);
            return Results.Ok(list);
        });

        app.MapGet("/api/sessions/{sessionId:guid}/questions", async (
            Guid sessionId,
            IQuestionService service,
            CancellationToken ct) =>
        {
            var list = await service.GetForSessionAsync(sessionId, ct);
            return Results.Ok(list);
        });

        app.MapPost("/api/sessions/{sessionId:guid}/questions", async (
            Guid sessionId,
            SessionQuestionsRequest body,
            HttpContext httpContext,
            CodeMeetDbContext db,
            IQuestionService service,
            CancellationToken ct) =>
        {
            var user = httpContext.User;
            if (user?.Identity is null || !user.Identity.IsAuthenticated)
                return Results.Unauthorized();

            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim is null || !Guid.TryParse(idClaim.Value, out var userId))
                return Results.BadRequest("User id claim is missing.");

            var session = await db.Sessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

            if (session is null)
                return Results.NotFound();

            var nowUtc = DateTime.UtcNow;

            var isCompletedByTime =
                session.ScheduledEndUtc.HasValue &&
                session.ScheduledEndUtc.Value <= nowUtc;

            var isCompletedByStatus = session.Status == SessionStatus.Completed;

            if (!isCompletedByTime && !isCompletedByStatus)
                return Results.BadRequest("Questions can be assigned only for completed sessions.");

            var isInterviewer = await db.Participants
                .AnyAsync(p =>
                        p.SessionId == sessionId &&
                        p.UserId == userId &&
                        p.Role == ParticipantRole.Interviewer,
                    ct);

            if (!isInterviewer)
                return Results.Forbid();

            await service.AssignToSessionAsync(
                sessionId,
                body.QuestionIds ?? Array.Empty<Guid>(),
                ct);

            return Results.NoContent();
        })
        .RequireAuthorization();

        return app;
    }

    private sealed class SessionQuestionsRequest
    {
        public IReadOnlyCollection<Guid>? QuestionIds { get; set; }
    }
}
