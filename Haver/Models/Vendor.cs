using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    //vendor class and properties
    public class Vendor : Auditable
    {
        public int ID { get; set; }


        #region SUMMARY PROPERTIES

        public string PhoneFormatted =>
      !string.IsNullOrEmpty(Phone) && Phone.Length == 10
          ? $"({Phone.Substring(0, 3)}) {Phone.Substring(3, 3)}-{Phone[6..]}"
          : "N/A";


        #endregion 

        //Name  Annotations

        [Display(Name = "Vendor Name")]
        [Required(ErrorMessage = "Enter the name of the vendor, it is required.")]
        [MaxLength(50, ErrorMessage = "Vendor name can not be more than 50 characters long.")]
        public string? Name { get; set; }

        //Phone number Annotations

        [Display(Name = "Phone Number")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number with no spacing.")]
        [DataType(DataType.PhoneNumber)]
        [MaxLength(10, ErrorMessage = "Phone can not be more than 10 digits.")]
        public string? Phone { get; set; }

        //Email Annotations

        [Display(Name = "Email")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please follow the correct email format test@email.com")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Procurement>? Procurements { get; set; } = new HashSet<Procurement>();
    }
}
