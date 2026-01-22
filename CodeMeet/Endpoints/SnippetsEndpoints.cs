using CodeMeet.BLL.DTOs.Snippets;
using CodeMeet.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CodeMeet.Endpoints;

public static class SnippetsEndpoints
{
    public static IEndpointRouteBuilder MapSnippetsEndpoints(this IEndpointRouteBuilder app)
    {
        // сниппеты по сессии
        var group = app.MapGroup("/api/sessions/{sessionId:guid}/snippets");

        group.MapGet("/", async (
            Guid sessionId,
            bool onlyFinal,
            ISnippetService service,
            CancellationToken ct) =>
        {
            var list = await service.GetBySessionAsync(sessionId, onlyFinal, ct);
            return Results.Ok(list);
        });

        group.MapPost("/", async (
            Guid sessionId,
            CreateSnippetRequest body,
            ISnippetService service,
            CancellationToken ct) =>
        {
            body.SessionId = sessionId;
            var created = await service.CreateAsync(body, ct);
            return Results.Created($"/api/snippets/{created.Id}", created);
        });

        // отметить сниппет как финальное решение
        app.MapPut("/api/snippets/{snippetId:guid}/final", async (
            Guid snippetId,
            ISnippetService service,
            CancellationToken ct) =>
        {
            await service.SetFinalAsync(snippetId, ct);
            return Results.NoContent();
        });

        return app;
    }
}
