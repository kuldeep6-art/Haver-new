using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    //procurement class and properties
    public class Procurement
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "You must select the Vendor.")]
        [Display(Name = "Vendor")]
        public int VendorID { get; set; }

        [Display(Name = "Vendor")]
        public Vendor? Vendor { get; set; }


        [Display(Name = "Machine")]
        public int? MachineID { get; set; }

        [Display(Name = "Machine")]
        public Machine? Machine { get; set; }

        [Display(Name = "Purchase Order Number")]
        [Required(ErrorMessage = "Please Enter the Purchase Order Number")]
        [StringLength(10, ErrorMessage = "Purchase Order number must be exactly 10 digits.")] 
        public string PONumber { get; set; } = "";

        [Display(Name = "Purchase Orders Due")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        public DateTime? PODueDate { get; set; }

        [Display(Name = "Purchase Orders Received")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MMM d, yyyy}")]
        public DateTime? PORcd { get; set; }

        // Checkbox Options
        [Display(Name = "Quality Inspection Completed")]
        public bool QualityICom { get; set; }

        // Checkbox Options
        [Display(Name = "NCR Raised")]
        public bool NcrRaised { get; set; }
    }
}
