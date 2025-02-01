using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class SalesOrderEngineer : Auditable
    {
        public int ID { get; set; }

        [Display(Name = "Machine Schedule")]
        [Required(ErrorMessage = "Select the schedule related to this engineer.")]
        public int SalesOrderID { get; set; }

        public SalesOrder? SalesOrder { get; set; }

        [Display(Name = "Engineer")]
        [Required(ErrorMessage = "Select the engineer related to this schedule.")]
        public int EngineerID { get; set; }
        public Engineer? Engineer { get; set; }
    }
}
