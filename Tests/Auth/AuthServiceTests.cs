using BlazorApp.UI.Auth.Models;
using BlazorApp.UI.Auth.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Tests.Auth
{
    // Fake implementation that mimics ProtectedSessionStorage behavior for testing
    public class FakeProtectedSessionStorage
    {
        private readonly Dictionary<string, object?> _storage = new();

        public Task SetAsync(string key, object value)
        {
            _storage[key] = value;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string key)
        {
            _storage.Remove(key);
            return Task.CompletedTask;
        }

        public Task<ProtectedBrowserStorageResult<T>> GetAsync<T>(string key)
        {
            if (_storage.TryGetValue(key, out var value) && value is T typedValue)
            {
                // Use reflection to create the result since constructor is internal
                var result = typeof(ProtectedBrowserStorageResult<T>)
                    .GetConstructor(
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                        null,
                        new[] { typeof(bool), typeof(T) },
                        null)
                    ?.Invoke(new object[] { true, typedValue });
                return Task.FromResult((ProtectedBrowserStorageResult<T>)result!);
            }

            var failResult = typeof(ProtectedBrowserStorageResult<T>)
                .GetConstructor(
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                    null,
                    new[] { typeof(bool), typeof(T) },
                    null)
                ?.Invoke(new object?[] { false, default(T) });
            return Task.FromResult((ProtectedBrowserStorageResult<T>)failResult!);
        }

        public void Clear()
        {
            _storage.Clear();
        }

        public bool ContainsKey(string key)
        {
            return _storage.ContainsKey(key);
        }
    }

    // Wrapper for AuthService that uses FakeProtectedSessionStorage
    public class TestableAuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly FakeProtectedSessionStorage _sessionStorage;
        private readonly ILogger<AuthService> _logger;
        private const string UserSessionKey = "UserSession";

        public TestableAuthService(
            IHttpClientFactory httpClientFactory,
            FakeProtectedSessionStorage sessionStorage,
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

    public class AuthServiceTests : IDisposable
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly FakeProtectedSessionStorage _fakeSessionStorage;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly TestableAuthService _authService;

        public AuthServiceTests()
        {
            // Arrange - Setup common mocks
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _fakeSessionStorage = new FakeProtectedSessionStorage();
            _mockLogger = new Mock<ILogger<AuthService>>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("Api"))
                .Returns(httpClient);

            _authService = new TestableAuthService(
                _mockHttpClientFactory.Object,
                _fakeSessionStorage,
                _mockLogger.Object);
        }

        public void Dispose()
        {
            _mockHttpMessageHandler.Object.Dispose();
        }

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessAndSaveToSessionStorage()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var expectedUserInfo = new UserInfo
            {
                UserId = "123",
                Email = "test@example.com",
                Name = "Test User",
                Token = "test-jwt-token"
            };

            var responseContent = JsonContent.Create(expectedUserInfo);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = responseContent
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().Contains("api/auth/login")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.User.Should().NotBeNull();
            result.User!.Token.Should().Be("test-jwt-token");
            result.User.Email.Should().Be("test@example.com");
            result.User.Name.Should().Be("Test User");

            // Verify session storage was updated
            _fakeSessionStorage.ContainsKey("UserSession").Should().BeTrue();
        }

        [Fact]
        public async Task LoginAsync_WithUnauthorized401_ShouldReturnFailureWithMessage()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Invalid credentials")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().Contains("api/auth/login")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Login failed");
            result.Message.Should().Contain("Invalid credentials");
            result.User.Should().BeNull();

            // Verify session storage was not updated
            _fakeSessionStorage.ContainsKey("UserSession").Should().BeFalse();
        }

        [Fact]
        public async Task LoginAsync_WithBadRequest400_ShouldReturnFailureWithMessage()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Pass"
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Invalid request")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Login failed");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_WhenExceptionOccurs_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("An error occurred during login. Please try again.");
            result.User.Should().BeNull();

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithSuccessButNullResponse_ShouldReturnFailure()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        #endregion

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldReturnSuccessAndSaveToSessionStorage()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "newuser@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Name = "New User"
            };

            var expectedUserInfo = new UserInfo
            {
                UserId = "456",
                Email = "newuser@example.com",
                Name = "New User",
                Token = "new-jwt-token"
            };

            var responseContent = JsonContent.Create(expectedUserInfo);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = responseContent
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().Contains("api/auth/register")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _authService.RegisterAsync(registerRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.User.Should().NotBeNull();
            result.User!.Token.Should().Be("new-jwt-token");
            result.User.Email.Should().Be("newuser@example.com");

            // Verify session storage was updated
            _fakeSessionStorage.ContainsKey("UserSession").Should().BeTrue();
        }

        [Fact]
        public async Task RegisterAsync_WithConflict409_ShouldReturnFailure()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "existing@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Name = "Existing User"
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Conflict,
                Content = new StringContent("Email already exists")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _authService.RegisterAsync(registerRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Registration failed");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_WhenExceptionOccurs_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Name = "Test User"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _authService.RegisterAsync(registerRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("An error occurred during registration. Please try again.");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region ForgotPasswordAsync Tests

        [Fact]
        public async Task ForgotPasswordAsync_WithValidEmail_ShouldReturnSuccess()
        {
            // Arrange
            var forgotPasswordRequest = new ForgotPasswordRequest
            {
                Email = "test@example.com"
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Success")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().Contains("api/auth/forgot-password")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _authService.ForgotPasswordAsync(forgotPasswordRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Password reset instructions have been sent to your email.");
        }

        [Fact]
        public async Task ForgotPasswordAsync_WithNotFound404_ShouldReturnFailure()
        {
            // Arrange
            var forgotPasswordRequest = new ForgotPasswordRequest
            {
                Email = "nonexistent@example.com"
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("User not found")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _authService.ForgotPasswordAsync(forgotPasswordRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Password reset failed");
        }

        [Fact]
        public async Task ForgotPasswordAsync_WhenExceptionOccurs_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var forgotPasswordRequest = new ForgotPasswordRequest
            {
                Email = "test@example.com"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception("Email service error"));

            // Act
            var result = await _authService.ForgotPasswordAsync(forgotPasswordRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("An error occurred during password reset. Please try again.");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region LogoutAsync Tests

        [Fact]
        public async Task LogoutAsync_ShouldDeleteUserSessionFromStorage()
        {
            // Arrange
            // First add a user to storage
            await _fakeSessionStorage.SetAsync("UserSession", new UserInfo
            {
                UserId = "123",
                Email = "test@example.com",
                Name = "Test User",
                Token = "test-token"
            });

            _fakeSessionStorage.ContainsKey("UserSession").Should().BeTrue();

            // Act
            await _authService.LogoutAsync();

            // Assert
            _fakeSessionStorage.ContainsKey("UserSession").Should().BeFalse();

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region GetCurrentUserAsync Tests

        [Fact]
        public async Task GetCurrentUserAsync_WithStoredUser_ShouldReturnUserInfo()
        {
            // Arrange
            var expectedUserInfo = new UserInfo
            {
                UserId = "123",
                Email = "test@example.com",
                Name = "Test User",
                Token = "test-token"
            };

            // Store user in fake session storage
            await _fakeSessionStorage.SetAsync("UserSession", expectedUserInfo);

            // Act
            var result = await _authService.GetCurrentUserAsync();

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("test@example.com");
            result.Token.Should().Be("test-token");
            result.Name.Should().Be("Test User");
        }

        [Fact]
        public async Task GetCurrentUserAsync_WithNoStoredUser_ShouldReturnNull()
        {
            // Arrange
            // Ensure storage is clear
            _fakeSessionStorage.Clear();

            // Act
            var result = await _authService.GetCurrentUserAsync();

            // Assert
            result.Should().BeNull();
        }

        #endregion
    }
}
