using System.ComponentModel.DataAnnotations;

namespace BlazorApp.UI.Presentation.Pages
{
    public class PeriodReportRequest
    {
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }
    }
}
