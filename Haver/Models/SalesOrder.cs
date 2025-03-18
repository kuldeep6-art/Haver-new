using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace haver.Models
{
    public class SalesOrder : Auditable, IValidatableObject
    {
        public int ID { get; set; }

        #region SUMMARY PROPERTIES

        public string allprice
        {
            get
            {
                return Currency + " " + Price;
            }
        }

        public string MachineOrderDetail
        {
            get
            {
                return OrderNumber + " " + "|" + " " + CompanyName;
            }
        }

        #endregion


        [Display(Name = "Order Number")]
        [Required(ErrorMessage = "Order Number cannot be blank")]
        [RegularExpression("^\\d{8}$", ErrorMessage = "The sales order number must be exactly 8 numeric digits.")]
        [StringLength(8, ErrorMessage = "Order number must be exactly 8 digits.")]
        public string? OrderNumber { get; set; }


        [Display(Name = "Customer")]
        [Required(ErrorMessage = "Enter the name of the customer related to this order")]
        public string CompanyName { get; set; } = "";

        //for draft

        //[Required(ErrorMessage = "Order date is required")]
        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        public DateTime? SoDate { get; set; }

        
        [Display(Name = "Price")]
        [Range(0, double.MaxValue, ErrorMessage = "The price must be a positive value.")]
        public Decimal? Price { get; set; }

        [Display(Name = "Currency")]
        public string? Currency { get; set; }

        [Display(Name = "Shipping Terms")]
        [MaxLength(800, ErrorMessage = "Shipping terms cannot exceed 800 characters")]
        public string? ShippingTerms { get; set; }


        //for draft

        //[Required(ErrorMessage = "Enter the approval drawings expected date")]
        [Display(Name = "Approved Drawings Expected")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        public DateTime? AppDwgExp { get; set; }

        [Display(Name = "Approved Drawings Released")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        public DateTime? AppDwgRel { get; set; }

        [Display(Name = "Approved Drawings Returned")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        public DateTime? AppDwgRet { get; set; }

        [Display(Name = "Pre Orders Expected")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        public DateTime? PreOExp { get; set; } = DateTime.Now; 

        [Display(Name = "Pre Orders Released")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        public DateTime? PreORel { get; set; }

        [Display(Name = "Engineering Package Expected")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        public DateTime? EngPExp { get; set; }

        [Display(Name = "Engineering Package Released")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        public DateTime? EngPRel { get; set; }

        // Checkbox Options
        [Display(Name = "Installed Media")]
        public bool Media { get; set; }

        [Display(Name = "Spare Parts")]
        public bool SpareParts { get; set; }

        [Display(Name = "Spare Media")]
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


        //[Display(Name = "Customer")]
        //[Required(ErrorMessage = "Enter the customer related to this order")]
        //public string? CompanyName { get; set; }

        //[Display(Name = "Customer")]
        //[Required(ErrorMessage = "Select the customer related to this order")]
        //public int CustomerID { get; set; }

        //public Customer? Customer { get; set; }

        // One-to-Many: A Sales Order has multiple Machines
        public ICollection<Machine> Machines { get; set; } = new HashSet<Machine>();

        // Many-to-Many: A Sales Order can have multiple Engineers
        [Display(Name = "Engineers")]
        public ICollection<SalesOrderEngineer> SalesOrderEngineers { get; set; } = new HashSet<SalesOrderEngineer>();
      
        [Display(Name = "Notes/Comments")]
        public string? Comments { get; set; }

        [Display(Name = "Package Release Information")]
        public PackageRelease? PackageRelease { get; set; } // Navigation property to PackageRelease

        public Status Status { get; set; } = Status.InProgress;

        [Display(Name ="Draft Record")]
        public bool IsDraft { get; set; } = false;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
        
            if (!IsDraft)
            { 
                if (SoDate == null)
                {
                    yield return new ValidationResult("Order Date must be entered.", new[] { nameof(SoDate) });
                }

                if (AppDwgExp == null)
                {
                    yield return new ValidationResult("Drawings Expected Date must be entered.", new[] { nameof(AppDwgExp) });
                }
            }

            if (AppDwgRel != null && AppDwgRet != null && AppDwgRet < AppDwgRel)
            {
                yield return new ValidationResult(
                    "Drawings Received from Customer cannot be earlier than Drawings Issued to Customer.",
                    new[] { nameof(AppDwgRet) }
                );
            }

            if (Price.HasValue && string.IsNullOrEmpty(Currency))
            {
                yield return new ValidationResult("Currency is required if a price is entered.", new[] { "Currency" });
            }
        }
    }
}
