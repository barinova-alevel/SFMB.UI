using System.ComponentModel.DataAnnotations;

namespace BlazorApp.UI.Models
{
    public class OperationTypeModel
    {
        public int OperationTypeId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public bool IsIncome { get; set; }
        public virtual ICollection<OperationModel> OperationModels { get; set; } = new HashSet<OperationModel>();
    }
}
