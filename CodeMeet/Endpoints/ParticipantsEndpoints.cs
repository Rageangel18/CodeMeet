using CodeMeet.BLL.DTOs.Participants;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CodeMeet.Endpoints;

public static class ParticipantsEndpoints
{
    public static IEndpointRouteBuilder MapParticipantsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sessions/{sessionId:guid}/participants");

        group.MapGet("/", async (
            Guid sessionId,
            ParticipantRole? role,
            IParticipantService service,
            CancellationToken ct) =>
        {
            var list = await service.GetBySessionAsync(sessionId, role, ct);
            return Results.Ok(list);
        });

        group.MapPost("/", async (
            Guid sessionId,
            AddOrUpdateParticipantRequest body,
            IParticipantService service,
            CancellationToken ct) =>
        {
            body.SessionId = sessionId;
            var result = await service.AddOrUpdateAsync(body, ct);
            return Results.Ok(result);
        });

        // обновление статуса отдельно по id участника
        app.MapPut("/api/participants/{participantId:guid}/status", async (
            Guid participantId,
            UpdateParticipantStatusRequest body,
            IParticipantService service,
            CancellationToken ct) =>
        {
            body.ParticipantId = participantId;
            await service.UpdateStatusAsync(body, ct);
            return Results.NoContent();
        });

        return app;
    }
}
