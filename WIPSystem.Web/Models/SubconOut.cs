using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class SubconOut
    {
        // Assuming Insp2Id is an auto-incrementing primary key
        public int SSubconOutId { get; set; }

        // String properties for the varchar fields
        public string PartNo { get; set; }
        public string LotNo { get; set; }

        // DateTime properties for date and datetime fields
        public DateTime Date { get; set; }
        public string ProcessCurrentStatus { get; set; } = "S-Subcon-Out"; // Default status
        public string CheckedBy { get; set; }
        public SSubconOutProcessStatus Status { get; set; }

        // For the Remarks text column, you can still use string type as it can hold large texts
        public string Remarks { get; set; }

        // Integer properties for int fields
        public int ProductId { get; set; }

        public int OutputQty { get; set; }
        public int RejectQty { get; set; }


    }

    public enum SSubconOutProcessStatus
    {
        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "On Hold")]
        OnHold
    }
}
