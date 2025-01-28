using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class MachineSchedule : Auditable, IValidatableObject
    {
        public int ID { get; set; }


        #region summary properties
        [Display(Name = "Machine Schedule")]
        public string Summary
        {
            get
            {
                return $"Starts on {StartDate:yyyy-MM-dd}, Due by {DueDate:yyyy-MM-dd}";
            }
        }


        #endregion

        //Start Date Annotation
        [Display(Name = "Starts On")]
        [Required(ErrorMessage = "The start date for can only be set to now or in days ahead")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        //DueDate Annotations

        [Display(Name = "Due By")]
        [Required(ErrorMessage = "Enter the day this schedule is due")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        //EndDate Annotations

        [Display(Name = "Ended On")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        //PackageRDate  Annotations

        [Display(Name = "Package Released On")]
        [Required(ErrorMessage = "Enter the day packages related to this schedule was relaeased.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PackageRDate { get; set; }

        //PODueDate Annotations

        [Display(Name = "Purchase Order Due On")]
        [Required(ErrorMessage = "Enter the day the purchase orders related to this schedule is due.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PODueDate { get; set; }

        //DeliveryDate Annotations

        [Display(Name = "Deliver By")]
        [Required(ErrorMessage = "Enter the day the order related to this schedule is due for delivery.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DeliveryDate { get; set; }


        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency


        //Media Annotations

        [Display(Name = "Media")]
        [Required(ErrorMessage = "Media is required")]
        public bool Media { get; set; }

        //SpareParts Annotations

        [Display(Name = "Spare Parts")]
        public bool SpareParts { get; set; }

        //SparePMedia Annotations

        [Display(Name = "Spare Parts/Media")]
        public bool SparePMedia { get; set; }

        //Base Annotations

        [Display(Name = "Base")]
        [Required(ErrorMessage = "Base is required")]
        public bool Base { get; set; }

        //Air Seal Annotations

        [Display(Name = "Air Seal")]
        [Required(ErrorMessage = "Air seal is required")]
        public bool AirSeal { get; set; }

        //Coating Lining Annotations

        [Display(Name = "Coating/Lining")]
        [Required(ErrorMessage = "Coating Lining is required")]
        public bool CoatingLining { get; set; }

        //Dissembly Annotations

        [Display(Name = "Dissembly")]
        [Required(ErrorMessage = "Dissembly is required")]
        public bool Dissembly { get; set; }
        //public int NoteID { get; set; }

        //public Note? Note { get; set; }

        public int MachineID { get; set; }

        public Machine? Machine { get; set; }


        //// One-to-one relationship with PackageRelease
        //public int? PackageReleaseID { get; set; }
        //public PackageRelease? PackageRelease { get; set; }


        // One-to-One Relationships

        [Display(Name = "Note")]
        public Note? Note { get; set; } // Navigation property to Note

        [Display(Name = "Package Release")]
        public PackageRelease? PackageRelease { get; set; } // Navigation property to PackageRelease

        [Display(Name = "Engineer")]
        public ICollection<MachineScheduleEngineer> MachineScheduleEngineers { get; set; } = new HashSet<MachineScheduleEngineer>();

        [Display(Name = "Sales Orders")]
        public ICollection<SalesOrder> SalesOrders { get; set; } = new HashSet<SalesOrder>();

        public bool IsCompleted { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate < DateTime.Today)
            {
                yield return new ValidationResult("Start Date cannot be in the past.", new[] { nameof(StartDate) });
            }

            if (DueDate <= StartDate)
            {
                yield return new ValidationResult("Due Date must be after the Start Date.", new[] { nameof(DueDate) });
            }

            if (PODueDate > DueDate)
            {
                yield return new ValidationResult("Purchase Order Due Date must not exceed the Due Date.", new[] { nameof(PODueDate) });
            }

        

           
        }
    }
}