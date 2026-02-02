using System.Net;
using System.Text;
using System.Text.Json;
using BlazorApp.UI.Components.Pages;
using BlazorApp.UI.Models;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NSubstitute;
using Xunit;

namespace Tests.Pages
{
    public class OperationsTests : BunitContext
    {
        [Fact]
        public void Render_ShouldShowPageHeader()
        {
            // Arrange
            Services.AddSingleton(Substitute.For<IJSRuntime>());
            Services.AddSingleton(CreateHttpClientFactory(
                operationTypes: new List<OperationTypeModel>(),
                operations: new List<OperationModel>()));

            // Act
            var cut = Render<Operations>();

            // Assert
            cut.Find("h3").TextContent.Should().Be("Operations");
            cut.Markup.Should().Contain("List of operations");
        }

        [Fact]
        public void Render_WhenNoOperations_ShouldShowNoOperationsAlert()
        {
            // Arrange
            Services.AddSingleton(Substitute.For<IJSRuntime>());
            Services.AddSingleton(CreateHttpClientFactory(
                operationTypes: new List<OperationTypeModel>(),
                operations: new List<OperationModel>()));

            // Act
            var cut = Render<Operations>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Find(".alert.alert-warning").TextContent.Should().Contain("No operations found");
            });
        }

        [Fact]
        public void Render_WhenOperationsExist_ShouldRenderTableRows()
        {
            // Arrange
            var operationTypes = new List<OperationTypeModel>
            {
                new OperationTypeModel { OperationTypeId = 1, Name = "Salary", IsIncome = true },
                new OperationTypeModel { OperationTypeId = 2, Name = "Groceries", IsIncome = false }
            };

            var operations = new List<OperationModel>
            {
                new OperationModel
                {
                    OperationId = 10,
                    Date = DateOnly.FromDateTime(new DateTime(2026, 01, 10)),
                    Amount = 100m,
                    Note = "January salary",
                    OperationTypeId = 1
                },
                new OperationModel
                {
                    OperationId = 11,
                    Date = DateOnly.FromDateTime(new DateTime(2026, 01, 11)),
                    Amount = 50m,
                    Note = "Food",
                    OperationTypeId = 2
                }
            };

            Services.AddSingleton(Substitute.For<IJSRuntime>());
            Services.AddSingleton(CreateHttpClientFactory(
                operationTypes: operationTypes,
                operations: operations));

            // Act
            var cut = Render<Operations>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.FindAll("table tbody tr").Count.Should().Be(2);
                cut.Markup.Should().Contain("Salary");
                cut.Markup.Should().Contain("Groceries");
            });
        }

        [Fact]
        public void AddNewOperationButton_WhenClicked_ShouldOpenCreateModal()
        {
            // Arrange
            Services.AddSingleton(Substitute.For<IJSRuntime>());
            Services.AddSingleton(CreateHttpClientFactory(
                operationTypes: new List<OperationTypeModel>
                {
                    new OperationTypeModel { OperationTypeId = 1, Name = "Salary", IsIncome = true }
                },
                operations: new List<OperationModel>()));

            var cut = Render<Operations>();

            // Act
            cut.Find("button.btn.btn-primary").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Find(".modal-title").TextContent.Should().Contain("Create");
                cut.Find(".modal-title").TextContent.Should().Contain("Operation");
            });
        }

        [Fact]
        public void EditButton_WhenClicked_ShouldOpenEditModal()
        {
            // Arrange
            var operationTypes = new List<OperationTypeModel>
            {
                new OperationTypeModel { OperationTypeId = 1, Name = "Salary", IsIncome = true }
            };

            var operations = new List<OperationModel>
            {
                new OperationModel
                {
                    OperationId = 10,
                    Date = DateOnly.FromDateTime(new DateTime(2026, 01, 10)),
                    Amount = 100m,
                    Note = "January salary",
                    OperationTypeId = 1
                }
            };

            Services.AddSingleton(Substitute.For<IJSRuntime>());
            Services.AddSingleton(CreateHttpClientFactory(operationTypes, operations));

            var cut = Render<Operations>();
            cut.WaitForElement("table");

            // Act
            cut.Find("button.btn.btn-sm.btn-outline-primary").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Find(".modal-title").TextContent.Should().Contain("Edit");
                cut.Find(".modal-title").TextContent.Should().Contain("Operation");
            });
        }

        [Fact]
        public void DeleteButton_WhenClicked_ShouldOpenDeleteConfirmationModal()
        {
            // Arrange
            var operationTypes = new List<OperationTypeModel>
            {
                new OperationTypeModel { OperationTypeId = 2, Name = "Groceries", IsIncome = false }
            };

            var operations = new List<OperationModel>
            {
                new OperationModel
                {
                    OperationId = 11,
                    Date = DateOnly.FromDateTime(new DateTime(2026, 01, 11)),
                    Amount = 50m,
                    Note = "Food",
                    OperationTypeId = 2
                }
            };

            Services.AddSingleton(Substitute.For<IJSRuntime>());
            Services.AddSingleton(CreateHttpClientFactory(operationTypes, operations));

            var cut = Render<Operations>();
            cut.WaitForElement("table");

            // Act
            cut.Find("button.btn.btn-sm.btn-outline-danger").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Find(".modal-title").TextContent.Should().Be("Confirm Delete");
                cut.Markup.Should().Contain("Are you sure you want to delete");
            });
        }

        private static IHttpClientFactory CreateHttpClientFactory(
            List<OperationTypeModel> operationTypes,
            List<OperationModel> operations)
        {
            var handler = new MockHttpMessageHandler(operationTypes, operations);
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test.local/") };

            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient("Api").Returns(httpClient);
            return httpClientFactory;
        }

        private sealed class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly List<OperationTypeModel> _operationTypes;
            private readonly List<OperationModel> _operations;

            public MockHttpMessageHandler(List<OperationTypeModel> operationTypes, List<OperationModel> operations)
            {
                _operationTypes = operationTypes;
                _operations = operations;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (request.Method == HttpMethod.Get && request.RequestUri is not null)
                {
                    var path = request.RequestUri.AbsolutePath.TrimStart('/');

                    if (path.Equals("api/operationtypes", StringComparison.OrdinalIgnoreCase))
                    {
                        return Task.FromResult(JsonResponse(_operationTypes));
                    }

                    if (path.Equals("api/operations", StringComparison.OrdinalIgnoreCase))
                    {
                        return Task.FromResult(JsonResponse(_operations));
                    }
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            private static HttpResponseMessage JsonResponse<T>(T value)
            {
                var json = JsonSerializer.Serialize(value);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }
        }
    }
}
