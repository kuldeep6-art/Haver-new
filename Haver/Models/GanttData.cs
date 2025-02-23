using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class GanttData : IValidatableObject
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Please select the machine")]
        [Display(Name = "Machine")]
        public int MachineID { get; set; }
        public Machine? Machine { get; set; }

        [Display(Name = "Approval Drawing Received")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? AppDRcd { get; set; }

        // Engineering Milestones  
        [Display(Name = "Engineering Package Expected")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? EngExpected { get; set; }

        [Display(Name = "Engineering Released")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? EngReleased { get; set; }

        [Display(Name = "Customer Approval Received")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? CustomerApproval { get; set; }

        // Procurement  
        [Display(Name = "Package Released to PIC/Spare Parts to Customer Service")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? PackageReleased { get; set; }

        [Display(Name = "Purchase Orders Issued")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseOrdersIssued { get; set; }

        [Display(Name = "Purchase Orders Completed")]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseOrdersCompleted { get; set; }

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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            // Ensure EngReleased is after EngExpected
            if (EngExpected.HasValue && EngReleased.HasValue && EngReleased.Value < EngExpected.Value)
            {
                yield return new ValidationResult("Drawings Received from Customer cannot be earlier than Drawings Issued to Customer.", new[] { "AppDwgRet" });
            }

            // Ensure CustomerApproval is after EngReleased
            if (EngReleased.HasValue && CustomerApproval.HasValue && CustomerApproval.Value < EngReleased.Value)
            {
                yield return new ValidationResult("Customer Approval date must not be earlier than Engineering Released date.", new[] { "CustomerApproval" });
            }

            // Ensure PackageReleased is after CustomerApproval
            if (CustomerApproval.HasValue && PackageReleased.HasValue && PackageReleased.Value < CustomerApproval.Value)
            {
                yield return new ValidationResult("Package Released date must not be earlier than Customer Approval date.", new[] { "PackageReleased" });
            }

            // Ensure ShipExpected is after PackageReleased
            if (PackageReleased.HasValue && ShipExpected.HasValue && ShipExpected.Value < PackageReleased.Value)
            {
                yield return new ValidationResult("Ship Expected date must not be earlier than Package Released date.", new[] { "ShipExpected" });
            }

            // Ensure DeliveryExpected is after ShipExpected
            if (ShipExpected.HasValue && DeliveryExpected.HasValue && DeliveryExpected.Value < ShipExpected.Value)
            {
                yield return new ValidationResult("Delivery Expected date must not be earlier than Ship Expected date.", new[] { "DeliveryExpected" });
            }





        }
    }



}