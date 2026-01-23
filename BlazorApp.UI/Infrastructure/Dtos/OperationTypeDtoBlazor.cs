namespace BlazorApp.UI.Infrastructure.Dtos
{
    public class OperationTypeDtoBlazor
    {
        public int OperationTypeId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsIncome { get; set; }
    }
}
