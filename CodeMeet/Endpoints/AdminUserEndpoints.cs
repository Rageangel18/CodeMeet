using CodeMeet.DAL;
using Microsoft.EntityFrameworkCore;

namespace CodeMeet.Endpoints;

public static class AdminUserEndpoints
{
    public static IEndpointRouteBuilder MapAdminUserEndpoints(
        this IEndpointRouteBuilder app)
    {
        // /api/admin/users

        var group = app.MapGroup("/api/admin/users");

        // GET /api/admin/users
        group.MapGet("/", async (CodeMeetDbContext db, CancellationToken ct) =>
        {
            var list = await db.Users
                .OrderBy(u => u.Email)
                .Select(u => new UserListItemDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    DisplayName = u.DisplayName
                })
                .ToListAsync(ct);

            return Results.Ok(list);
        });

        return app;
    }

    public sealed class UserListItemDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
    }
}
