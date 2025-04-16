using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace haver.Models
{
    public class GanttData : IValidatableObject
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Please select the sales order")]
        [Display(Name = "Sales Order")]
        public int SalesOrderID { get; set; }
        public SalesOrder? SalesOrder { get; set; }

        [Display(Name = "Machine")]
        public int? MachineID { get; set; }
        public Machine? Machine { get; set; }

        [Display(Name = "Approval Drawings Due")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? AppDExp { get; set; }

        [Display(Name = "Approval Drawings Released")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? AppDRcd { get; set; }

        [Display(Name = "Start of Week")]
        public WeekStartOption StartOfWeek { get; set; } = WeekStartOption.Monday; // Default to Monday

        public int WeekNumber
        {
            get
            {
                var today = DateTime.Today; 
                var culture = CultureInfo.CurrentCulture;
                return culture.Calendar.GetWeekOfYear(today,
                    culture.DateTimeFormat.CalendarWeekRule,
                    StartOfWeek == WeekStartOption.Monday ? DayOfWeek.Monday : DayOfWeek.Friday);
            }
        }

        public string Month => DateTime.Today.ToString("MMMM");
        public int Day => DateTime.Today.Day;

        // Engineering Milestones  
        [Display(Name = "Engineering Package Due")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? EngExpected { get; set; }

        [Display(Name = "Engineering Package Released")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? EngReleased { get; set; }

        // Procurement  
        [Display(Name = "Package Released to PIC/Spare Parts to Customer Service")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? PackageReleased { get; set; }

        [Display(Name = "Purchase Orders Expected")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseOrdersIssued { get; set; }

        [Display(Name = "Purchase Orders Issued")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseOrdersCompleted { get; set; }

        [Display(Name = "Purchase Orders Received")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseOrdersReceived { get; set; }

        [Display(Name = "Supplier Purchase Order Due")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? SupplierPODue { get; set; }

        // Assembly & Testing  
        [Display(Name = "Machine Assembly and Testing Start")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? AssemblyStart { get; set; }

        [Display(Name = "Machine Assembly and Testing Completed")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? AssemblyComplete { get; set; }

        // Shipping  
        [Display(Name = "Readiness To Ship Expected")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? ShipExpected { get; set; }

        [Display(Name = "Readiness To Ship Actual")]
        [DataType(DataType.Date)]
        public DateTime? ShipActual { get; set; }

        // Delivery  
        [Display(Name = "Delivery Expected")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? DeliveryExpected { get; set; }

        [Display(Name = "Delivery Actual")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? DeliveryActual { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }  // Notes for tracking changes or delays

        public bool IsFinalized { get; set; } = false;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {        
            if (AssemblyStart.HasValue && AssemblyComplete.HasValue && AssemblyComplete.Value < AssemblyStart.Value)
            {
                yield return new ValidationResult("Assembly Complete Date cannot be less than Assembly Start Date", new[] { "AssemblyComplete" });
            }

            if (PurchaseOrdersIssued.HasValue && PurchaseOrdersCompleted.HasValue && PurchaseOrdersCompleted.Value < PurchaseOrdersIssued.Value)
            {
                yield return new ValidationResult("Purchase Orders Completed cannot be less than Purchase Orders Issued Date", new[] { "PurchaseOrdersCompleted" });
            }

        }
    }
}