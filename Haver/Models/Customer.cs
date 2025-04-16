using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class Customer : Auditable
    {
        public int ID { get; set; }

        #region SUMMARY PROPERTIES

        [Display(Name = "Company Phone")]
        public string PhoneFormatted =>
       !string.IsNullOrEmpty(Phone) && Phone.Length == 10
           ? $"({Phone.Substring(0, 3)}) {Phone.Substring(3, 3)}-{Phone[6..]}"
           : "N/A";

        #endregion

        //Company Name Annotations

        [Display(Name = "Company Name")]
        [Required(ErrorMessage = "Enter the name of the company this customer is related to")]
        [MaxLength(50, ErrorMessage = "Company name cannot be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "Company name cannot be less than 2 characters long.")]
        public string? CompanyName { get; set; }

        //Phone Annotations
        [Display(Name = "Company Phone")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number with no spaces.")]
        [DataType(DataType.PhoneNumber)]
        [MaxLength(10)]
        public string? Phone { get; set; }

        public ICollection<SalesOrder> SalesOrders { get; set; } = new HashSet<SalesOrder>();
    }
}
