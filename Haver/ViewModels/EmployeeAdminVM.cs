using haver.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace haver.ViewModels
{
    [ModelMetadataType(typeof(EmployeeMetaData))]
    public class EmployeeAdminVM : EmployeeVM
    {
        public string Email { get; set; } = "";
        public bool Active { get; set; } = true;

        [Display(Name = "Roles")]
        public List<string> UserRoles { get; set; } = new List<string>();
    }
}
