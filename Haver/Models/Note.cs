using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class Note : Auditable
    {
        public int ID { get; set; }

        //PreOrder  Annotations

        [Display(Name = "Pre-Order:")]
        public string? PreOrder { get; set; }

        //Scope  Annotations

        [Display(Name = "Scope:")]
        [MaxLength(1000, ErrorMessage = "Limit of 200 characters for scope")]
        public string? Scope { get; set; }

        //AssemblyHours Annotations
        [Display(Name = "Assembly Hours:")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [Range(0, double.MaxValue, ErrorMessage = "Assembly hours cannot be negative.")]
        public decimal AssemblyHours { get; set; }

        //ReworkHours Annotations
        [Display(Name = "Rework Hours:")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [Range(0, double.MaxValue, ErrorMessage = "Rework hours cannot be negative.")]
        public decimal ReworkHours { get; set; }

        //BudgetHours Annotations
        [Display(Name = "Budget Hours:")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [Range(0, double.MaxValue, ErrorMessage = "Budget hours cannot be negative.")]
        public decimal BudgetHours { get; set; }

        //NamePlate  Annotations

        [Display(Name = "Name Plate")]
        public NamePlate? NamePlate { get; set; }

        //Machine schedule annotation
        [Display(Name = "Machine Schedule")]
        [Required(ErrorMessage = "Select the machine schedule this order is related to")]
        public int MachineScheduleID { get; set; }
        public MachineSchedule? MachineSchedule { get; set; }
    }
}
