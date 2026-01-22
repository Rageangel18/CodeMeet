using CodeMeet.BLL.DTOs.Snippets;
using CodeMeet.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CodeMeet.Endpoints;

public static class SnippetCommentsEndpoints
{
    public static IEndpointRouteBuilder MapSnippetCommentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/snippets/{snippetId:guid}/comments");

        group.MapGet("/", async (
            Guid snippetId,
            ISnippetCommentService service,
            CancellationToken ct) =>
        {
            var list = await service.GetBySnippetAsync(snippetId, ct);
            return Results.Ok(list);
        });

        group.MapPost("/", async (
            Guid snippetId,
            CreateSnippetCommentRequest body,
            ISnippetCommentService service,
            CancellationToken ct) =>
        {
            body.SnippetId = snippetId;
            var created = await service.AddAsync(body, ct);
            return Results.Created($"/api/snippets/{snippetId}/comments/{created.Id}", created);
        });

        return app;
    }
}
