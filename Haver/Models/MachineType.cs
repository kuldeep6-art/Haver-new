using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    //machine model class and properties
    public class MachineType : Auditable
    {
        public int ID { get; set; }



        [Display(Name = "Machine Model")]
        [Required(ErrorMessage = "Machine Model is required.")]
        public string? Description {  get; set; }

        public ICollection<Machine> Machines { get; set; } = new HashSet<Machine>();
    }
}
