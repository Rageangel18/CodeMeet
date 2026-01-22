using System.Security.Claims;
using CodeMeet.BLL.DTOs.Auth;
using CodeMeet.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            IAuthService authService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await authService.RegisterAsync(request, ct);
            if (!result.Success)
                return Results.BadRequest(result.Error);

            await SignInAsync(httpContext, result);
            return Results.Ok();
        });

        group.MapPost("/login", async (
            LoginRequest request,
            IAuthService authService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await authService.LoginAsync(request, ct);
            if (!result.Success)
                return Results.BadRequest(result.Error);

            await SignInAsync(httpContext, result);
            return Results.Ok();
        });

        group.MapPost("/logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync("Cookies");
            return Results.Ok();
        });

        // -------- ME (current user info) --------
        group.MapGet("/me", (HttpContext httpContext) =>
        {
            var user = httpContext.User;

            if (user?.Identity is null || !user.Identity.IsAuthenticated)
            {
                return Results.Ok(new MeResponse
                {
                    IsAuthenticated = false
                });
            }

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            Guid? userId = null;
            if (userIdClaim is not null && Guid.TryParse(userIdClaim.Value, out var parsed))
            {
                userId = parsed;
            }

            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            var displayName = user.Identity.Name ?? email;

            Guid? orgId = null;
            var orgClaim = user.FindFirst("org_id");
            if (orgClaim is not null && Guid.TryParse(orgClaim.Value, out var parsedOrg))
            {
                orgId = parsedOrg;
            }

            var roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToArray();

            var me = new MeResponse
            {
                IsAuthenticated = true,
                UserId = userId,
                Email = email,
                DisplayName = displayName,
                OrgId = orgId,
                Roles = roles
            };

            return Results.Ok(me);
        });

        return app;
    }

    private static async Task SignInAsync(HttpContext httpContext, AuthResult result)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId.ToString()),
            new(ClaimTypes.Name, string.IsNullOrWhiteSpace(result.DisplayName) ? result.Email : result.DisplayName),
            new(ClaimTypes.Email, result.Email),
        };

        if (result.OrgId != Guid.Empty)
            claims.Add(new("org_id", result.OrgId.ToString()));

        if (result.Roles is not null)
        {
            foreach (var role in result.Roles)
                claims.Add(new(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            "Cookies",                    
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
                AllowRefresh = true
            });
    }


private sealed class MeResponse
    {
        public bool IsAuthenticated { get; set; }
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public Guid? OrgId { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
