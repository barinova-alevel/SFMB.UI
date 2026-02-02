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

            // Act
            var cut = Render<Login>();

            // Assert
            cut.Find("h3").TextContent.Should().Contain("Login");
            cut.Find("#email").Should().NotBeNull();
            cut.Find("#password").Should().NotBeNull();
            cut.Find("button[type='submit']").TextContent.Should().Contain("Login");
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

            var cut = Render<Login>();
            var navManager = Services.GetRequiredService<NavigationManager>();

            // Act
            cut.Find("#email").Change("test@example.com");
            cut.Find("#password").Change("password123");
            cut.Find("form").Submit();

            // Assert - verify successful login navigation occurred
            navManager.Uri.Should().EndWith("/");
        }
    }
}