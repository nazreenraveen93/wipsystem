using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WIPSystem.Web.Models
{
    public class UserEntity
    {
        [Key]
        public int UserId { get; set; }

        public string username { get; set; }

        public string password { get; set; }

        public string employeeNo { get; set; }

        public string Role { get; set; }

        public string Email { get; set; }

        // Reference to Department
        [ForeignKey("Department")]
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        [ForeignKey("Process")]
        public int? ProcessId { get; set; }
        public virtual Process Process { get; set; }
    }
}


