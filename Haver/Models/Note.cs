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

        //AssemblyHours  Annotations

        [Display(Name = "Assembly Hours:")]
        [DataType(DataType.Time)]
        public Decimal AssemblyHours { get; set; }

        //ReworkHours  Annotations

        [Display(Name = "Rework Hours:")]
        [DataType(DataType.Time)]
        public Decimal ReworkHours { get; set; }

        //BudgetHours  Annotations

        [Display(Name = "Budget Hours:")]
        [DataType(DataType.Time)]
        public Decimal BudgetHours { get; set; }

        //NamePlate  Annotations

        [Display(Name = "Name plate")]
        public NamePlate? NamePlate { get; set; }

        public MachineSchedule? MachineSchedule { get; set; }
    }
}
