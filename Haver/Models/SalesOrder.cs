using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class SalesOrder
    {
        public int ID { get; set; }

        //OrderNumber  Annotations

        [Display(Name = "Order Number")]
        [Required(ErrorMessage = "Order Number Cannot be blank")]
        public int OrderNumber { get; set; }

        //SoDate  Annotations

        [Display(Name = "SO Date")]
        [Required(ErrorMessage = "Date is Required")]
        [DataType(DataType.DateTime)]
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

        [Display(Name = "AppDwgRcd")]
        [Required(ErrorMessage = "AppDwgRcd is Required")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime AppDwgRcd { get; set; }

        //DwgIsDt  Annotations

        [Display(Name = "DwgIsDt")]
        [Required(ErrorMessage = "DwgIsDt is Required")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DwgIsDt { get; set; }
        public int CustomerID { get; set; }
        public Customer? Customer { get; set; }
        public int VendorID { get; set; }
        public Vendor? Vendor { get; set; }

        public ICollection<SalesOrderMachine> SalesOrderMachines { get; set; } = new HashSet<SalesOrderMachine>();
        public ICollection<SalesOrderPO> SalesOrderPOs { get; set; } = new HashSet<SalesOrderPO>();
    }
}
