using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class LotTraveller
    {
        [Key]
        public int LotTravellerId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public string LotNo { get; set; }

        // Assuming you don't store the QR code data in the database and generate it on-the-fly
        // public string QrCodeData { get; set; }

        // Stores the datetime when the lot traveller was created/registered
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        // Navigation property to retrieve customer name through the product
        public string CustomerName => Product?.CustName;

        // Navigation property to retrieve related process mappings
        // Not stored in the database, but used to access the ProductProcessMappings through the Product
        [NotMapped] // This attribute ensures it is not mapped to a database column
        public IEnumerable<ProductProcessMapping> ProcessMappings => Product?.ProductProcessMappings;
    }

}
