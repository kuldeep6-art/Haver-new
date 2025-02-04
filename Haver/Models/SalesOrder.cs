using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace haver.Models
{
    public class SalesOrder : Auditable
    {
        public int ID { get; set; }

        #region Summary Properties

        [Display(Name = "Order Date")]
        public string SummaryDate => SoDate.ToString("MMM d, yyyy");

        [Display(Name = "Approved Drawings Received")]
        public string AppDate => AppDwgRcd.ToString("MMM d, yyyy");

        [Display(Name = "Order Drawings Sent")]
        public string DwgDte => DwgIsDt.ToString("MMM d, yyyy");

		[Display(Name = "Date")]
		public string DateSummary
		{
			get
			{
				return $"Rel {PDate:MMM-yy}";
			}
		}



		#endregion

		[Display(Name = "Order Number")]
        [Required(ErrorMessage = "Order Number cannot be blank")]
        [RegularExpression("^\\d{8}$", ErrorMessage = "The sales order number must be exactly 8 numeric digits.")]
        [StringLength(8, ErrorMessage = "Order number must be exactly 8 digits.")]
        public string? OrderNumber { get; set; }

        [Display(Name = "Order Date")]
        [Required(ErrorMessage = "Order date is required")]
        [DataType(DataType.Date)]
        public DateTime SoDate { get; set; }

        //[Required(ErrorMessage = "Price is Required")]
        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "The price must be a positive value.")]
        public Decimal Price { get; set; }

        //[Required(ErrorMessage = "Shipping Terms are Required")]
        [Display(Name = "Shipping Terms")]
        [MaxLength(800, ErrorMessage = "Shipping terms cannot exceed 800 characters")]
        public string? ShippingTerms { get; set; }

        [Display(Name = "Approved Drawings Received")]
        [Required(ErrorMessage = "Enter the date approved drawings from the customer were received")]
        [DataType(DataType.Date)]
        public DateTime AppDwgRcd { get; set; }

        [Display(Name = "Order Drawings Sent")]
        [Required(ErrorMessage = "Select the date drawings for this order were sent to the customer")]
        [DataType(DataType.Date)]
        public DateTime DwgIsDt { get; set; }

		[Display(Name = "Eng Package Release")]
		[Required(ErrorMessage = "Select the day engineering package was released.")]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime PDate { get; set; }

		[Display(Name = "Customer")]
        [Required(ErrorMessage = "Select the customer related to this order")]
        public int CustomerID { get; set; }
        public Customer? Customer { get; set; }

        // One-to-Many: A Sales Order has multiple Machines
        public ICollection<Machine> Machines { get; set; } = new HashSet<Machine>();

        // Many-to-Many: A Sales Order can have multiple Engineers
        [Display(Name = "Engineers")]
        public ICollection<SalesOrderEngineer> SalesOrderEngineers { get; set; } = new HashSet<SalesOrderEngineer>();

        // One-to-Many: A Sales Order can have multiple Procurements
        public ICollection<Procurement> Procurements { get; set; } = new HashSet<Procurement>();

        [Display(Name = "Notes/Comments")]
        public string? Comments { get; set; }

        [Display(Name = "Package Release Information")]
        public PackageRelease? PackageRelease { get; set; } // Navigation property to PackageRelease

        public Status Status { get; set; } = Status.InProgress;

     
    }
}
