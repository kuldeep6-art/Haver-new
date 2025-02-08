using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class MachineType : Auditable
    {
        public int ID { get; set; }


        // Machine Description
        [Display(Name = "Machine Description")]
        [Required(ErrorMessage = "Machine description is required.")]
        [MaxLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
        [MinLength(10, ErrorMessage = "Description must be at least 5 characters.")]
        public string? Description { get; set; }

        public ICollection<Machine> Machines { get; set; } = new HashSet<Machine>();
    }
}
