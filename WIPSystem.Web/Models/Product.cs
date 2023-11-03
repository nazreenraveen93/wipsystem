using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Display(Name = "Part Number")]
        [Required]
        public string PartNo { get; set; }

        [Display(Name = "Customer Name")]
        [Required]
        public string CustName { get; set; }

        [Display(Name = "Package Size")]
        [Required]
        public string PackageSize { get; set; }

        [Display(Name = "Pieces Per Blank")]
        [Required]
        public int PiecesPerBlank { get; set; }

        public ICollection<ProductProcessMapping> ProductProcessMappings { get; set; }

    }

}
