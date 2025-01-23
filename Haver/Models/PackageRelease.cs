using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class PackageRelease
    {
        public int ID { get; set; }

        //Name Annotations

        [Display(Name = "Name")]
        [Required(ErrorMessage = "Cannot leave the name blank.")]
        [MaxLength(50, ErrorMessage = "Name cannot be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "Name cannot be less then 2 characters")]
        public string? Name { get; set; }

        //Package Release DateP Annotations

        [Display(Name = "Package Release DateP")]
        [Required(ErrorMessage = "Date is Required")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? PReleaseDateP { get; set; }

        //Package Release DateA Annotations

        [Display(Name = "Package Release DateA")]
        [Required(ErrorMessage = "Date is Required")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? PReleaseDateA { get; set; }

        //Notes Annotations

        [Display(Name = "Notes About Package")]
        [Required(ErrorMessage = "cannot leave notes blank")]
        [MaxLength(400, ErrorMessage = "notes cannot be more than 400 characters long.")]
        [MinLength(10, ErrorMessage = "notes must be at least 10 characters long.")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        public int MachineScheduleID { get; set; }
        public MachineSchedule? Schedule { get; set; }
    }
}
