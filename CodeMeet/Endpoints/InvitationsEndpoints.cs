using CodeMeet.BLL.DTOs.Invitations;
using CodeMeet.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CodeMeet.Endpoints;

public static class InvitationsEndpoints
{
    public static IEndpointRouteBuilder MapInvitationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invitations");

        group.MapPost("/", async (
            CreateInvitationRequest body,
            IInvitationService service,
            CancellationToken ct) =>
        {
            var created = await service.CreateAsync(body, ct);
            return Results.Created($"/api/invitations/{created.Id}", created);
        });

        group.MapPost("/accept", async (
            AcceptInvitationRequest body,
            IInvitationService service,
            CancellationToken ct) =>
        {
            var ok = await service.AcceptAsync(body, ct);
            return ok ? Results.Ok() : Results.NotFound();
        });

        // все инвайты по сессии
        app.MapGet("/api/sessions/{sessionId:guid}/invitations", async (
            Guid sessionId,
            IInvitationService service,
            CancellationToken ct) =>
        {
            var list = await service.GetBySessionAsync(sessionId, ct);
            return Results.Ok(list);
        });

        return app;
    }
}
