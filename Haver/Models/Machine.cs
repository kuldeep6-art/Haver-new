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
        [Range(1, int.MaxValue, ErrorMessage = "Size must be a Positive number")]
        public string? Size { get; set; }

        //Class Annotations

        [Display(Name = "Class")]
        [Required(ErrorMessage = "Class is required")]
        public string? Class { get; set; }

        //SizeDeck Annotations

        [Display(Name = "Size Deck")]
        [Required(ErrorMessage = "Size Deck is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Size Deck must be a Positive number")]
        public string? SizeDeck { get; set; }

        //Media Annotations

        [Display(Name = "Media")]
        [Required(ErrorMessage = "Media is required")]
        public bool Media { get; set; }

        //SpareParts Annotations

        [Display(Name = "Spare Parts")]
        public bool SpareParts { get; set; }

        //SparePMedia Annotations

        [Display(Name = "SpareP Media")]
        public bool SparePMedia { get; set; }

        //Base Annotations

        [Display(Name = "Base")]
        [Required(ErrorMessage = "Base is required")]
        public bool Base { get; set; }

        //Air Seal Annotations

        [Display(Name = "Air Seal")]
        [Required(ErrorMessage = "Air Seal is required")]
        public bool AirSeal { get; set; }

        //Coating Lining Annotations

        [Display(Name = "Coating Lining")]
        [Required(ErrorMessage = "Coating Lining is required")]
        public bool CoatingLining { get; set; }

        //Dissembly Annotations

        [Display(Name = "Dissembly")]
        [Required(ErrorMessage = "Dissembly is required")]
        public bool Dissembly { get; set; }

        public ICollection<SalesOrderMachine> SalesOrderMachines { get; set; } = new HashSet<SalesOrderMachine>();
    }
}
