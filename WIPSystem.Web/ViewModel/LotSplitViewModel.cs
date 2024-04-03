using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WIPSystem.Web.Models;

namespace WIPSystem.Web.ViewModel
{
    public class LotSplitViewModel
    {
        public int SplitLotId { get; set; } // Add this line
        public string EmpNo { get; set; }
        public string PartNo { get; set; }
        public string OriginalLot { get; set; }
        public int Quantity { get; set; }
        public int SplitSuffix { get; set; }
        public List<SplitDetailViewModel> SplitDetails { get; set; } = new List<SplitDetailViewModel>();

        // Add these properties
        public int ProcessId { get; set; }
        public int SelectedProcessId { get; set; } // Selected process ID
        public List<SelectListItem> Processes { get; set; } // List of processes for dropdown
        public string SelectedProcessName { get; set; }
        public string ProcessName { get; set; }

        // Add these properties
        public string SelectedPartNo { get; set; }
        public List<SelectListItem> PartNumbers { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime LastUpdated { get; set; }

    }

    public class SplitDetailViewModel
    {
        public int SplitDetailId { get; set; } // Add this line

        public int Quantity { get; set; }
        public string LotNumber { get; set; }
        public string Camber { get; set; }
    }

}
