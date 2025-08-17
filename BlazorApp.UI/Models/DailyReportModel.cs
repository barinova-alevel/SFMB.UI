using BlazorApp.UI.Dtos;

namespace BlazorApp.UI.Models
{
    public class DailyReportModel
    {
        public DateTime Date { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public List<OperationDtoBlazor> Operations { get; set; } = new();
    }
}
