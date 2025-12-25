using System.Net.Http.Json;
using System.Security.Claims;
using BlazorApp.UI.Auth.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BlazorApp.UI.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthStore _authStore;
        private readonly ILogger<AuthService> _logger;
        private const string UserSessionKey = "UserSession";

        public AuthService(
            IHttpClientFactory httpClientFactory,
            AuthStore authStore,
            ILogger<AuthService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _authStore = authStore;
            _logger = logger;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.PostAsJsonAsync("api/auth/login", request);

                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserInfo>();

                    if (user != null)
                    {
                        _authStore.CurrentUser = user;
                        return new()
                        {
                            Success = true,
                            User = user
                        };
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Login failed: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during login. Please try again."
                };
            }
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.PostAsJsonAsync("api/auth/register", request);

                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (authResponse?.Success == true && authResponse.User != null)
                    {
                        //await _authStore.SetAsync(UserSessionKey, authResponse.User);
                        return authResponse;
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Registration failed: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during registration. Please try again."
                };
            }
        }

        public async Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.PostAsJsonAsync("api/auth/forgot-password", request);

                if (response.IsSuccessStatusCode)
                {
                    return new AuthResponse
                    {
                        Success = true,
                        Message = "Password reset instructions have been sent to your email."
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Password reset failed: {errorContent}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during password reset. Please try again."
                };
            }
        }

        public async Task LogoutAsync()
        {
            _authStore.CurrentUser = null;
            await Task.CompletedTask;
        }

        public Task<UserInfo?> GetCurrentUserAsync()
        {
            return Task.FromResult(_authStore.CurrentUser);
        }
    }
}