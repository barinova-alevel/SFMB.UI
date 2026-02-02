using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
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
    public class OperationTypesTests : BunitContext
    {
        [Fact]
        public void Render_ShouldShowPageHeader()
        {
            // Arrange
            Services.AddSingleton(Substitute.For<IJSRuntime>());
            Services.AddSingleton(CreateHttpClientFactory(operationTypes: new List<OperationTypeModel>()));

            // Act
            var cut = Render<OperationTypes>();

            // Assert
            cut.Find("h3").TextContent.Should().Be("Operation Types Management");
        }

        [Fact]
        public void Render_WhenNoOperationTypes_ShouldShowNoOperationTypesAlert()
        {
            // Arrange
            Services.AddSingleton(Substitute.For<IJSRuntime>());
            Services.AddSingleton(CreateHttpClientFactory(operationTypes: new List<OperationTypeModel>()));

            // Act
            var cut = Render<OperationTypes>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Markup.Should().Contain("No operation types found");
            });
        }

        [Fact]
        public void Render_WhenOperationTypesExist_ShouldRenderIncomeAndExpenseSections()
        {
            // Arrange
            var operationTypes = new List<OperationTypeModel>
            {
                new OperationTypeModel { OperationTypeId = 1, Name = "Salary", IsIncome = true, Description = "Monthly" },
                new OperationTypeModel { OperationTypeId = 2, Name = "Groceries", IsIncome = false, Description = "Food" }
            };

            Services.AddSingleton(Substitute.For<IJSRuntime>());
            Services.AddSingleton(CreateHttpClientFactory(operationTypes));

            // Act
            var cut = Render<OperationTypes>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Markup.Should().Contain("Salary");
                cut.Markup.Should().Contain("Groceries");
            });
        }

        [Fact]
        public void AddNewOperationTypeButton_WhenClicked_ShouldOpenCreateModal()
        {
            // Arrange
            Services.AddSingleton(Substitute.For<IJSRuntime>());
            Services.AddSingleton(CreateHttpClientFactory(operationTypes: new List<OperationTypeModel>()));

            var cut = Render<OperationTypes>();

            // Act
            // Prefer the button text selector to avoid accidentally selecting other primary buttons.
            cut.FindAll("button")
                .Single(b => b.TextContent.Contains("Add New", StringComparison.OrdinalIgnoreCase))
                .Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Find(".modal-title").TextContent.Should().Contain("Create");
                cut.Find(".modal-title").TextContent.Should().Contain("Operation Type");
            });
        }

        [Fact]
        public void EditButton_WhenClicked_ShouldOpenEditModal()
        {
            // Arrange
            var operationTypes = new List<OperationTypeModel>
            {
                new OperationTypeModel { OperationTypeId = 1, Name = "Salary", IsIncome = true, Description = "Monthly" }
            };

            Services.AddSingleton(Substitute.For<IJSRuntime>());
            Services.AddSingleton(CreateHttpClientFactory(operationTypes));

            var cut = Render<OperationTypes>();

            // Ensure at least one edit button is rendered
            cut.WaitForAssertion(() => cut.FindAll("button.btn.btn-sm.btn-outline-primary").Count.Should().BeGreaterThan(0));

            // Act
            cut.Find("button.btn.btn-sm.btn-outline-primary").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Find(".modal-title").TextContent.Should().Contain("Edit");
                cut.Find(".modal-title").TextContent.Should().Contain("Operation Type");
            });
        }

        [Fact]
        public void DeleteButton_WhenClicked_ShouldOpenDeleteConfirmationModal()
        {
            // Arrange
            var operationTypes = new List<OperationTypeModel>
            {
                new OperationTypeModel { OperationTypeId = 2, Name = "Groceries", IsIncome = false, Description = "Food" }
            };

            Services.AddSingleton(Substitute.For<IJSRuntime>());
            Services.AddSingleton(CreateHttpClientFactory(operationTypes));

            var cut = Render<OperationTypes>();

            // Ensure at least one delete button is rendered
            cut.WaitForAssertion(() => cut.FindAll("button.btn.btn-sm.btn-outline-danger").Count.Should().BeGreaterThan(0));

            // Act
            cut.Find("button.btn.btn-sm.btn-outline-danger").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                cut.Find(".modal-title").TextContent.Should().Be("Confirm Delete");
                cut.Markup.Should().Contain("Are you sure you want to delete");
            });
        }

        private static IHttpClientFactory CreateHttpClientFactory(List<OperationTypeModel> operationTypes)
        {
            var handler = new MockHttpMessageHandler(operationTypes);
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test.local/") };

            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient("Api").Returns(httpClient);
            return httpClientFactory;
        }

        private sealed class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly List<OperationTypeModel> _operationTypes;

            public MockHttpMessageHandler(List<OperationTypeModel> operationTypes)
            {
                _operationTypes = operationTypes;
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
