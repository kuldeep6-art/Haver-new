using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public enum miledates
    {
        [Display(Name = "Engineering Released to Customer")]
        ecustomer,
        [Display(Name = "Customer Approval Received")]
        careceived,
        [Display(Name = "Package Released to PIC")]
        ppic,
        [Display(Name = "Spare Parts to Customer Service")]
        scus,
        [Display(Name = "Purchase Orders Issued")]
        poissued,
        [Display(Name = "Supplier Purchase Orders Due")]
        spdue,
        [Display(Name = "Machine Assembly & Testing")]
        mtesting
    }
}
