namespace CodeMeet.BLL.Dtos.Admin;

public sealed class AdminUserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedUtc { get; init; }
}
