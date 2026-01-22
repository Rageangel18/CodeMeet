using CodeMeet.BLL.DTOs.Organizations;
using CodeMeet.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CodeMeet.Endpoints;

public static class OrganizationsEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orgs");

        group.MapGet("/", async (IOrganizationService service, CancellationToken ct) =>
        {
            var list = await service.GetListAsync(ct);
            return Results.Ok(list);
        });

        group.MapGet("/{id:guid}", async (Guid id, IOrganizationService service, CancellationToken ct) =>
        {
            var org = await service.GetByIdAsync(id, ct);
            return org is null ? Results.NotFound() : Results.Ok(org);
        });

        group.MapPost("/", async (CreateOrganizationRequest request, IOrganizationService service, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request, ct);
            return Results.Created($"/api/orgs/{created.Id}", created);
        });

        return app;
    }
}
