namespace BlazorApp.UI.Dtos
{
    public class OperationDtoBlazor
    {
        public int OperationId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public int OperationTypeId { get; set; }
        public OperationTypeDtoBlazor? OperationType { get; set; }
    }
}
