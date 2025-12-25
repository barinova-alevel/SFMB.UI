using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using BlazorApp.UI.Auth.Models;
using BlazorApp.UI.Auth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Options;

public class AuthStore
{
    public event Action<UserInfo?>? UserChanged;
    private UserInfo? currentUser;
    public UserInfo? CurrentUser
    {
        get { return currentUser; }
        set
        {
            currentUser = value;

            if (UserChanged is not null)
            {
                UserChanged(currentUser);
            }
        }
    }
}

namespace BlazorApp.UI.Auth
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private AuthenticationState authenticationState;

        public CustomAuthenticationStateProvider(AuthStore service)
        {
            authenticationState = new AuthenticationState(Create(service.CurrentUser));

            service.UserChanged += (newUser) =>
            {
                var principal = Create(newUser);
                authenticationState = new AuthenticationState(principal);

                NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
            };
        }

        private ClaimsPrincipal Create(UserInfo? user)
        {
            if (user == null)
            {
                return new ClaimsPrincipal();
            }

            var claims = new[]
                        {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId),
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.Email, user.Email)
                    };

            var identity = new ClaimsIdentity(claims, "custom");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            return claimsPrincipal;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
            Task.FromResult(authenticationState);
    }
}