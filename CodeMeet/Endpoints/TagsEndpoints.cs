using CodeMeet.BLL.DTOs.Tags;
using CodeMeet.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CodeMeet.Endpoints;

public static class TagsEndpoints
{
    public static IEndpointRouteBuilder MapTagsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tags");

        group.MapPost("/", async (
            CreateTagRequest body,
            ITagService service,
            CancellationToken ct) =>
        {
            var created = await service.CreateAsync(body, ct);
            return Results.Created($"/api/tags/{created.Id}", created);
        });

        app.MapGet("/api/orgs/{orgId:guid}/tags", async (
            Guid orgId,
            ITagService service,
            CancellationToken ct) =>
        {
            var list = await service.GetByOrgAsync(orgId, ct);
            return Results.Ok(list);
        });

        // теги для вопроса
        app.MapGet("/api/questions/{questionId:guid}/tags", async (
            Guid questionId,
            ITagService service,
            CancellationToken ct) =>
        {
            var list = await service.GetForQuestionAsync(questionId, ct);
            return Results.Ok(list);
        });

        app.MapPut("/api/questions/{questionId:guid}/tags", async (
            Guid questionId,
            QuestionTagsRequest body,
            ITagService service,
            CancellationToken ct) =>
        {
            await service.SetTagsForQuestionAsync(questionId, body.TagIds ?? Array.Empty<Guid>(), ct);
            return Results.NoContent();
        });

        return app;
    }

    private sealed class QuestionTagsRequest
    {
        public IReadOnlyCollection<Guid>? TagIds { get; set; }
    }
}
