using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class SalesOrder
    {
        public int ID { get; set; }

        //OrderNumber  Annotations
        //[RegularExpression("^\\d{8}$", ErrorMessage = "The Order number must be exactly 8 numeric digits.")]
        [Display(Name = "Order Number")]
        [Required(ErrorMessage = "Order Number cannot be blank")]
        [RegularExpression("^\\d{8}$", ErrorMessage = "The sales order number must be exactly 8 numeric digits.")]
        [StringLength(8)]
        public string? OrderNumber { get; set; }

        //SoDate  Annotations

        [Display(Name = "SO Date")]
        [Required(ErrorMessage = "Date is Required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime SoDate { get; set; }

        //Price  Annotations

        [Display(Name = "Price")]
        [Required(ErrorMessage = "Price is Required")]
        [DataType(DataType.Currency)]
        public Decimal Price { get; set; }

        //ShippingTerms  Annotations

        [Display(Name = "ShippingTerms")]
        [Required(ErrorMessage = "ShippingTerms is Required")]
        [MaxLength(200, ErrorMessage = "Shipping terms cannot exceed 200 characters")]
        public string? ShippingTerms { get; set; }

        //AppDwgRcd  Annotations

        [Display(Name = "Approved Drawings Recieved")]
        [Required(ErrorMessage = "Enter the date approved drawings from the customer was recieved")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime AppDwgRcd { get; set; }

        //DwgIsDt  Annotations

        [Display(Name = "Order Drawings Sent")]
        [Required(ErrorMessage = "Enter the date drawings for this order was sent to the customer")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DwgIsDt { get; set; }

        public string? PoNumber {  get; set; } 
        public int CustomerID { get; set; }

        public Customer? Customer { get; set; }

        public int VendorID { get; set; }

        public Vendor? Vendor { get; set; }

        public int MachineScheduleID {  get; set; }

        public MachineSchedule? MachineSchedule { get; set; }
    }
}
