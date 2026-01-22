using CodeMeet.BLL.DTOs.Exec;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.DAL.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CodeMeet.Endpoints;

public static class ExecRequestsEndpoints
{
    public static IEndpointRouteBuilder MapExecRequestsEndpoints(this IEndpointRouteBuilder app)
    {
        // run: create + publish to Rabbit
        app.MapPost("/api/sessions/{sessionId:guid}/exec/run", async (
            Guid sessionId,
            CreateExecRequestRequest body,
            IExecOrchestrator orchestrator,
            CancellationToken ct) =>
        {
            body.SessionId = sessionId;
            var created = await orchestrator.EnqueueAsync(body, ct);
            return Results.Ok(created);
        });

        // list by session
        app.MapGet("/api/sessions/{sessionId:guid}/exec-requests", async (
            Guid sessionId,
            ExecStatus? status,
            IExecRequestService service,
            CancellationToken ct) =>
        {
            var list = await service.GetBySessionAsync(sessionId, status, ct);
            return Results.Ok(list);
        });

        // get by id
        app.MapGet("/api/exec-requests/{id:guid}", async (
            Guid id,
            IExecRequestService service,
            CancellationToken ct) =>
        {
            var item = await service.GetByIdAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        // complete (used by consumer if you ever want HTTP, но сейчас consumer пишет напрямую в сервис)
        app.MapPut("/api/exec-requests/{id:guid}/complete", async (
            Guid id,
            CompleteExecRequestRequest body,
            IExecRequestService service,
            CancellationToken ct) =>
        {
            body.Id = id;
            await service.CompleteAsync(body, ct);
            return Results.NoContent();
        });

        return app;
    }
}
