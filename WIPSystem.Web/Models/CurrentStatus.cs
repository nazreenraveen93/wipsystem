using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WIPSystem.Web.Models
{
    public class CurrentStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CurrentStatusId { get; set; }

        [Required]
        [StringLength(50)]
        public string PartNo { get; set; }

        [Required]
        [StringLength(50)]
        public string LotNo { get; set; }

        [Required]
        public string ProcessCurrentStatus { get; set; }

        [Required]
        public int ReceivedQuantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        public string Remarks { get; set; }

        [Required]
        [StringLength(50)]
        public string PIC { get; set; }

        [Required]
        [StringLength(50)]
        public string NextProcess { get; set; }

        public DateTime Date { get; set; }

        // Foreign key to Product
        public int? ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        // Add ProcessId property
        public int? ProcessId { get; set; }

        public int? MachineId { get; set; }
     
    }
}