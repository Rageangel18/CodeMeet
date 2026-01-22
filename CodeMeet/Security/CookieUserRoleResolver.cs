using System.Security.Claims;

namespace CodeMeet.Web.Security;

public interface IUserRoleResolver
{
    bool IsInterviewerOrAdmin(ClaimsPrincipal user);
}

public sealed class CookieUserRoleResolver : IUserRoleResolver
{
    private static readonly string[] AdminRoles =
    [
        "Admin",
        "SuperAdmin"
    ];

    private static readonly string[] InterviewerRoles =
    [
        "Interviewer"
    ];

    private const string OrgIdClaim = "org_id";

    // У вас встречается и org_role, и ClaimTypes.Role
    private static readonly string[] OrgRoleClaimTypes =
    [
        "org_role",
        ClaimTypes.Role
    ];

    private const string OrgAdminRole = "OrgAdmin";

    public bool IsInterviewerOrAdmin(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        // 1) глобальный админ (если есть такие роли)
        if (HasAnyRole(user, AdminRoles))
            return true;

        // 2) интервьюер (если роль есть - даем доступ)
        if (HasAnyRole(user, InterviewerRoles))
            return true;

        // 3) org admin - только если есть org_id и роль OrgAdmin
        if (IsOrgAdmin(user))
            return true;

        return false;
    }

    private static bool HasAnyRole(ClaimsPrincipal user, string[] roles)
    {
        foreach (var r in roles)
        {
            if (user.IsInRole(r))
                return true;

            // на всякий - иногда роли могут быть не в IsInRole, а в claims
            if (user.Claims.Any(c => c.Type == ClaimTypes.Role && string.Equals(c.Value, r, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        return false;
    }

    private static bool IsOrgAdmin(ClaimsPrincipal user)
    {
        var orgClaim = user.FindFirst(OrgIdClaim);
        var isOrgMember = orgClaim != null && Guid.TryParse(orgClaim.Value, out _);

        if (!isOrgMember)
            return false;

        // как в твоем коде: user.IsInRole("OrgAdmin") или claim org_role/role == OrgAdmin
        if (user.IsInRole(OrgAdminRole))
            return true;

        return user.Claims.Any(c =>
            OrgRoleClaimTypes.Contains(c.Type) &&
            string.Equals(c.Value, OrgAdminRole, StringComparison.OrdinalIgnoreCase));
    }
}
