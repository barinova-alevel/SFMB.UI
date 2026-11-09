using BlazorApp.UI.Auth;
using BlazorApp.UI.Auth.Models;
using BlazorApp.UI.Auth.Services;
using BlazorApp.UI.Components.Pages;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Tests.Pages
{
    public class LoginTests : BunitContext
    {
        [Fact]
        public void Render_ShouldShowLoginHeaderAndFields()
        {
            // Arrange
            Services.AddSingleton(Substitute.For<IAuthService>());
            Services.AddSingleton(Substitute.For<AuthenticationStateProvider>());
            Services.AddSingleton(TimeProvider.System);

            // Act
            var cut = Render<Login>();

            // Assert
            cut.Find("h3").TextContent.Should().Contain("Login");
            cut.Find("#email").Should().NotBeNull();
            cut.Find("#password").Should().NotBeNull();
            cut.Find("button[type='submit']").TextContent.Should().Contain("Login");

            var demo = cut.Find("#login-demo");
            demo.TextContent.Should().Contain("Login with demo user");
            cut.Markup.Should().Contain("No account needed");
            cut.Markup.IndexOf("id=\"login-demo\"", StringComparison.Ordinal)
                .Should().BeLessThan(cut.Markup.IndexOf("id=\"email\"", StringComparison.Ordinal));
        }

        [Fact]
        public void Submit_WhenAuthServiceReturnsSuccess_ShouldNavigateToHome()
        {
            // Arrange
            var authService = Substitute.For<IAuthService>();
            var authStateProvider = Substitute.For<AuthenticationStateProvider>();
            
            // Configure mock after creating navigation to avoid arg spec conflicts
            authService.LoginAsync(Arg.Any<LoginRequest>())
                .Returns(Task.FromResult(new AuthResponse { Success = true, User = new UserInfo { Email = "test@example.com" } }));

            Services.AddSingleton(authService);
            Services.AddSingleton(authStateProvider);
            Services.AddSingleton(TimeProvider.System);

            var cut = Render<Login>();
            var navManager = Services.GetRequiredService<NavigationManager>();

            // Act
            cut.Find("#email").Change("test@example.com");
            cut.Find("#password").Change("password123");
            cut.Find("form").Submit();

            // Assert
            authService.Received(1).LoginAsync(Arg.Is<LoginRequest>(r =>
                r.Email == "test@example.com" && r.Password == "password123"));

            navManager.Uri.Should().EndWith("/");
        }

        [Fact]
        public void Submit_WhenAuthServiceReturnsFailure_ShouldShowErrorMessage()
        {
            // Arrange
            var authService = Substitute.For<IAuthService>();
            var authStateProvider = Substitute.For<AuthenticationStateProvider>();
            
            // Configure mock after creating navigation to avoid arg spec conflicts
            authService.LoginAsync(Arg.Any<LoginRequest>())
                .Returns(Task.FromResult(new AuthResponse { Success = false, Message = "Invalid credentials" }));

            Services.AddSingleton(authService);
            Services.AddSingleton(authStateProvider);
            Services.AddSingleton(TimeProvider.System);

            var cut = Render<Login>();

            // Act
            cut.Find("#email").Change("bad@example.com");
            cut.Find("#password").Change("wrongpass");
            cut.Find("form").Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Find(".alert.alert-danger").TextContent.Should().Contain("Invalid credentials");
            });
        }

        [Fact]
        public void Submit_WhenAuthServiceThrows_ShouldShowUnexpectedErrorMessage()
        {
            // Arrange
            var authService = Substitute.For<IAuthService>();
            var authStateProvider = Substitute.For<AuthenticationStateProvider>();
            
            // Configure mock after creating navigation to avoid arg spec conflicts
            authService.LoginAsync(Arg.Any<LoginRequest>())
                .ThrowsAsync(new InvalidOperationException("boom"));

            Services.AddSingleton(authService);
            Services.AddSingleton(authStateProvider);
            Services.AddSingleton(TimeProvider.System);

            var cut = Render<Login>();

            // Act
            cut.Find("#email").Change("test@example.com");
            cut.Find("#password").Change("password123");
            cut.Find("form").Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Find(".alert.alert-danger").TextContent.Should().Contain("An unexpected error occurred");
            });
        }

        [Fact]
        public void ErrorAlert_WhenCloseClicked_ShouldHideError()
        {
            // Arrange
            var authService = Substitute.For<IAuthService>();
            var authStateProvider = Substitute.For<AuthenticationStateProvider>();
            
            // Configure mock after creating navigation to avoid arg spec conflicts
            authService.LoginAsync(Arg.Any<LoginRequest>())
                .Returns(Task.FromResult(new AuthResponse { Success = false, Message = "Invalid credentials" }));

            Services.AddSingleton(authService);
            Services.AddSingleton(authStateProvider);
            Services.AddSingleton(TimeProvider.System);

            var cut = Render<Login>();

            cut.Find("#email").Change("bad@example.com");
            cut.Find("#password").Change("wrongpass");
            cut.Find("form").Submit();

            cut.WaitForElement(".alert.alert-danger");

            // Act
            cut.Find(".alert.alert-danger .btn-close").Click();

            // Assert
            cut.FindAll(".alert.alert-danger").Count.Should().Be(0);
        }

        [Fact]
        public void Submit_WhenAuthStateProviderIsCustom_ShouldNavigateToHome()
        {
            // Arrange
            var authService = Substitute.For<IAuthService>();
            // Use a simple mock AuthenticationStateProvider - we're testing the Login component behavior
            var authStateProvider = Substitute.For<AuthenticationStateProvider>();
            authStateProvider.GetAuthenticationStateAsync().Returns(Task.FromResult(
                new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity()))));
            
            // Configure mock
            authService.LoginAsync(Arg.Any<LoginRequest>())
                .Returns(Task.FromResult(new AuthResponse { Success = true }));

            Services.AddSingleton(authService);
            Services.AddSingleton(authStateProvider);
            Services.AddSingleton(TimeProvider.System);

            var cut = Render<Login>();
            var navManager = Services.GetRequiredService<NavigationManager>();

            // Act
            cut.Find("#email").Change("test@example.com");
            cut.Find("#password").Change("password123");
            cut.Find("form").Submit();

            // Assert - verify successful login navigation occurred
            navManager.Uri.Should().EndWith("/");
        }

        [Fact]
        public async Task DemoLogin_FillsCredentials_Pauses_ThenSignsIn()
        {
            var authService = Substitute.For<IAuthService>();
            var authStateProvider = Substitute.For<AuthenticationStateProvider>();
            var clock = new PausingTimeProvider();

            authService.GetDemoCredentialsAsync()
                .Returns(new DemoCredentials("demo@goldfish.app", "secret1"));
            authService.LoginAsync(Arg.Any<LoginRequest>())
                .Returns(Task.FromResult(new AuthResponse
                {
                    Success = true,
                    User = new UserInfo { Email = "demo@goldfish.app" }
                }));

            Services.AddSingleton(authService);
            Services.AddSingleton(authStateProvider);
            Services.AddSingleton<TimeProvider>(clock);

            var cut = Render<Login>();
            var navManager = Services.GetRequiredService<NavigationManager>();

            var click = cut.Find("#login-demo").ClickAsync();

            cut.WaitForAssertion(() =>
            {
                cut.Find("#email").GetAttribute("value").Should().Be("demo@goldfish.app");
                cut.Find("#password").GetAttribute("value").Should().Be("secret1");
                cut.Find("#login-demo-status").TextContent.Should().Contain(DemoLogin.CredentialsLoadedStatus);
                clock.DueTime.Should().Be(TimeSpan.FromMilliseconds(DemoLogin.DelayMilliseconds));
            });

            await authService.DidNotReceive().LoginAsync(Arg.Any<LoginRequest>());

            clock.Release();

            cut.WaitForAssertion(() => navManager.Uri.Should().EndWith("/"));
            await authService.Received(1).LoginAsync(Arg.Is<LoginRequest>(request =>
                request.Email == "demo@goldfish.app" && request.Password == "secret1"));
            await click;
        }

        [Fact]
        public void DemoLogin_WhenCredentialsAreMissing_ShowsNotConfiguredAndDoesNotSubmit()
        {
            var authService = Substitute.For<IAuthService>();
            var authStateProvider = Substitute.For<AuthenticationStateProvider>();

            authService.GetDemoCredentialsAsync().Returns((DemoCredentials?)null);

            Services.AddSingleton(authService);
            Services.AddSingleton(authStateProvider);
            Services.AddSingleton(TimeProvider.System);

            var cut = Render<Login>();

            cut.Find("#login-demo").Click();

            cut.WaitForAssertion(() =>
            {
                cut.Find(".alert.alert-danger").TextContent.Should().Contain(DemoLogin.NotConfiguredMessage);
            });

            authService.DidNotReceive().LoginAsync(Arg.Any<LoginRequest>());
            cut.Find("#email").GetAttribute("value").Should().BeNullOrEmpty();
            cut.FindAll("#login-demo-status").Count.Should().Be(0);
        }

        private sealed class PausingTimeProvider : TimeProvider
        {
            private TimerCallback? _callback;
            private object? _state;

            public TimeSpan? DueTime { get; private set; }

            public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
            {
                _callback = callback;
                _state = state;
                DueTime = dueTime;
                return new NoopTimer();
            }

            public void Release() => _callback?.Invoke(_state);

            private sealed class NoopTimer : ITimer
            {
                public bool Change(TimeSpan dueTime, TimeSpan period) => true;

                public void Dispose()
                {
                }

                public ValueTask DisposeAsync() => ValueTask.CompletedTask;
            }
        }
    }
}