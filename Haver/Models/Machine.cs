using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace haver.Models
{
    public class Machine : Auditable
    {
        public int ID { get; set; }

        // Machine Description
        [Display(Name = "Machine Description")]
        [Required(ErrorMessage = "Machine description is required.")]
        [MaxLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
        [MinLength(10, ErrorMessage = "Description must be at least 10 characters.")]
        public string? Description { get; set; }

        // Production Order Number
        [Display(Name = "Production Order Number")]
        [Required(ErrorMessage = "Production order number is required.")]
        public string? ProductionOrderNumber { get; set; }

        // Serial Number
        [Display(Name = "Serial Number")]
        [Required(ErrorMessage = "Serial number is required.")]
        public string? SerialNumber { get; set; }

        // Quantity
        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number.")]
        public int Quantity { get; set; }

        // Machine Size
        [Display(Name = "Size")]
        [Required(ErrorMessage = "Machine size is required.")]
        public string? Size { get; set; }

        // Machine Class
        [Display(Name = "Class")]
        [Required(ErrorMessage = "Machine class is required.")]
        public string? Class { get; set; }

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

        [Display(Name = "Disassembly")]
        [Required(ErrorMessage = "Disassembly is required")]
        public bool Disassembly { get; set; }

        // Budgeted & Actual Assembly Hours
        [Display(Name = "Budgeted Hours")]
        [Required(ErrorMessage = "Enter the budgeted hours for this machine.")]
        [Range(0, int.MaxValue, ErrorMessage = "Budgeted hours must be a positive number.")]
        public Decimal BudgetedHours { get; set; }

        [Display(Name = "Actual Assembly Hours")]
        [Range(0, int.MaxValue, ErrorMessage = "Actual assembly hours must be a positive number.")]
        public  Decimal? ActualAssemblyHours { get; set; }

        [Display(Name = "Rework Hours")]
        [Range(0, int.MaxValue, ErrorMessage = "Rework hours must be a positive number.")]
        public Decimal? ReworkHours { get; set; }

        // Nameplate Status
        [Display(Name = "Nameplate Status")]
        [Required(ErrorMessage = "Select the nameplate status.")]
        public NamePlate? Nameplate { get; set; } // Options: Ordered, Received

        [Display(Name = "Pre-Order:")]
        public string? PreOrder { get; set; }

        //Scope  Annotations
        [Display(Name = "Scope:")]
        [MaxLength(1000, ErrorMessage = "Limit of 200 characters for scope")]
        public string? Scope { get; set; }

        // Foreign Key to SalesOrder
        [Display(Name = "Sales Order")]
        //[Required(ErrorMessage = "Select the Sales Order associated with this machine.")]
        public int SalesOrderID { get; set; }
        public SalesOrder? SalesOrder { get; set; }
    }
}
