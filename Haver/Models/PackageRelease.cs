using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class PackageRelease : Auditable , IValidatableObject
    {
        public int ID { get; set; }

        //Name Annotations

        [Display(Name = "Name")]
        [Required(ErrorMessage = "Cannot leave the name blank.")]
        [MaxLength(50, ErrorMessage = "Name cannot be more than 50 characters long.")]
        [MinLength(2, ErrorMessage = "Name cannot be less then 2 characters")]
        public string? Name { get; set; }

        //Package Release DateP Annotations

        [Display(Name = "Date Released")]
        [Required(ErrorMessage = "Date is Required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? PReleaseDateP { get; set; }

        //Package Release DateA Annotations

        [Display(Name = "Date Approved")]
        [Required(ErrorMessage = "Date is Required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? PReleaseDateA { get; set; }

        //Notes Annotations

        [Display(Name = "Notes About Package")]
        [Required(ErrorMessage = "cannot leave notes blank")]
        [MaxLength(400, ErrorMessage = "notes cannot be more than 400 characters long.")]
        [MinLength(10, ErrorMessage = "notes must be at least 10 characters long.")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        public int MachineScheduleID { get; set; }
        public MachineSchedule? MachineSchedule { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate that Date Approved is after or equal to Date Released
            if (PReleaseDateP.HasValue && PReleaseDateA.HasValue && PReleaseDateA < PReleaseDateP)
            {
                yield return new ValidationResult("Date Approved cannot be earlier than Date Released.", new[] { nameof(PReleaseDateA) });
            }

            // Validate that Dates fall within the Machine Schedule timeline
            if (MachineSchedule != null)
            {
                if (PReleaseDateP.HasValue && (PReleaseDateP < MachineSchedule.StartDate || PReleaseDateP > MachineSchedule.DueDate))
                {
                    yield return new ValidationResult("Date Released must be within the Machine Schedule timeline.", new[] { nameof(PReleaseDateP) });
                }

                if (PReleaseDateA.HasValue && (PReleaseDateA < MachineSchedule.StartDate || PReleaseDateA > MachineSchedule.DueDate))
                {
                    yield return new ValidationResult("Date Approved must be within the Machine Schedule timeline.", new[] { nameof(PReleaseDateA) });
                }
            }

            // Validate that notes contain enough useful content
            if (!string.IsNullOrEmpty(Notes) && Notes.Trim().Length < 10)
            {
                yield return new ValidationResult("Notes must provide sufficient information (at least 10 characters).", new[] { nameof(Notes) });
            }
        }
    }
    }
