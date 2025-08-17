using System.ComponentModel.DataAnnotations;

namespace BlazorApp.UI.Models
{
    public class OperationModel
    {
        public int OperationId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public decimal? Amount { get; set; }
        public string? Note { get; set; }

        [Required(ErrorMessage = "Please select an operation type.")]
        public int? OperationTypeId { get; set; }
        public virtual OperationTypeModel? OperationTypeModel { get; set; }
    }
}
