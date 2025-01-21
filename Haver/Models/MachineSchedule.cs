using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class MachineSchedule
    {
        public int ID { get; set; }

        //DueDate Annotations

        [Display(Name = "Due Date")]
        [Required(ErrorMessage = "Due date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        //EndDate Annotations

        [Display(Name = "End Date")]
        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        //PackageRDate  Annotations

        [Display(Name = "Package Released Date")]
        [Required(ErrorMessage = "PackageR date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PackageRDate { get; set; }

        //PODueDate Annotations

        [Display(Name = "PODue Date")]
        [Required(ErrorMessage = "PODue date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PODueDate { get; set; }

        //DeliveryDate Annotations

        [Display(Name = "Delivery Date")]
        [Required(ErrorMessage = "Delivery Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DeliveryDate { get; set; }

        public int NoteID { get; set; }

        //Note Annotations

        //[MaxLength(1000, ErrorMessage = "Limit of 1000 characters for notes.")]
        //[DataType(DataType.MultilineText)]
        public Note? Note { get; set; }

        public Machine? Machine { get; set; }

        public ICollection<MachineScheduleEngineer> MachineScheduleEngineers { get; set; } = new HashSet<MachineScheduleEngineer>();

        public ICollection<PackageRelease> PackageReleases { get; set; } = new HashSet<PackageRelease>();
    }
}
