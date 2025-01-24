using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class Customer
    {
        public int ID { get; set; }

        #region SUMMARY PROPERTIES

        [Display(Name = "Customer Name")]
        public string Summary
        {
            get
            {
                string FullName = FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                        (" " + (char?)MiddleName[0] + ". ").ToUpper())
                    + LastName;
                return FullName;
            }
        }

        [Display(Name = "Date")]
        public string DateSummary
        {
            get
            {
                return $"Rel {Date:MMM-yy}"; 
            }
        }

        public string PhoneFormatted =>
       !string.IsNullOrEmpty(Phone) && Phone.Length == 10
           ? $"({Phone.Substring(0, 3)}) {Phone.Substring(3, 3)}-{Phone[6..]}"
           : "N/A";

        #endregion 

        //First Name Annotations

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "Cannot leave the first name blank.")]
        [MaxLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        [MinLength(2,ErrorMessage = "First name cannot be less than 2 characters long.")]
        public string? FirstName { get; set; }

        [Display(Name = "Middle Name")]
        //[Required(ErrorMessage = "Cannot leave the last name blank.")]
        [MaxLength(50, ErrorMessage = "Middle name cannot be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "Middle name cannot be less than 2 characters long.")]
        public string? MiddleName { get; set; }

        //Last Name Annotations

        [Display(Name = "Last Name")]
        //[Required(ErrorMessage = "Cannot leave the last name blank.")]
        [MaxLength(50, ErrorMessage = "Last name cannot be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "Last name cannot be less than 2 characters long.")]
        public string? LastName { get; set; }

        //Date Name Annotations

        [Display(Name = "Date")]
        [Required(ErrorMessage = "Date is Required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        //Phone Name Annotations

        //[Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
        [DataType(DataType.PhoneNumber)]
        [MaxLength(10)]
        public string? Phone { get; set; }

        //Company Name Annotations

        //[Required(ErrorMessage = "Company name is required.")]
        [Display(Name = "Company Name")]
        [MaxLength(50, ErrorMessage = "Company name cannot be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "Company name cannot be less than 2 characters long.")]
        public string? CompanyName { get; set; }


        public ICollection<SalesOrder> SalesOrders { get; set; } = new HashSet<SalesOrder>();
    }
}
