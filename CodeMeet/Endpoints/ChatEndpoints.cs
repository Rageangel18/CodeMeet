using CodeMeet.BLL.DTOs.Chat;
using CodeMeet.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CodeMeet.Endpoints;

public static class ChatEndpoints
{
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sessions/{sessionId:guid}/chat");

        group.MapGet("/", async (
            Guid sessionId,
            DateTime? fromUtc,
            IChatService service,
            CancellationToken ct) =>
        {
            var list = await service.GetBySessionAsync(sessionId, fromUtc, ct);
            return Results.Ok(list);
        });

        group.MapPost("/", async (
            Guid sessionId,
            CreateChatMessageRequest body,
            IChatService service,
            CancellationToken ct) =>
        {
            body.SessionId = sessionId;
            var created = await service.SendAsync(body, ct);
            return Results.Created($"/api/sessions/{sessionId}/chat/{created.Id}", created);
        });

        return app;
    }
}
