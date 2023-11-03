using System.Collections.Generic;
using WIPSystem.Web.Models;

namespace WIPSystem.Web.ViewModels
{
    public class ProductProcessRegistrationViewModel
    {
        public int ProductId { get; set; } // <-- Add this line for ProductId
        public string PartNo { get; set; }
        public string CustName { get; set; }
        public List<Process> AvailableProcesses { get; set; }
        public List<SelectedProcess> SelectedProcesses { get; set; } = new List<SelectedProcess>();
    }


    public class SelectedProcess
    {
        public int ProcessId { get; set; }
        public int Sequence { get; set; }
    }

}
