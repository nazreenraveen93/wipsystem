using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class Machine
    {
        [Key]
        public int MachineId { get; set; } // Primary key

        [Display(Name = "Machine Name")]
        [Required]
        public string MachineName { get; set; }
        [Display(Name = "Machine Number")]

        [Required]
        public string MachineNumber { get; set; } // Added new field

        [Display(Name = "Machine Quantity")]
        [Required]
        public int MachineQty { get; set; }

        // Foreign key for Process
        public int ProcessId { get; set; }
        // Navigation property for Process
        public Process Process { get; set; }

        public ICollection<Jig> Jigs { get; set; }

    }
}
