using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace haver.Models
{
    public class Machine : Auditable
    {
        public int ID { get; set; }


        #region SUMMARY PROPERTIES

        //[Display(Name = "Readiness To Ship Expected")]
        //public string RToSExp => RToShipExp.ToString("MMM d, yyyy");

        //[Display(Name = "Readiness To Ship Actual ")]
        //public string RToSAc => RToShipExp.ToString("MMM d, yyyy");

        #endregion

        // Serial Number
        [Display(Name = "Serial Number")]
        [Required(ErrorMessage = "Serial number is required.")]
        public string? SerialNumber { get; set; }

        // Production Order Number
        [Display(Name = "Production Order Number")]
        [Required(ErrorMessage = "Production order number is required.")]
        public string? ProductionOrderNumber { get; set; }

        // Quantity
        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number.")]
        public int? Quantity { get; set; }

        // Machine Size
        [Display(Name = "Size")]
        [Required(ErrorMessage = "Machine size is required.")]
        public string? Size { get; set; }

        // Machine Class
        [Display(Name = "Class")]
        [Required(ErrorMessage = "Machine class is required.")]
        public string? Class { get; set; }

        [Display(Name = "Readiness To Ship Expected")]
        [Required(ErrorMessage = "Enter the Readiness to Ship Expected Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        public DateTime? RToShipExp { get; set; }


        [Display(Name = "Readiness To Ship Actual")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        public DateTime? RToShipA { get; set; }

        // Number of Decks
        [Display(Name = "Size/Deck")]
        [Required(ErrorMessage = "Size Deck is required.")]
        public string? SizeDeck { get; set; }

        // Checkbox Options
        [Display(Name = "Media")]
        public bool Media { get; set; }

        [Display(Name = "Spare Parts")]
        public bool SpareParts { get; set; }

        [Display(Name = "Spare Parts/Media")]
        public bool SparePMedia { get; set; }

        //Base Annotations

        [Display(Name = "Base")]
        public bool Base { get; set; }

        //Air Seal Annotations

        [Display(Name = "Air Seal")]
        public bool AirSeal { get; set; }

        //Coating Lining Annotations

        [Display(Name = "Coating/Lining")]
        public bool CoatingLining { get; set; }

        //Dissembly Annotations

        [Display(Name = "Disassembly")]
        [Required(ErrorMessage = "Disassembly is required")]
        public bool Disassembly { get; set; }

        // Budgeted & Actual Assembly Hours
        [Display(Name = "Budgeted Hours")]
        [Range(0, int.MaxValue, ErrorMessage = "Budgeted hours must be a positive number.")]
        public Decimal? BudgetedHours { get; set; }

        [Display(Name = "Actual Assembly Hours")]
        [Range(0, int.MaxValue, ErrorMessage = "Actual assembly hours must be a positive number.")]
        public  Decimal? ActualAssemblyHours { get; set; }

        [Display(Name = "Rework Hours")]
        [Range(0, int.MaxValue, ErrorMessage = "Rework hours must be a positive number.")]
        public Decimal? ReworkHours { get; set; }

        // Nameplate Status
        [Display(Name = "Nameplate Status")]
        public NamePlate? Nameplate { get; set; } = NamePlate.Required;

        [Display(Name = "Pre-Order:")]
        public string? PreOrder { get; set; }

        //Scope  Annotations
        [Display(Name = "Scope:")]
        public string? Scope { get; set; }

        // Foreign Key to SalesOrder
        [Display(Name = "Sales Order")]
        [Required(ErrorMessage = "Select the Sales Order associated with this machine.")]
        public int SalesOrderID { get; set; }

        [Display(Name = "Sales Order")]
        [Required(ErrorMessage = "Select the Sales Order associated with this machine.")]
        public SalesOrder? SalesOrder { get; set; }

        // Foreign Key to MachineType
        [Display(Name = "Machine Type")]
        [Required(ErrorMessage = "Select the Machine Type")]
        public int MachineTypeID { get; set; }
        public MachineType? MachineType { get; set; }

        // One-to-Many: A Machine can have multiple Procurements
        public ICollection<Procurement> Procurements { get; set; } = new HashSet<Procurement>();

    }
}
