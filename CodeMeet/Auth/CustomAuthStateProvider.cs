using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace CodeMeet.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    private UserInfo? _currentUser;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_currentUser is null)
            return Task.FromResult(new AuthenticationState(_anonymous));

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _currentUser.Id.ToString()),
            new Claim(ClaimTypes.Email, _currentUser.Email),
            new Claim(ClaimTypes.Name, _currentUser.DisplayName ?? _currentUser.Email)
        }, "CustomAuth");

        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(principal));
    }

    public void SignIn(Guid userId, string email, string? displayName)
    {
        _currentUser = new UserInfo
        {
            Id = userId,
            Email = email,
            DisplayName = displayName
        };

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void SignOut()
    {
        _currentUser = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private sealed class UserInfo
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
    }
}
