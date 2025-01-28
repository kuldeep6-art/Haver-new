using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class Customer : Auditable
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
        [Required(ErrorMessage = "Customers first name can not be empty, it is required.")]
        [MaxLength(50, ErrorMessage = "First name can not be more than 50 characters long.")]
        [MinLength(2,ErrorMessage = "First name can not be less than 2 characters long.")]
        [RegularExpression("^[a-zA-Z-]+$", ErrorMessage = "First name can only contain letters and hyphens.")]
        public string? FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [MaxLength(50, ErrorMessage = "Middle name can not be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "Middle name can not be less than 2 characters long.")]
        [RegularExpression("^[a-zA-Z-]+$", ErrorMessage = "Middle name can only contain letters and hyphens.")]
        public string? MiddleName { get; set; }

        //Last Name Annotations

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Customers last name can not be empty, it is required")]
        [MaxLength(50, ErrorMessage = "Last name can not be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "Last name can not be less than 2 characters long.")]
        [RegularExpression("^[a-zA-Z-]+$", ErrorMessage = "Last name can only contain letters and hyphens.")]
        public string? LastName { get; set; }

        //Date Name Annotations

        [Display(Name = "Date")]
        [Required(ErrorMessage = "Select the day engineering package was released.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        //Phone Name Annotations

        //[Required(ErrorMessage = "Customers phone number is required and can not be left blank.")]
        [Display(Name= "Phone")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number with no spaces.")]
        [DataType(DataType.PhoneNumber)]
        [MaxLength(10)]
        public string? Phone { get; set; }

        //Company Name Annotations

        [Display(Name = "Company Name")]
       [Required(ErrorMessage = "Enter the name of the company this customer is related to")]
        [MaxLength(50, ErrorMessage = "Company name cannot be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "Company name cannot be less than 2 characters long.")]
        public string? CompanyName { get; set; }


        public ICollection<SalesOrder> SalesOrders { get; set; } = new HashSet<SalesOrder>();
    }
}
