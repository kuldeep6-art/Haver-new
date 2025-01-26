using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class MachineSchedule : Auditable
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

        //Start Date Annotation (set to datetime now.)
        [Display(Name = "Start On")]
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
        //[Required(ErrorMessage = "End date is required")]
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
        //public int NoteID { get; set; }

        //public Note? Note { get; set; }

        public int MachineID { get; set; }

        public Machine? Machine { get; set; }


        //// One-to-one relationship with PackageRelease
        //public int? PackageReleaseID { get; set; }
        //public PackageRelease? PackageRelease { get; set; }


        // One-to-One Relationships
        public Note? Note { get; set; } // Navigation property to Note
        public PackageRelease? PackageRelease { get; set; } // Navigation property to PackageRelease
        public ICollection<MachineScheduleEngineer> MachineScheduleEngineers { get; set; } = new HashSet<MachineScheduleEngineer>();

        public ICollection<SalesOrder> SalesOrders { get; set; } = new HashSet<SalesOrder>();

        
    }
}
