using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class MachineScheduleEngineer : Auditable
    {
        public int ID { get; set; }

        [Display(Name = "Machine Schedule")]
        [Required(ErrorMessage = "Select the schedule related to this engineer.")]
        public int MachineScheduleID { get; set; }

        public MachineSchedule? Schedule { get; set; }

        [Display(Name = "Engineer")]
        [Required(ErrorMessage = "Select the engineer related to this schedule.")]
        public int EngineerID { get; set; }
        public Engineer? Engineer { get; set; }
    }
}
