using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class GanttTask
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "You must select the Sales Order")]
        [Display(Name = "Sales Order")]
        public int SalesOrderID { get; set; }
        public SalesOrder? SalesOrder { get; set; }

        [Required(ErrorMessage ="Start Date is Required")]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is Required")]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Special Notes")]
        public string Notes { get; set; }  // Inline note tracking

        // Navigation property for related milestones
        public ICollection<GanttMilestone> GanttMilestones { get; set; } = new HashSet<GanttMilestone>();

        // Computed progress based on milestones
        [NotMapped]
        public int OverallProgress => GanttMilestones.Any()
            ? (int)(GanttMilestones.Average(m => m.Progress))
            : 0;
    }
}
