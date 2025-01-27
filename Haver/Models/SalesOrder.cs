using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class SalesOrder : Auditable, IValidatableObject
    {
        public int ID { get; set; }

        //OrderNumber  Annotations
        
        [Display(Name = "Order Number")]
        [Required(ErrorMessage = "Order Number cannot be blank")]
        [RegularExpression("^\\d{8}$", ErrorMessage = "The sales order number must be exactly 8 numeric digits.")]
        [StringLength(8, ErrorMessage = "Order number must be exactly 8 digits.")]
        public string? OrderNumber { get; set; }

        //SoDate  Annotations

        [Display(Name = "Order Date")]
        [Required(ErrorMessage = "Order date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime SoDate { get; set; }

        //Price  Annotations

        [Display(Name = "Price")]
        [Required(ErrorMessage = "Price is Required")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "The price involved with this order must be a positive value.")]
        public Decimal Price { get; set; }

        //ShippingTerms  Annotations

        [Display(Name = "ShippingTerms")]
        [Required(ErrorMessage = "ShippingTerms is Required")]
        [MaxLength(800, ErrorMessage = "Shipping terms cannot exceed 800 characters")]
        public string? ShippingTerms { get; set; }

        //Recieved Drawings  Annotations
        [Display(Name = "Approved Drawings Recieved")]
        [Required(ErrorMessage = "Enter the date approved drawings from the customer was recieved")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime AppDwgRcd { get; set; }

        //Drawinds Sent  Annotations
        [Display(Name = "Order Drawings Sent")]
        [Required(ErrorMessage = "Select the date drawings for this order was sent to the customer")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DwgIsDt { get; set; }

        [Display(Name = "Purchase Order Number")]
        public string? PoNumber {  get; set; }

        [Display(Name = "Customer")]
        [Required(ErrorMessage = "Select the customer related to this order")]
        public int CustomerID { get; set; }
        public Customer? Customer { get; set; }


        [Display(Name = "Vendor")]
        [Required(ErrorMessage = "Select the vendor related to this order")]
        public int VendorID { get; set; }
        public Vendor? Vendor { get; set; }

        [Display(Name = "Machine Schedule")]
        [Required(ErrorMessage = "Select the machine schedule related to this order.")]
        public int MachineScheduleID {  get; set; }
   
        public MachineSchedule? MachineSchedule { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate that Approved Drawings Received date is after Drawings Sent date
            if (AppDwgRcd < DwgIsDt)
            {
                yield return new ValidationResult("Approved Drawings Received date cannot be earlier than Order Drawings Sent date.", new[] { nameof(AppDwgRcd) });
            }

        }
    }
}
