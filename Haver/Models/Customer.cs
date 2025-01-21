using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class Customer
    {
        public int ID { get; set; }

        //First Name Annotations

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "Cannot leave the first name blank.")]
        [MaxLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        public string? FirstName { get; set; }

        //Last Name Annotations

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Cannot leave the last name blank.")]
        [MaxLength(50, ErrorMessage = "Last name cannot be more than 50 characters long.")]
        public string? LastName { get; set; }

        //Date Name Annotations

        [Display(Name = "Date")]
        [Required(ErrorMessage = "Date is Required")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        //Phone Name Annotations

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
        [DataType(DataType.PhoneNumber)]
        [MaxLength(10)]
        public string? Phone { get; set; }


        public string? CompanyName { get; set; }


        public ICollection<SalesOrder> SalesOrders { get; set; } = new HashSet<SalesOrder>();
    }
}
