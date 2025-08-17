namespace BlazorApp.UI.Dtos
{
    public class DailyReportDtoBlazor
    {
        public DateTime Date { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public List<OperationDtoBlazor> Operations { get; set; } = new();
    }
}
