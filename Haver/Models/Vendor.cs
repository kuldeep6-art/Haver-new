using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class Vendor
    {
        public int ID { get; set; }


        #region SUMMARY PROPERTIES

        public string PhoneFormatted => "(" + Phone.Substring(0, 3) + ") "
          + Phone.Substring(3, 3) + "-" + Phone[6..];

        #endregion 

        //ShippingTerms  Annotations

        [Display(Name = "Name")]
        //[Required(ErrorMessage = "You cannot leave the name blank.")]
        //[MaxLength(50, ErrorMessage = "name cannot be more than 50 characters long.")]
        public Name? Name { get; set; }

        //ShippingTerms  Annotations

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
        [DataType(DataType.PhoneNumber)]
        [MaxLength(10)]
        public string? Phone { get; set; }

        //ShippingTerms  Annotations

        [Required(ErrorMessage = "Email address is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please follow the correct email format test@email.com")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        public ICollection<SalesOrder> SalesOrders { get; set; } = new HashSet<SalesOrder>();
    }
}
