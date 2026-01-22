using CodeMeet.BLL.DTOs.Exports;
using CodeMeet.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CodeMeet.Endpoints;

public static class ExportsEndpoints
{
    public static IEndpointRouteBuilder MapExportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/exports");

        group.MapPost("/", async (
            CreateExportRequest body,
            IExportService service,
            CancellationToken ct) =>
        {
            var created = await service.CreateAsync(body, ct);
            return Results.Created($"/api/exports/{created.Id}", created);
        });

        app.MapGet("/api/orgs/{orgId:guid}/exports", async (
            Guid orgId,
            IExportService service,
            CancellationToken ct) =>
        {
            var list = await service.GetByOrgAsync(orgId, ct);
            return Results.Ok(list);
        });

        app.MapGet("/api/sessions/{sessionId:guid}/exports", async (
            Guid sessionId,
            IExportService service,
            CancellationToken ct) =>
        {
            var list = await service.GetBySessionAsync(sessionId, ct);
            return Results.Ok(list);
        });

        return app;
    }
}
