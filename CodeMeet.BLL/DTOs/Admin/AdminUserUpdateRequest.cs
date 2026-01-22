namespace CodeMeet.BLL.Dtos.Admin;

public sealed class AdminUserUpdateRequest
{
    // Email специально не даём править, чтобы не ломать аутентификацию.
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; }
}
