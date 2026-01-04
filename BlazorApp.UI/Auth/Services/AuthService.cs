using BlazorApp.UI.Auth.Models;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BlazorApp.UI.Auth.Services
{
    public class CircuitServicesAccessor
    {
        static readonly AsyncLocal<IServiceProvider> blazorServices = new();

        public IServiceProvider? Services
        {
            get => blazorServices.Value;
            set => blazorServices.Value = value!;
        }
    }

    public class ServicesAccessorCircuitHandler(
        IServiceProvider services, CircuitServicesAccessor servicesAccessor)
        : CircuitHandler
    {
        public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
            Func<CircuitInboundActivityContext, Task> next) =>
                async context =>
                {
                    servicesAccessor.Services = services;
                    await next(context);
                    servicesAccessor.Services = null;
                };
    }

    public static class CircuitServicesServiceCollectionExtensions
    {
        public static IServiceCollection AddCircuitServicesAccessor(
            this IServiceCollection services)
        {
            services.AddScoped<CircuitServicesAccessor>();
            services.AddScoped<CircuitHandler, ServicesAccessorCircuitHandler>();

            return services;
        }
    }
    public class TokenProviderMessageHandler : DelegatingHandler
    {
        private readonly CircuitServicesAccessor _serviceProvider;
        private const string UserSessionKey = "UserSession";
        public TokenProviderMessageHandler(CircuitServicesAccessor serviceProvider) : base(new HttpClientHandler())
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var result = await _serviceProvider.Services.GetRequiredService<ProtectedSessionStorage>().GetAsync<UserInfo>(UserSessionKey);
            if (result.Success && result.Value != null)
            {
                var user = result.Value;
                if (!string.IsNullOrEmpty(user.Token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);
                }
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }

    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly ILogger<AuthService> _logger;
        private const string UserSessionKey = "UserSession";

        public AuthService(
            IHttpClientFactory httpClientFactory,
            ProtectedSessionStorage sessionStorage,
            ILogger<AuthService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _sessionStorage = sessionStorage;
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
                    var authResponse = await response.Content.ReadFromJsonAsync<UserInfo>();
                    if (authResponse != null)
                    {
                        await _sessionStorage.SetAsync(UserSessionKey, authResponse);
                        return new AuthResponse { User = authResponse, Success = true };
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
                    var authResponse = await response.Content.ReadFromJsonAsync<UserInfo>();
                    if (authResponse != null)
                    {
                        await _sessionStorage.SetAsync(UserSessionKey, authResponse);
                        return new AuthResponse { User = authResponse, Success = true};
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
            await _sessionStorage.DeleteAsync(UserSessionKey);
        }

        public async Task<UserInfo?> GetCurrentUserAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<UserInfo>(UserSessionKey);
                return result.Success ? result.Value : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
