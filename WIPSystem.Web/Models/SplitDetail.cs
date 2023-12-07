using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class SplitDetail
    {
        [Key]
        public int SplitDetailId { get; set; } // Primary key for the SplitDetail table

        [ForeignKey("SplitLot")]
        public int SplitLotId { get; set; } // Foreign key to the SplitLot table
       
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; } // The quantity for this specific split

        [Required]
        public string LotNumber { get; set; } // Lot number for this specific split

        public string Camber { get; set; } // Additional detail for this split

        // Navigation property back to SplitLot
        public virtual SplitLot SplitLot { get; set; }
    }
}
