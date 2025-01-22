using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class Machine
    {
        public int ID { get; set; }

        //Machine Desc Annotations

        [Display(Name = "Machine Description")]
        [Required(ErrorMessage = "cannot leave the machine description blank.")]
        [MaxLength(100, ErrorMessage = "Description cannot exceed 100 Characters")]
        [MinLength(10, ErrorMessage = "Description cannot be less than 10 characters long.")]

        public string? Description { get; set; }

        //Serial Num Annotations

        [Display(Name = "Serial Number")]
        [Required(ErrorMessage = "Serial number is required")]
        public string? SerialNumber { get; set; }

        //Quantity Num Annotations

        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a Positive number")]
        public int Quantity { get; set; }

        //Size of machine Annotations

        [Display(Name = "Size of Machine")]
        [Required(ErrorMessage = "Size Is required")]
        public string? Size { get; set; }

        //Class Annotations

        [Display(Name = "Class")]
        [Required(ErrorMessage = "Class is required")]
        public string? Class { get; set; }

        //SizeDeck Annotations

        [Display(Name = "Size Deck")]
        [Required(ErrorMessage = "Size Deck is required")]
        public string? SizeDeck { get; set; }

       

        public ICollection<MachineSchedule> MachineSchedules { get; set; } = new HashSet<MachineSchedule>();
    }
}
