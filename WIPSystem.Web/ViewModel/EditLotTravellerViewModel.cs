using Microsoft.AspNetCore.Mvc.Rendering;

namespace WIPSystem.Web.ViewModel
{
    public class EditLotTravellerViewModel
    {
        public EditLotTravellerViewModel()
        {
            ProcessSteps = new List<ProcessStepViewModel>();
            AvailableProcesses = new SelectList(new List<ProcessViewModel>(), "ProcessId", "ProcessName");
            SelectedProcesses = new List<SelectedProcessViewModel>();
        }
        public int LotTravellerId { get; set; }
        public string LotNo { get; set; }
        public string PartNo { get; set; }
        public string CustName { get; set; }
        public string PackageSize { get; set; }
        public int PiecesPerBlank { get; set; }
        public DateTime DateCreated { get; set; } // Added DateCreated property
        public List<ProcessStepViewModel> ProcessSteps { get; set; } // Renamed from ProcessFlow for clarity
        public SelectList ProductSelectList { get; set; } // For dropdown list of products
        public string EmployeeNumber { get; set; }
        public DateTime UpdatedDate { get; set; }
        public SelectList AvailableProcesses { get; set; }
        // New properties to be added
        //public List<ProcessViewModel> AvailableProcesses { get; set; }
        public List<SelectedProcessViewModel> SelectedProcesses { get; set; }


    }

    public class EditProcessStepViewModel // Renamed
    {
        public string Sequence { get; set; }
        public string ProcessName { get; set; }
        // ... other properties
    }

    // This class represents the processes selected for the Lot Traveller
    // Ensure it has ProcessId and a sequence number
    public class SelectedProcessViewModel
    {
        public int ProcessId { get; set; }
        public int Sequence { get; set; }
        // ... other properties ...
    }
}
