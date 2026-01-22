using CodeMeet.BLL.DTOs.Audit;
using CodeMeet.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CodeMeet.Endpoints;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/audit");

        group.MapPost("/query", async (
            AuditLogQuery query,
            IAuditLogService service,
            CancellationToken ct) =>
        {
            var list = await service.QueryAsync(query, ct);
            return Results.Ok(list);
        });

        return app;
    }
}
