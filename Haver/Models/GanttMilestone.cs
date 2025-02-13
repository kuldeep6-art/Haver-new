using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class GanttMilestone
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "You must select the Gantt Task")]
        [Display(Name = "Gantt Task")]
        public int GanttTaskID { get; set; }
        public GanttTask? GanttTask { get; set; }  // Proper foreign key setup

        [Required(ErrorMessage = "You must select the milestone")]
        [Display(Name = "Milestone Name")]
        public string MilestoneName { get; set; }  // Example: "Engineering Released"

        [Required(ErrorMessage = "You must enter progress.")]
        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100.")]
        [Display(Name = "Progress (%)")]
        public int Progress { get; set; }  // Store as an integer percentage

        [Display(Name = "Date Completed")]
        public DateTime? DateCompleted { get; set; }  // Nullable, for tracking milestone completion

    }
}
