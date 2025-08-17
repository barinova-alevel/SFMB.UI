using BlazorApp.UI.Dtos;
using BlazorApp.UI.Models;
using Microsoft.JSInterop;

namespace BlazorApp.UI.Components.Pages
{
    public partial class PeriodReport
    {
        private PeriodReportRequest reportRequest = new();
        private PeriodReportModel? report;
        private List<OperationDtoBlazor>? filteredOperations;
        private bool isGenerating = false;
        private string? errorMessage;
        private bool hasRendered = false;

        protected override async Task OnInitializedAsync()
        {
            var now = DateTime.Now;
            reportRequest.StartDate = new DateTime(now.Year, now.Month, 1);
            reportRequest.EndDate = now.Date;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !hasRendered)
            {
                hasRendered = true;
                await JSRuntime.InvokeVoidAsync("console.log", "PeriodReport page initialized");
            }
        }

        private async Task GenerateReport()
        {
            try
            {
                isGenerating = true;
                errorMessage = null;

                var client = HttpClientFactory.CreateClient("Api");
                var response = await client.GetAsync($"api/periodreport/report/period?startDate={reportRequest.StartDate:yyyy-MM-dd}&endDate={reportRequest.EndDate:yyyy-MM-dd}");

                if (response.IsSuccessStatusCode)
                {
                    var reportDto = await response.Content.ReadFromJsonAsync<PeriodReportDtoBlazor>();
                    if (reportDto != null)
                    {
                        report = new PeriodReportModel
                        {
                            StartDate = reportDto.StartDate,
                            EndDate = reportDto.EndDate,
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
                    await JSRuntime.InvokeVoidAsync("console.log", $"Report generated for period: {reportRequest.StartDate:yyyy-MM-dd} to {reportRequest.EndDate:yyyy-MM-dd}");
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
