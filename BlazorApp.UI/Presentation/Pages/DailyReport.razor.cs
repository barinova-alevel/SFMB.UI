using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using BlazorApp.UI.Infrastructure.Dtos;
using BlazorApp.UI.Domain.Models;
using Microsoft.JSInterop;

namespace BlazorApp.UI.Presentation.Pages
{
    public partial class DailyReport
    {
        public class DailyReportRequest
        {
            [Required(ErrorMessage = "Date is required")]
            public DateTime Date { get; set; }
        }

        private DailyReportRequest reportRequest = new();
        private DailyReportModel? report;
        private List<OperationDtoBlazor>? filteredOperations;
        private bool isGenerating = false;
        private string? errorMessage;
        private bool hasRendered = false;

        protected override async Task OnInitializedAsync()
        {
            var now = DateTime.Now;
            reportRequest.Date = new DateTime(now.Year, now.Month, now.Day);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !hasRendered)
            {
                hasRendered = true;
                await JSRuntime.InvokeVoidAsync("console.log", "Daily Report page initialized");
            }
        }

        private async Task GenerateReport()
        {
            try
            {
                isGenerating = true;
                errorMessage = null;

                var client = HttpClientFactory.CreateClient("Api");
                var response = await client.GetAsync($"api/dailyreport/report/daily?Date={reportRequest.Date:yyyy-MM-dd}");
                Console.WriteLine($" Date {reportRequest.Date:yyyy-MM-dd}");
                Console.WriteLine(response.ToString());

                if (response.IsSuccessStatusCode)
                {
                    var reportDto = await response.Content.ReadFromJsonAsync<DailyReportDtoBlazor>();
                    if (reportDto != null)
                    {
                        report = new DailyReportModel
                        {
                            Date = reportDto.Date,
                            TotalIncome = reportDto.TotalIncome,
                            TotalExpenses = reportDto.TotalExpenses,
                            Operations = reportDto.Operations?.Select(o => new OperationDtoBlazor
                            {
                                OperationId = o.OperationId,
                                Date = o.Date,
                                Amount = o.Amount,
                                Note = o.Note,
                                OperationTypeId = o.OperationTypeId,
                                OperationType = o.OperationType != null ? new OperationTypeDtoBlazor
                                {
                                    OperationTypeId = o.OperationType.OperationTypeId,
                                    Name = o.OperationType.Name,
                                    Description = o.OperationType.Description,
                                    IsIncome = o.OperationType.IsIncome
                                } : null
                            }).ToList() ?? new()
                        };
                        filteredOperations = report.Operations;
                    }
                    await JSRuntime.InvokeVoidAsync("console.log", $"Report generated for {reportRequest.Date:yyyy-MM-dd}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    errorMessage = $"Failed to generate report: {response.StatusCode}. {errorContent}";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error generating report: {ex.Message}";
                await JSRuntime.InvokeVoidAsync("console.error", "Error generating report:", ex);
            }
            finally
            {
                isGenerating = false;
            }
        }

        private void FilterOperations(bool? isIncome)
        {
            if (report?.Operations == null)
            {
                filteredOperations = new();
                return;
            }

            if (isIncome.HasValue)
            {
                filteredOperations = report.Operations
                    .Where(o => o.OperationType?.IsIncome == isIncome.Value)
                    .ToList();
            }
            else
            {
                filteredOperations = report.Operations.ToList();
            }
        }
    }
}
