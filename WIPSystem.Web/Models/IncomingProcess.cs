using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WIPSystem.Web.Models
{
    public class IncomingProcess
    {
        [Key]
        public int IncomingProcessId { get; set; }

        [Required(ErrorMessage = "Part number is required.")]
        [Display(Name = "Part Number")]
        public string PartNo { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } // Navigation property

        [Required(ErrorMessage = "Customer name is required.")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Pieces per blank is required.")]
        [Display(Name = "Pieces Per Blank")]
        public int PiecePerBlank { get; set; }

        [Required(ErrorMessage = "Package size is required.")]
        [Display(Name = "Package Size")]
        public string PackageSize { get; set; }

        [Required(ErrorMessage = "Lot number is required.")]
        [Display(Name = "Lot Number")]
        public string LotNo { get; set; }

        [Required(ErrorMessage = "Received quantity is required.")]
        [Display(Name = "Received Quantity")]
        public int ReceivedQuantity { get; set; }

        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Now; // Sets the current date by default

        [Display(Name = "Current Status")]
        public string ProcessCurrentStatus { get; set; } = "Incoming"; // Default status

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Required]
        [Display(Name = "Status")]
        public IncomingProcessStatus Status { get; set; }

        [Required(ErrorMessage = "Person in charge is required.")]
        [Display(Name = "PIC")]
        public string CheckedBy { get; set; }
    }

    // Define the enum for Status
    public enum IncomingProcessStatus
    {
        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "On Hold")]
        OnHold
    }
}
