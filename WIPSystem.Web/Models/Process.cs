using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class Process
    {
        [Key]
        public int ProcessId { get; set; } // 'process_id' field, primary key, likely non-nullable in your database.

        [Display(Name ="Process Code")]
        [Required]
        public int ProcessCode { get; set; } 

        [Display(Name = "Process Name")]
        [Required]
        public string ProcessName { get; set; }

        public ICollection<ProductProcessMapping> ProductProcessMappings { get; set; }

        public ICollection<Machine> Machines { get; set; }

      
    }

}
