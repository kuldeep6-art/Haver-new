﻿using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class PackageRelease : Auditable
    {
        public int ID { get; set; }

		//Name Annotations
		[Display(Name = "Package Release")]
		public string Summary
		{
			get
			{
				return Name + "\n"
					+ (PReleaseDateP.HasValue ? "P - " + PReleaseDateP.Value.ToString("M/d/yyyy") : "") + "\n"
					+ (PReleaseDateA.HasValue ? "A - " + PReleaseDateA.Value.ToString("M/d/yyyy") : "");
			}
		}


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

        [Display(Name = "Sales Order")]
        public int SalesOrderID { get; set; }
        public SalesOrder? SalesOrder { get; set; }


    }
}
