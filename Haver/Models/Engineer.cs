using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class Engineer
    {
        public int ID { get; set; }

        //First Name Annotations

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "cannot leave the first name blank.")]
        [MaxLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "First name cannot be less than 2 characters long.")]
        public string? FirstName { get; set; }

        //Last Name Annotations

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "cannot leave the last name blank.")]
        [MaxLength(50, ErrorMessage = "Last name cannot be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "Last name cannot be less than 2 characters long.")]
        public string? LastName { get; set; }

        //Phone Annotations

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
        [DataType(DataType.PhoneNumber)]
        [MaxLength(10)]
        public string? Phone { get; set; }

        //Email Annotations

        [Required(ErrorMessage = "Email address is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please follow the correct email format test@email.com")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = "";
        public ICollection<MachineScheduleEngineer> MachineScheduleEngineers { get; set; } = new HashSet<MachineScheduleEngineer>();
        
        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{ }


    }
}
