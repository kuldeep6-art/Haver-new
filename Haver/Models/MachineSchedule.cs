﻿using System.ComponentModel.DataAnnotations;

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
        public int NoteID { get; set; }

        //Note Annotations

        //[MaxLength(1000, ErrorMessage = "Limit of 1000 characters for notes.")]
        //[DataType(DataType.MultilineText)]
        public Note? Note { get; set; }

        public int MachineID { get; set; }

        public Machine? Machine { get; set; }

        public PackageRelease? PackageRelease { get; set; }

        public ICollection<MachineScheduleEngineer> MachineScheduleEngineers { get; set; } = new HashSet<MachineScheduleEngineer>();

        public ICollection<SalesOrder> SalesOrders { get; set; } = new HashSet<SalesOrder>();

        
    }
}
