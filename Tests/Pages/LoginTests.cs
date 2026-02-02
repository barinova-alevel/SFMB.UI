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
            Services.AddSingleton(CreateFakeNavigationManager());

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
            authService.LoginAsync(Arg.Any<LoginRequest>())
                .Returns(Task.FromResult(new AuthResponse { Success = true, User = new UserInfo { Email = "test@example.com" } }));

            var authStateProvider = Substitute.For<AuthenticationStateProvider>();
            var navigation = CreateFakeNavigationManager();

            Services.AddSingleton(authService);
            Services.AddSingleton(authStateProvider);
            Services.AddSingleton(navigation);

            var cut = Render<Login>();

            // Act
            Bunit.InputEventDispatchExtensions.Change(cut.Find("#email"), "test@example.com");
            Bunit.InputEventDispatchExtensions.Change(cut.Find("#password"), "password123");
            //cut.FindComponent<EditForm>().Submit();
            var editForm = cut.FindComponent<EditForm>();
            editForm.Find("form").Submit();

            // Assert
            authService.Received(1).LoginAsync(Arg.Is<LoginRequest>(r =>
                r.Email == "test@example.com" && r.Password == "password123"));

            navigation.Uri.Should().EndWith("/");
        }

        [Fact]
        public void Submit_WhenAuthServiceReturnsFailure_ShouldShowErrorMessage()
        {
            // Arrange
            var authService = Substitute.For<IAuthService>();
            authService.LoginAsync(Arg.Any<LoginRequest>())
                .Returns(Task.FromResult(new AuthResponse { Success = false, Message = "Invalid credentials" }));

            Services.AddSingleton(authService);
            Services.AddSingleton(Substitute.For<AuthenticationStateProvider>());
            Services.AddSingleton(CreateFakeNavigationManager());

            var cut = Render<Login>();

            // Act
            Bunit.InputEventDispatchExtensions.Change(cut.Find("#email"), "bad@example.com");
            Bunit.InputEventDispatchExtensions.Change(cut.Find("#password"), "wrongpass");
            //cut.FindComponent<EditForm>().Submit();
            var editForm = cut.FindComponent<EditForm>();
            editForm.Find("form").Submit();

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
           // authService.LoginAsync(Arg.Any<LoginRequest>())
              //  .Returns(_ => throw new InvalidOperationException("boom"));

            

            authService.LoginAsync(Arg.Any<LoginRequest>())
                .ThrowsAsync(new InvalidOperationException("boom"));

            Services.AddSingleton(authService);
            Services.AddSingleton(Substitute.For<AuthenticationStateProvider>());
            Services.AddSingleton(CreateFakeNavigationManager());

            var cut = Render<Login>();

            // Act
            Bunit.InputEventDispatchExtensions.Change(cut.Find("#email"), "test@example.com");
            Bunit.InputEventDispatchExtensions.Change(cut.Find("#password"), "password123");
            //cut.FindComponent<EditForm>().Submit();
            var editForm = cut.FindComponent<EditForm>();
            editForm.Find("form").Submit();

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
            authService.LoginAsync(Arg.Any<LoginRequest>())
                .Returns(Task.FromResult(new AuthResponse { Success = false, Message = "Invalid credentials" }));

            Services.AddSingleton(authService);
            Services.AddSingleton(Substitute.For<AuthenticationStateProvider>());
            Services.AddSingleton(CreateFakeNavigationManager());

            var cut = Render<Login>();

            Bunit.InputEventDispatchExtensions.Change(cut.Find("#email"), "bad@example.com");
            Bunit.InputEventDispatchExtensions.Change(cut.Find("#password"), "wrongpass");
            //cut.FindComponent<EditForm>().Submit();
            var editForm = cut.FindComponent<EditForm>();
            editForm.Find("form").Submit();

            cut.WaitForElement(".alert.alert-danger");

            // Act
            //cut.Find(".alert.alert-danger .btn-close").Click();
            // Act
            Bunit.EventHandlerDispatchExtensions.Click(
                cut.Find(".alert.alert-danger .btn-close"),
                new Microsoft.AspNetCore.Components.Web.MouseEventArgs()
            );

            // Assert
            cut.FindAll(".alert.alert-danger").Count.Should().Be(0);
        }

        [Fact]
        public void Submit_WhenAuthStateProviderIsCustom_ShouldCallNotifyAuthenticationStateChanged()
        {
            // Arrange
            var authService = Substitute.For<IAuthService>();
            authService.LoginAsync(Arg.Any<LoginRequest>())
                .Returns(Task.FromResult(new AuthResponse { Success = true }));

            var customProvider = Substitute.For<CustomAuthenticationStateProvider>(
                Substitute.For<Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage.ProtectedSessionStorage>());

            Services.AddSingleton(authService);
            Services.AddSingleton<AuthenticationStateProvider>(customProvider);
            Services.AddSingleton(CreateFakeNavigationManager());

            var cut = Render<Login>();

            // Act
            Bunit.InputEventDispatchExtensions.Change(cut.Find("#email"), "test@example.com");
            Bunit.InputEventDispatchExtensions.Change(cut.Find("#password"), "password123");
            var editForm = cut.FindComponent<EditForm>();
            editForm.Find("form").Submit();
            //cut.FindComponent<EditForm>().Submit();

            // Assert
            customProvider.Received(1).NotifyAuthenticationStateChanged();
        }

        private static NavigationManager CreateFakeNavigationManager()
        {
            var nav = Substitute.For<NavigationManager>();
            nav.When(n => n.NavigateTo(Arg.Any<string>(), Arg.Any<bool>()))
                .Do(callInfo =>
                {
                    var uri = (string)callInfo[0];
                    nav.Uri.Returns(new Uri(new Uri("http://localhost"), uri).ToString());
                });

            nav.Uri.Returns("http://localhost/login");
            nav.BaseUri.Returns("http://localhost/");
            return nav;
        }
    }
}