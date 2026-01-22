using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.Endpoints;

public static class AdminOrganizationEndpoints
{
    public static IEndpointRouteBuilder MapAdminOrganizationEndpoints(
        this IEndpointRouteBuilder app)
    {
        // /api/admin/organizations

        var group = app.MapGroup("/api/admin/organizations");

        // GET /api/admin/organizations
        group.MapGet("/", async (CodeMeetDbContext db, CancellationToken ct) =>
        {
            var list = await db.Organizations
                .OrderBy(o => o.Name)
                .Select(o => new OrgListItemDto
                {
                    Id = o.Id,
                    Name = o.Name
                })
                .ToListAsync(ct);

            return Results.Ok(list);
        });

        return app;
    }

    public sealed class OrgListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
