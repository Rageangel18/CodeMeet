using System.Security.Claims;
using CodeMeet.BLL.DTOs.Feedback;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL;
using CodeMeet.DAL.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.Endpoints;

public static class FeedbackEndpoints
{
    public static IEndpointRouteBuilder MapFeedbackEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/feedback");

        group.MapPost("/", async (
            CreateFeedbackRequest body,
            HttpContext httpContext,
            CodeMeetDbContext db,
            IFeedbackService service,
            CancellationToken ct) =>
        {
            var user = httpContext.User;
            if (user?.Identity is null || !user.Identity.IsAuthenticated)
                return Results.Unauthorized();

            if (body.SessionId == Guid.Empty)
                return Results.BadRequest("SessionId is required.");

            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim is null || !Guid.TryParse(idClaim.Value, out var userId))
                return Results.BadRequest("User id claim is missing.");

            var session = await db.Sessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == body.SessionId, ct);

            if (session is null)
                return Results.NotFound();

            var nowUtc = DateTime.UtcNow;
            var isCompletedByTime =
                session.ScheduledEndUtc.HasValue &&
                session.ScheduledEndUtc.Value <= nowUtc;
            var isCompletedByStatus = session.Status == SessionStatus.Completed;

            if (!isCompletedByTime && !isCompletedByStatus)
                return Results.BadRequest("Feedback can be submitted only for completed sessions.");

            var isInterviewer = await db.Participants
                .AnyAsync(p =>
                        p.SessionId == body.SessionId &&
                        p.UserId == userId &&
                        p.Role == ParticipantRole.Interviewer,
                    ct);

            if (!isInterviewer)
                return Results.Forbid();

            body.ReviewerUserId = userId;

            var created = await service.AddAsync(body, ct);
            return Results.Created($"/api/feedback/{created.Id}", created);
        })
        .RequireAuthorization();

        // Получить весь feedback по сессии
        app.MapGet("/api/sessions/{sessionId:guid}/feedback", async (
            Guid sessionId,
            IFeedbackService service,
            CancellationToken ct) =>
        {
            var list = await service.GetBySessionAsync(sessionId, ct);
            return Results.Ok(list);
        });

        return app;
    }
}
