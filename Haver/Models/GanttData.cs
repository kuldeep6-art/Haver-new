using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class GanttData
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Please select the machine")]
        [Display(Name = "Machine")]
        public int MachineID { get; set; }
        public Machine? Machine { get; set; }

        // Engineering Milestones  
        [Display(Name = "Engineering Package Expected")]
        [DataType(DataType.Date)]
        public DateTime? EngExpected { get; set; }

        [Display(Name = "Engineering Released")]
        [DataType(DataType.Date)]
        public DateTime? EngReleased { get; set; }

        [Display(Name = "Customer Approval Received")]
        [DataType(DataType.Date)]
        public DateTime? CustomerApproval { get; set; }

        // Procurement  
        [Display(Name = "Package Released to PIC/Spare Parts to Customer Service")]
        [DataType(DataType.Date)]
        public DateTime? PackageReleased { get; set; }

        [Display(Name = "Purchase Orders Issued")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseOrdersIssued { get; set; }

        [Display(Name = "Purchase Orders Completed")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseOrdersCompleted { get; set; }

        [Display(Name = "Supplier Purchase Order Due")]
        [DataType(DataType.Date)]
        public DateTime? SupplierPODue { get; set; }

        // Assembly & Testing  
        [Display(Name = "Machine Assembly and Testing Start")]
        [DataType(DataType.Date)]
        public DateTime? AssemblyStart { get; set; }

        [Display(Name = "Machine Assembly and Testing Completed")]
        [DataType(DataType.Date)]
        public DateTime? AssemblyComplete { get; set; }

        // Shipping  
        [Display(Name = "Readiness To Ship Expected")]
        [DataType(DataType.Date)]
        public DateTime? ShipExpected { get; set; }

        [Display(Name = "Readiness To Ship Actual")]
        [DataType(DataType.Date)]
        public DateTime? ShipActual { get; set; }

        // Delivery  
        [Display(Name = "Delivery Expected")]
        [DataType(DataType.Date)]
        public DateTime? DeliveryExpected { get; set; }

        [Display(Name = "Delivery Actual")]
        [DataType(DataType.Date)]
        public DateTime? DeliveryActual { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }  // Notes for tracking changes or delays
    }



}