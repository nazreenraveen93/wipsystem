using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WIPSystem.Web.Models;

namespace WIPSystem.Web.ViewModel
{
    public class CamberViewModel
    {
        public int CamberId { get; set; }
        // Hidden fields
        public int CurrentStatusId { get; set; }
        public string ProcessStatus { get; set; }
        // Basic properties
        public string PartNo { get; set; }
        public string LotNo { get; set; }
        public int ReceivedQuantity { get; set; }
        public string PackageSize { get; set; }
        public string CustomerName { get; set; }
        public int PiecesPerBlank { get; set; }

        // Dropdown for selecting the machine
        public int SelectedMachineId { get; set; }
        public string SelectedMachineOption { get; set; }
        public IEnumerable<SelectListItem> MachineOptions { get; set; }

        // Machine details
        public int MachineId { get; set; }
        public string MachineName { get; set; }

        // Start and end times
        public DateTime MachineStartTime { get; set; }
        public DateTime MachineEndTime { get; set; }

        // Calculated or entered quantities
        public double Hour { get; set; }
        public decimal AvgThickness { get; set; }
        public decimal PieceWeight { get; set; }
        public string Range { get; set; }
        public int OutputQty { get; set; }
        public int MachineRejectQty { get; set; }
        public int CamberQty { get; set; }
        public decimal Yield { get; set; }
        public decimal RejectRate { get; set; }
        public int TotalQty { get; set; }
        public decimal TotalWeight { get; set; }
        public int InputQty { get; set; }

        // Text area for remarks
        public string Remarks { get; set; }

        // Dropdown for status
        public CamberProcessStatus Status { get; set; }

        // Person in charge (PIC)
        public string CheckedBy { get; set; }

        // Additional property
        public string ProcessCurrentStatus { get; set; }

        public CamberViewModel()
        {
            MachineOptions = new List<SelectListItem>();
           
        }
    }
}

