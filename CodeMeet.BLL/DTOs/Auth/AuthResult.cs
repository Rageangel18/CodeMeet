namespace CodeMeet.BLL.DTOs.Auth;

public sealed class AuthResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }

    public Guid? UserId { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public Guid? OrgId { get; init; }
    public IReadOnlyCollection<string>? Roles { get; init; }
}
