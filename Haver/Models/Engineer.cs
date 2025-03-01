using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class Engineer : Auditable
    {
        public int ID { get; set; }

        #region SUMMARY PROPERTIES

        [Display(Name = "Engineer Initials")]
        public string EngineerInitials
        {
            get
            {

                if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
                {
                    return FirstName.Substring(0, 1).ToUpper() + LastName.Substring(0, 1).ToUpper();
                }
                return string.Empty;
            }
        }

        [Display(Name ="Engineer Initials")]
        public string EngineerInitialsB
        {
            get
            {

                if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
                {
                    return FirstName.Substring(0, 1).ToUpper() + LastName.Substring(0, 1).ToUpper() + FirstName + LastName;
                }
                return string.Empty;
            }
        }


        #endregion

        //First Name Annotations

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "Enter the first name of the engineer related to this schedule.")]
        [MaxLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "First name cannot be less than 2 characters long.")]
        public string? FirstName { get; set; }

        //Last Name Annotations

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Enter the first name of the engineer related to this schedule.")]
        [MaxLength(50, ErrorMessage = "Last name cannot be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "Last name cannot be less than 2 characters long.")]
        public string? LastName { get; set; }

        //Email Annotations

        //[Required(ErrorMessage = "Email address is required.")]
        [Display(Name = "Email")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please follow the correct email format test@email.com")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = "";
        public ICollection<SalesOrderEngineer> SalesOrderEngineers { get; set; } = new HashSet<SalesOrderEngineer>();




    }
}
