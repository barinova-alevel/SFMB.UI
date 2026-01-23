namespace BlazorApp.UI.Infrastructure.Dtos
{
    public class DailyReportDtoBlazor
    {
        public DateOnly Date { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public List<OperationDtoBlazor> Operations { get; set; } = new();
    }
}
