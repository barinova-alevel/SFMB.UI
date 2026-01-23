using BlazorApp.UI.Infrastructure.Dtos;

namespace BlazorApp.UI.Domain.Models
{
    public class DailyReportModel
    {
        public DateOnly Date { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public List<OperationDtoBlazor> Operations { get; set; } = new();
    }
}
