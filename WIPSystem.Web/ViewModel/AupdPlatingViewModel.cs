using Microsoft.AspNetCore.Mvc.Rendering;
using WIPSystem.Web.Models;

namespace WIPSystem.Web.ViewModel
{
    public class AupdPlatingViewModel
    {
        public int CurrentStatusId { get; set; } // Hidden field
        public int AupdPlatingId { get; set; } // Hidden field
        // Fields corresponding to form inputs
        public string PartNo { get; set; }
        public string LotNo { get; set; }
        public int ReceivedQuantity { get; set; }
        public string PackageSize { get; set; }
        public string CustomerName { get; set; }
        public int PiecesPerBlank { get; set; }
        public string ProcessStatus { get; set; } // Hidden field

        // Dropdown for selecting the machine
        public int SelectedMachineId { get; set; }
        public string SelectedMachineOption { get; set; }
        public IEnumerable<SelectListItem> MachineOptions { get; set; }

        // Start and end times
        public DateTime MachineStartTime { get; set; }
        public DateTime MachineEndTime { get; set; }

        // Calculated or entered quantities
        public double Hour { get; set; } // Assuming this is a calculated field and not a direct input
        public int OutputQty { get; set; }
        public int RejectQty { get; set; }
        public int Difference { get; set; } // Assuming this is an input. If it's calculated, it should be a double or decimal.

        // Text area for remarks
        public string Remarks { get; set; }

        //// Dropdown for status
        public AupdPlatingProcessStatus Status { get; set; }

        // Person in charge (PIC)
        public string CheckedBy { get; set; }

        public string ProcessCurrentStatus { get; set; }

        // Property to hold the corresponding machine id
        public int MachineId { get; set; }

        public string MachineName { get; set; } // Add this line

        // Constructor to initialize MachineOptions if needed
        public AupdPlatingViewModel()
        {
            MachineOptions = new List<SelectListItem>();
        }
    }
}

