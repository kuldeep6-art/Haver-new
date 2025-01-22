using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class Note
    {
        public int ID { get; set; }

        //PreOrder  Annotations

        [Display(Name = "Pre-Order:")]
        public string? PreOrder { get; set; }

        //Scope  Annotations

        [Display(Name = "Scope:")]
        [MaxLength(200, ErrorMessage = "Limit of 200 characters for scope")]
        public string? Scope { get; set; }

        //AssemblyHours Annotations
        [Display(Name = "Assembly Hours:")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)] // Two decimal places
        public decimal AssemblyHours { get; set; }

        //ReworkHours Annotations
        [Display(Name = "Rework Hours:")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)] // Two decimal places
        public decimal ReworkHours { get; set; }

        //BudgetHours Annotations
        [Display(Name = "Budget Hours:")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)] // Two decimal places
        public decimal BudgetHours { get; set; }


        //NamePlate  Annotations

        [Display(Name = "Name plate")]
        public NamePlate? NamePlate { get; set; }

        public MachineSchedule? MachineSchedule { get; set; }
    }
}
