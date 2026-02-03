using BlazorApp.UI.Auth.Models;
using BlazorApp.UI.Auth.Services;
using BlazorApp.UI.Components.Pages;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Tests.Pages
{
    public class RegisterTests : BunitContext
    {
        [Fact]
        public void Render_ShouldShowSignUpHeaderAndFields()
        {
            // Arrange
            Services.AddSingleton(Substitute.For<IAuthService>());
            Services.AddSingleton(Substitute.For<AuthenticationStateProvider>());

            // Act
            var cut = Render<Register>();

            // Assert
            cut.Find("h3").TextContent.Should().Contain("Sign Up");
            cut.Find("#name").Should().NotBeNull();
            cut.Find("#email").Should().NotBeNull();
            cut.Find("#password").Should().NotBeNull();
            cut.Find("#confirmPassword").Should().NotBeNull();
            cut.Find("button[type='submit']").TextContent.Should().Contain("Create Account");
        }

        [Fact]
        public void Submit_WhenAuthServiceReturnsSuccess_ShouldNavigateToHome()
        {
            // Arrange
            var authService = Substitute.For<IAuthService>();
            authService.RegisterAsync(Arg.Any<RegisterRequest>())
                .Returns(Task.FromResult(new AuthResponse
                {
                    Success = true,
                    User = new UserInfo { Email = "newuser@example.com" }
                }));

            Services.AddSingleton(authService);
            Services.AddSingleton(Substitute.For<AuthenticationStateProvider>());

            var nav = Services.GetRequiredService<NavigationManager>();

            var cut = Render<Register>();

            // Act
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#name"), "New User");
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#email"), "newuser@example.com");
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#password"), "Password1!");
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#confirmPassword"), "Password1!");
            Bunit.EventHandlerDispatchExtensions.Submit(cut.Find("form"));

            // Assert
            authService.Received(1).RegisterAsync(Arg.Is<RegisterRequest>(r =>
                r.Name == "New User" &&
                r.Email == "newuser@example.com" &&
                r.Password == "Password1!" &&
                r.ConfirmPassword == "Password1!"));

            nav.Uri.Should().EndWith("/");
        }

        [Fact]
        public void Submit_WhenAuthServiceReturnsFailure_ShouldShowErrorMessage()
        {
            // Arrange
            var authService = Substitute.For<IAuthService>();
            authService.RegisterAsync(Arg.Any<RegisterRequest>())
                .Returns(Task.FromResult(new AuthResponse { Success = false, Message = "Email already exists" }));

            Services.AddSingleton(authService);
            Services.AddSingleton(Substitute.For<AuthenticationStateProvider>());

            var cut = Render<Register>();

            // Act
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#name"), "New User");
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#email"), "existing@example.com");
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#password"), "Password1!");
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#confirmPassword"), "Password1!");
            Bunit.EventHandlerDispatchExtensions.Submit(cut.Find("form"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Find(".alert.alert-danger").TextContent.Should().Contain("Email already exists");
            });
        }

        [Fact]
        public void Submit_WhenAuthServiceThrows_ShouldShowUnexpectedErrorMessage()
        {
            // Arrange
            var authService = Substitute.For<IAuthService>();
            authService.RegisterAsync(Arg.Any<RegisterRequest>())
                .Returns(Task.FromException<AuthResponse>(new InvalidOperationException("boom")));

            Services.AddSingleton(authService);
            Services.AddSingleton(Substitute.For<AuthenticationStateProvider>());

            var cut = Render<Register>();

            // Act
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#name"), "New User");
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#email"), "newuser@example.com");
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#password"), "Password1!");
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#confirmPassword"), "Password1!");
            Bunit.EventHandlerDispatchExtensions.Submit(cut.Find("form"));

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
            authService.RegisterAsync(Arg.Any<RegisterRequest>())
                .Returns(Task.FromResult(new AuthResponse { Success = false, Message = "Email already exists" }));

            Services.AddSingleton(authService);
            Services.AddSingleton(Substitute.For<AuthenticationStateProvider>());

            var cut = Render<Register>();

            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#name"), "New User");
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#email"), "existing@example.com");
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#password"), "Password1!");
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#confirmPassword"), "Password1!");
            Bunit.EventHandlerDispatchExtensions.Submit(cut.Find("form"));

            cut.WaitForElement(".alert.alert-danger");

            // Act
            cut.Find(".alert.alert-danger .btn-close").Click();

            // Assert
            cut.FindAll(".alert.alert-danger").Count.Should().Be(0);
        }
    }
}
