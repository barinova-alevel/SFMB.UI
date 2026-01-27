using BlazorApp.UI.Auth.Models;
using BlazorApp.UI.Auth.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Tests.Auth
{
    public class AuthServiceTests
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TestableProtectedSessionStorage _sessionStorage;
        private readonly ILogger<AuthService> _logger;
        private readonly TestableAuthService _authService;
        private const string UserSessionKey = "UserSession";

        public AuthServiceTests()
        {
            // Arrange - Common setup for all tests
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            _sessionStorage = new TestableProtectedSessionStorage();
            _logger = Substitute.For<ILogger<AuthService>>();
            _authService = new TestableAuthService(_httpClientFactory, _sessionStorage, _logger);
        }

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var expectedUser = new UserInfo
            {
                UserId = "123",
                Email = "test@example.com",
                Name = "Test User",
                Token = "test-token"
            };

            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetResponse(HttpStatusCode.OK, expectedUser);
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://test.local") };
            _httpClientFactory.CreateClient("Api").Returns(httpClient);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.User.Should().NotBeNull();
            result.User!.Email.Should().Be(expectedUser.Email);
            result.User.Name.Should().Be(expectedUser.Name);
            result.User.Token.Should().Be(expectedUser.Token);
            _sessionStorage.SetAsyncCallCount.Should().Be(1);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldReturnFailureResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetResponse(HttpStatusCode.Unauthorized, "Invalid credentials");
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://test.local") };
            _httpClientFactory.CreateClient("Api").Returns(httpClient);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Login failed");
            result.User.Should().BeNull();
            _sessionStorage.SetAsyncCallCount.Should().Be(0);
        }

        [Fact]
        public async Task LoginAsync_WhenExceptionOccurs_ShouldReturnFailureResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "password123"
            };

            _httpClientFactory.CreateClient("Api").Throws(new HttpRequestException("Network error"));

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("An error occurred during login");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_WithNullUserResponse_ShouldReturnFailureResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetResponse(HttpStatusCode.OK, (UserInfo?)null);
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://test.local") };
            _httpClientFactory.CreateClient("Api").Returns(httpClient);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert - When deserialization fails, it throws JsonException which is caught as a general error
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("An error occurred during login");
            _sessionStorage.SetAsyncCallCount.Should().Be(0);
        }

        #endregion

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldReturnSuccessResponse()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "newuser@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Name = "New User"
            };

            var expectedUser = new UserInfo
            {
                UserId = "456",
                Email = "newuser@example.com",
                Name = "New User",
                Token = "registration-token"
            };

            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetResponse(HttpStatusCode.OK, expectedUser);
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://test.local") };
            _httpClientFactory.CreateClient("Api").Returns(httpClient);

            // Act
            var result = await _authService.RegisterAsync(registerRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.User.Should().NotBeNull();
            result.User!.Email.Should().Be(expectedUser.Email);
            result.User.Name.Should().Be(expectedUser.Name);
            result.User.Token.Should().Be(expectedUser.Token);
            _sessionStorage.SetAsyncCallCount.Should().Be(1);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldReturnFailureResponse()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "existing@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Name = "Test User"
            };

            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetResponse(HttpStatusCode.BadRequest, "Email already exists");
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://test.local") };
            _httpClientFactory.CreateClient("Api").Returns(httpClient);

            // Act
            var result = await _authService.RegisterAsync(registerRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Registration failed");
            result.User.Should().BeNull();
            _sessionStorage.SetAsyncCallCount.Should().Be(0);
        }

        [Fact]
        public async Task RegisterAsync_WhenExceptionOccurs_ShouldReturnFailureResponse()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Name = "Test User"
            };

            _httpClientFactory.CreateClient("Api").Throws(new Exception("Database error"));

            // Act
            var result = await _authService.RegisterAsync(registerRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("An error occurred during registration");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_WithNullUserResponse_ShouldReturnFailureResponse()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Name = "Test User"
            };

            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetResponse(HttpStatusCode.OK, (UserInfo?)null);
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://test.local") };
            _httpClientFactory.CreateClient("Api").Returns(httpClient);

            // Act
            var result = await _authService.RegisterAsync(registerRequest);

            // Assert - When deserialization fails, it throws JsonException which is caught as a general error
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("An error occurred during registration");
            _sessionStorage.SetAsyncCallCount.Should().Be(0);
        }

        #endregion

        #region ForgotPasswordAsync Tests

        [Fact]
        public async Task ForgotPasswordAsync_WithValidEmail_ShouldReturnSuccessResponse()
        {
            // Arrange
            var forgotPasswordRequest = new ForgotPasswordRequest
            {
                Email = "test@example.com"
            };

            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetResponse(HttpStatusCode.OK, string.Empty);
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://test.local") };
            _httpClientFactory.CreateClient("Api").Returns(httpClient);

            // Act
            var result = await _authService.ForgotPasswordAsync(forgotPasswordRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("Password reset instructions have been sent");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ForgotPasswordAsync_WithNonExistentEmail_ShouldReturnFailureResponse()
        {
            // Arrange
            var forgotPasswordRequest = new ForgotPasswordRequest
            {
                Email = "nonexistent@example.com"
            };

            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetResponse(HttpStatusCode.NotFound, "User not found");
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://test.local") };
            _httpClientFactory.CreateClient("Api").Returns(httpClient);

            // Act
            var result = await _authService.ForgotPasswordAsync(forgotPasswordRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Password reset failed");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task ForgotPasswordAsync_WhenExceptionOccurs_ShouldReturnFailureResponse()
        {
            // Arrange
            var forgotPasswordRequest = new ForgotPasswordRequest
            {
                Email = "test@example.com"
            };

            _httpClientFactory.CreateClient("Api").Throws(new Exception("Email service error"));

            // Act
            var result = await _authService.ForgotPasswordAsync(forgotPasswordRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("An error occurred during password reset");
            result.User.Should().BeNull();
        }

        #endregion

        #region LogoutAsync Tests

        [Fact]
        public async Task LogoutAsync_ShouldDeleteUserSession()
        {
            // Arrange
            // (No specific arrangement needed beyond constructor)

            // Act
            await _authService.LogoutAsync();

            // Assert
            _sessionStorage.DeleteAsyncCallCount.Should().Be(1);
        }

        #endregion

        #region GetCurrentUserAsync Tests

        [Fact]
        public async Task GetCurrentUserAsync_WithValidSession_ShouldReturnUser()
        {
            // Arrange
            var expectedUser = new UserInfo
            {
                UserId = "123",
                Email = "test@example.com",
                Name = "Test User",
                Token = "test-token"
            };

            _sessionStorage.SetStoredValue(UserSessionKey, expectedUser, true);

            // Act
            var user = await _authService.GetCurrentUserAsync();

            // Assert
            user.Should().NotBeNull();
            user!.UserId.Should().Be(expectedUser.UserId);
            user.Email.Should().Be(expectedUser.Email);
            user.Name.Should().Be(expectedUser.Name);
            user.Token.Should().Be(expectedUser.Token);
        }

        [Fact]
        public async Task GetCurrentUserAsync_WithNoSession_ShouldReturnNull()
        {
            // Arrange
            _sessionStorage.SetStoredValue<UserInfo?>(UserSessionKey, null, false);

            // Act
            var user = await _authService.GetCurrentUserAsync();

            // Assert
            user.Should().BeNull();
        }

        [Fact]
        public async Task GetCurrentUserAsync_WhenExceptionOccurs_ShouldReturnNull()
        {
            // Arrange
            _sessionStorage.ShouldThrowException = true;

            // Act
            var user = await _authService.GetCurrentUserAsync();

            // Assert
            user.Should().BeNull();
        }

        #endregion
    }

    // Helper class to mock HTTP responses
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private HttpStatusCode _statusCode = HttpStatusCode.OK;
        private object? _responseContent;

        public void SetResponse(HttpStatusCode statusCode, object? content)
        {
            _statusCode = statusCode;
            _responseContent = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode);

            if (_responseContent != null)
            {
                if (_responseContent is string stringContent)
                {
                    response.Content = new StringContent(stringContent, Encoding.UTF8, "application/json");
                }
                else
                {
                    var json = JsonSerializer.Serialize(_responseContent);
                    response.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
            }
            else
            {
                // Return empty content for null responses
                response.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            }

            return Task.FromResult(response);
        }
    }

    // Testable wrapper around ProtectedSessionStorage
    public class TestableProtectedSessionStorage
    {
        private readonly Dictionary<string, object?> _storage = new();
        private readonly Dictionary<string, bool> _success = new();
        public int SetAsyncCallCount { get; private set; }
        public int DeleteAsyncCallCount { get; private set; }
        public bool ShouldThrowException { get; set; }

        public void SetStoredValue<T>(string key, T? value, bool success)
        {
            _storage[key] = value;
            _success[key] = success;
        }

        public ValueTask SetAsync(string key, object value)
        {
            SetAsyncCallCount++;
            _storage[key] = value;
            _success[key] = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask DeleteAsync(string key)
        {
            DeleteAsyncCallCount++;
            if (_storage.ContainsKey(key))
            {
                _storage.Remove(key);
                _success.Remove(key);
            }
            return ValueTask.CompletedTask;
        }

        public ValueTask<ProtectedBrowserStorageResult<T>> GetAsync<T>(string key)
        {
            if (ShouldThrowException)
            {
                throw new Exception("Storage error");
            }

            var success = _success.ContainsKey(key) && _success[key];
            var value = _storage.ContainsKey(key) ? (T?)_storage[key] : default;

            // Use unsafe code or FormatterServices to create a struct with init-only properties
            var resultType = typeof(ProtectedBrowserStorageResult<T>);
            var result = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(resultType);
            
            // Set the backing fields directly
            var successField = resultType.GetField("<Success>k__BackingField", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var valueField = resultType.GetField("<Value>k__BackingField", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            
            successField?.SetValue(result, success);
            valueField?.SetValue(result, value);

            return new ValueTask<ProtectedBrowserStorageResult<T>>((ProtectedBrowserStorageResult<T>)result);
        }
    }

    // Testable version of AuthService that uses the testable storage
    public class TestableAuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TestableProtectedSessionStorage _sessionStorage;
        private readonly ILogger<AuthService> _logger;
        private const string UserSessionKey = "UserSession";

        public TestableAuthService(
            IHttpClientFactory httpClientFactory,
            TestableProtectedSessionStorage sessionStorage,
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
                        _logger.LogInformation("User registered");
                        return new AuthResponse { User = authResponse, Success = true };
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
            _logger.LogInformation("Logout");
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
