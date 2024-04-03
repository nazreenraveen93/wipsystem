using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.ViewModel
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string EmpNo { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string SelectedDepartment { get; set; } // To capture the selected department
        public List<SelectListItem> Departments { get; set; } // For the dropdown list
        public string SelectedRole { get; set; } // To capture the selected role
        public List<SelectListItem> Roles { get; set; } // For the role dropdown list
    }
}
