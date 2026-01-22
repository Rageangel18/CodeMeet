namespace CodeMeet.BLL.DTOs.Auth;

public sealed class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string Password { get; set; } = string.Empty;
}
