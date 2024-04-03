using WIPSystem.Web.Models;

namespace WIPSystem.Web.ViewModel
{
    public class IncomingProcessIndexViewModel
    {
        public int IncomingProcessId { get; set; }
        public string PartNo { get; set; }
        public string LotNo { get; set; }
        public DateTime Date { get; set; }
        public int ReceivedQuantity { get; set; }
        public string CheckedBy { get; set; }
        public string Remarks { get; set; }
        // Add the Status property
        public IncomingProcessStatus Status { get; set; }
    }

}
