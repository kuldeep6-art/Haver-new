using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class Procurement
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "You must select the Vendor.")]
        [Display(Name = "Vendor")]
        public int VendorID { get; set; }

        [Display(Name = "Vendor")]
        public Vendor? Vendor { get; set; }


        [Display(Name = "Sales Order")]
        public int? SalesOrderID { get; set; }

        [Display(Name = "Sales Order")]
        public SalesOrder? SalesOrder { get; set; }

        [Display(Name = "Purchase Order Number")]
        public string? PONumber { get; set; }

        [Display(Name = "Expected Due Date")]
        [Required(ErrorMessage = "Expected Due Date is Required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ExpDueDate { get; set; }


        [Display(Name = "Delivery Date")]
        [Required(ErrorMessage = "Delivery is Required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DeliveryDate { get; set; }
    }
}
