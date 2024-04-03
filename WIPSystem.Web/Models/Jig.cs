using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class Jig
    {
        [Key]
        public int JigId { get; set; }

        [Display(Name = "Jig Name")]
        [Required]
        public string JigName { get; set; }

        [Display(Name = "Jig Life Span")]
        [Required]
        public int JigLifeSpan { get; set; }

        [Display(Name = "Current Usage")]
        public int CurrentUsage { get; set; } // Added property

        public int MachineId { get; set; }
        public Machine Machine { get; set; }
    }
}
