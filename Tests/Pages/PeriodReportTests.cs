using System.Net;
using System.Text;
using System.Text.Json;
using BlazorApp.UI.Components.Pages;
using BlazorApp.UI.Dtos;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NSubstitute;

namespace Tests.Pages
{
    public class PeriodReportTests : BunitContext
    {
        [Fact]
        public void Render_ShouldShowPageHeader()
        {
            // Arrange
            Services.AddSingleton(Substitute.For<IHttpClientFactory>());
            Services.AddSingleton(Substitute.For<IJSRuntime>());

            // Act
            var cut = Render<PeriodReport>();

            // Assert
            cut.Find("h3").TextContent.Should().Be("Period Report");
            cut.Markup.Should().Contain("Generate financial reports for any date period");
        }

        [Fact]
        public void Render_ShouldShowStartAndEndDateInputsAndGenerateButton()
        {
            // Arrange
            Services.AddSingleton(Substitute.For<IHttpClientFactory>());
            Services.AddSingleton(Substitute.For<IJSRuntime>());

            // Act
            var cut = Render<PeriodReport>();

            // Assert
            cut.Find("#startDate").Should().NotBeNull();
            cut.Find("#endDate").Should().NotBeNull();
            cut.Find("button[type='submit']").TextContent.Should().Contain("Generate Report");
        }


        private sealed class MockHttpMessageHandler : HttpMessageHandler
        {
            private HttpStatusCode _statusCode = HttpStatusCode.OK;
            private object? _responseContent;

            public void SetResponse(HttpStatusCode statusCode, object? content)
            {
                _statusCode = statusCode;
                _responseContent = content;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(_statusCode);

                if (_responseContent is null)
                {
                    response.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                    return Task.FromResult(response);
                }

                if (_responseContent is string stringContent)
                {
                    response.Content = new StringContent(stringContent, Encoding.UTF8, "application/json");
                    return Task.FromResult(response);
                }

                var json = JsonSerializer.Serialize(_responseContent);
                response.Content = new StringContent(json, Encoding.UTF8, "application/json");
                return Task.FromResult(response);
            }
        }
    }
}
