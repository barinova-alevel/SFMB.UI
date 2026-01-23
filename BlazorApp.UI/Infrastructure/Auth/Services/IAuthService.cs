using BlazorApp.UI.Infrastructure.Auth.Models;

namespace BlazorApp.UI.Infrastructure.Auth.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task LogoutAsync();
        Task<UserInfo?> GetCurrentUserAsync();
    }
}
