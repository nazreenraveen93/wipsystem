using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class ProductProcessMapping
    {
        [Key]
        public int ProductProcessMappingId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [ForeignKey("Process")]
        public int ProcessId { get; set; }
        public Process Process { get; set; }

        [Display(Name = "Sequence")]
        [Required]
        public int Sequence { get; set; }

        public int ProcessCode { get; set; } // Add this line if it's missing
    }

}