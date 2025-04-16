using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class Engineer : Auditable
    {
        public int ID { get; set; }

        #region SUMMARY PROPERTIES

        [Display(Name = "Engineer Initials")]
        public string EngineerInitialsB
        {
            get
            {

                if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
                {
                    return EngineerInitials + " ( " + FirstName + " " + LastName + " ) ";
                }
                return string.Empty;
            }
        }


        #endregion


        [Display(Name = "Engineer Initials")]
        [Required(ErrorMessage = "Enter Engineer Initials.")]
        [MaxLength(2, ErrorMessage = "First name cannot be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "First name cannot be less than 2 characters long.")]
        public string? EngineerInitials { get; set; }

        //First Name Annotations
        [Display(Name = "First Name")]
        [Required(ErrorMessage = "Enter the first name of the engineer .")]
        [MaxLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "First name cannot be less than 2 characters long.")]
        public string? FirstName { get; set; }

        //Last Name Annotations
        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Enter the last name of the engineer.")]
        [MaxLength(50, ErrorMessage = "Last name cannot be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "Last name cannot be less than 2 characters long.")]
        public string? LastName { get; set; }

        //Email Annotations
        [Display(Name = "Email")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please follow the correct email format test@email.com")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = "";
        public ICollection<SalesOrderEngineer> SalesOrderEngineers { get; set; } = new HashSet<SalesOrderEngineer>();


    }
}
