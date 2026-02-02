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
    public class DailyReportTests : BunitContext
    {
        [Fact]
        public void Render_ShouldShowPageHeader()
        {
            // Arrange
            Services.AddSingleton(Substitute.For<IHttpClientFactory>());
            Services.AddSingleton(Substitute.For<IJSRuntime>());

            // Act
            var cut = Render<DailyReport>();

            // Assert
            cut.Find("h3").TextContent.Should().Be("Daily Report");
            cut.Markup.Should().Contain("Generate financial reports for a date");
        }

        [Fact]
        public void Render_ShouldShowDateInputAndGenerateButton()
        {
            // Arrange
            Services.AddSingleton(Substitute.For<IHttpClientFactory>());
            Services.AddSingleton(Substitute.For<IJSRuntime>());

            // Act
            var cut = Render<DailyReport>();

            // Assert
            cut.Find("#Date").Should().NotBeNull();
            cut.Find("button[type='submit']").TextContent.Should().Contain("Generate Report");
        }

        [Fact]
        public void Submit_WhenApiReturnsSuccess_ShouldRenderTotalsCards()
        {
            // Arrange
            var reportDate = new DateTime(2026, 01, 15);

            var dto = new DailyReportDtoBlazor
            {
                Date = DateOnly.FromDateTime(reportDate),
                TotalIncome = 1000m,
                TotalExpenses = 250m,
                Operations = new()
            };

            var handler = new MockHttpMessageHandler();
            handler.SetResponse(HttpStatusCode.OK, dto);

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test.local/") };
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient("Api").Returns(httpClient);

            Services.AddSingleton(httpClientFactory);
            Services.AddSingleton(Substitute.For<IJSRuntime>());

            var cut = Render<DailyReport>();

            // Act
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#Date"), reportDate.ToString("yyyy-MM-dd"));
            Bunit.EventHandlerDispatchExtensions.Submit(cut.Find("form"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Markup.Should().Contain("Total Income");
                cut.Markup.Should().Contain("Total Expenses");
            });
        }

        [Fact]
        public void Submit_WhenApiReturnsSuccessWithOperations_ShouldRenderOperationsCount()
        {
            // Arrange
            DateTime dateTime = new DateTime(2026, 01, 15);
            var reportDate = DateOnly.FromDateTime(dateTime);
            var dto = new DailyReportDtoBlazor
            {
                Date = reportDate,
                TotalIncome = 100m,
                TotalExpenses = 50m,
                Operations = new()
                {
                    new OperationDtoBlazor
                    {
                        OperationId = 1,
                        Date = reportDate,
                        Amount = 100m,
                        Note = "Salary",
                        OperationTypeId = 10,
                        OperationType = new OperationTypeDtoBlazor
                        {
                            OperationTypeId = 10,
                            Name = "Salary",
                            Description = "Monthly",
                            IsIncome = true
                        }
                    },
                    new OperationDtoBlazor
                    {
                        OperationId = 2,
                        Date = reportDate,
                        Amount = 50m,
                        Note = "Food",
                        OperationTypeId = 11,
                        OperationType = new OperationTypeDtoBlazor
                        {
                            OperationTypeId = 11,
                            Name = "Groceries",
                            Description = "Store",
                            IsIncome = false
                        }
                    }
                }
            };

            var handler = new MockHttpMessageHandler();
            handler.SetResponse(HttpStatusCode.OK, dto);

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test.local/") };
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient("Api").Returns(httpClient);

            Services.AddSingleton(httpClientFactory);
            Services.AddSingleton(Substitute.For<IJSRuntime>());

            var cut = Render<DailyReport>();

            // Act
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#Date"), reportDate.ToString("yyyy-MM-dd"));
            Bunit.EventHandlerDispatchExtensions.Submit(cut.Find("form"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Markup.Should().Contain("Operations (2)");
                cut.FindAll("table tbody tr").Count.Should().Be(2);
            });
        }

        [Fact]
        public void FilterIncome_WhenClicked_ShouldShowOnlyIncomeRows()
        {
            // Arrange
            DateTime dateTime = new DateTime(2026, 01, 15);
            var reportDate = DateOnly.FromDateTime(dateTime);

            var dto = new DailyReportDtoBlazor
            {
                Date = reportDate,
                TotalIncome = 100m,
                TotalExpenses = 50m,
                Operations = new()
                {
                    new OperationDtoBlazor
                    {
                        OperationId = 1,
                        Date = reportDate,
                        Amount = 100m,
                        Note = "Salary",
                        OperationTypeId = 10,
                        OperationType = new OperationTypeDtoBlazor
                        {
                            OperationTypeId = 10,
                            Name = "Salary",
                            IsIncome = true
                        }
                    },
                    new OperationDtoBlazor
                    {
                        OperationId = 2,
                        Date = reportDate,
                        Amount = 50m,
                        Note = "Food",
                        OperationTypeId = 11,
                        OperationType = new OperationTypeDtoBlazor
                        {
                            OperationTypeId = 11,
                            Name = "Groceries",
                            IsIncome = false
                        }
                    }
                }
            };

            var handler = new MockHttpMessageHandler();
            handler.SetResponse(HttpStatusCode.OK, dto);

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test.local/") };
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient("Api").Returns(httpClient);

            Services.AddSingleton(httpClientFactory);
            Services.AddSingleton(Substitute.For<IJSRuntime>());

            var cut = Render<DailyReport>();

            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#Date"), reportDate.ToString("yyyy-MM-dd"));
            //cut.FindComponent<EditForm>().Submit();
            Bunit.EventHandlerDispatchExtensions.Submit(cut.Find("form"));

            cut.WaitForElement("table");

            // Act
            var incomeButton = cut.FindAll("button").Single(b => b.TextContent.Trim() == "Income");
           Bunit.EventHandlerDispatchExtensions.Click(incomeButton);

            // Assert
            cut.FindAll("table tbody tr").Count.Should().Be(1);
            cut.Markup.Should().Contain("table-success");
        }

        [Fact]
        public void FilterExpenses_WhenClicked_ShouldShowOnlyExpenseRows()
        {
            // Arrange
            DateTime dateTime = new DateTime(2026, 01, 15);
            var reportDate = DateOnly.FromDateTime(dateTime);

            var dto = new DailyReportDtoBlazor
            {
                Date = reportDate,
                TotalIncome = 100m,
                TotalExpenses = 50m,
                Operations = new()
                {
                    new OperationDtoBlazor
                    {
                        OperationId = 1,
                        Date = reportDate,
                        Amount = 100m,
                        Note = "Salary",
                        OperationTypeId = 10,
                        OperationType = new OperationTypeDtoBlazor
                        {
                            OperationTypeId = 10,
                            Name = "Salary",
                            IsIncome = true
                        }
                    },
                    new OperationDtoBlazor
                    {
                        OperationId = 2,
                        Date = reportDate,
                        Amount = 50m,
                        Note = "Food",
                        OperationTypeId = 11,
                        OperationType = new OperationTypeDtoBlazor
                        {
                            OperationTypeId = 11,
                            Name = "Groceries",
                            IsIncome = false
                        }
                    }
                }
            };

            var handler = new MockHttpMessageHandler();
            handler.SetResponse(HttpStatusCode.OK, dto);

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test.local/") };
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient("Api").Returns(httpClient);

            Services.AddSingleton(httpClientFactory);
            Services.AddSingleton(Substitute.For<IJSRuntime>());

            var cut = Render<DailyReport>();

            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#Date"), reportDate.ToString("yyyy-MM-dd"));
            //cut.FindComponent<EditForm>().Submit();
            Bunit.EventHandlerDispatchExtensions.Submit(cut.Find("form"));

            cut.WaitForElement("table");

            // Act
            var expensesButton = cut.FindAll("button").Single(b => b.TextContent.Trim() == "Expenses");
           Bunit.EventHandlerDispatchExtensions.Click(expensesButton);

            // Assert
            cut.FindAll("table tbody tr").Count.Should().Be(1);
            cut.Markup.Should().Contain("table-danger");
        }

        [Fact]
        public void Submit_WhenApiReturnsFailure_ShouldRenderErrorAlert()
        {
            // Arrange
            DateTime dateTime = new DateTime(2026, 01, 15);
            var reportDate = DateOnly.FromDateTime(dateTime);

            var handler = new MockHttpMessageHandler();
            handler.SetResponse(HttpStatusCode.BadRequest, "Bad request");

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test.local/") };
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient("Api").Returns(httpClient);

            Services.AddSingleton(httpClientFactory);
            Services.AddSingleton(Substitute.For<IJSRuntime>());

            var cut = Render<DailyReport>();

            // Act
            Bunit.EventHandlerDispatchExtensions.Change(cut.Find("#Date"), reportDate.ToString("yyyy-MM-dd"));
            Bunit.EventHandlerDispatchExtensions.Submit(cut.Find("form"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Find(".alert.alert-danger").TextContent.Should().Contain("Failed to generate report");
            });
        }

        // Kept local to this test file to avoid coupling with other test types.
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
