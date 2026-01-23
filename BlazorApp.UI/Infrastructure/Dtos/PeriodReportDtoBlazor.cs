namespace BlazorApp.UI.Infrastructure.Dtos
{
    public class PeriodReportDtoBlazor
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public List<OperationDtoBlazor> Operations { get; set; } = new();
    }
}
