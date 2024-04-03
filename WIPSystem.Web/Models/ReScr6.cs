using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class ReScr6
    {
        // Assuming AgcusetId is an auto-incrementing primary key
        public int ReScr6Id { get; set; }

        // String properties for the varchar fields
        public string PartNo { get; set; }
        public string LotNo { get; set; }

        // DateTime properties for date and datetime fields
        public DateTime Date { get; set; }
        public string ProcessCurrentStatus { get; set; } = "Re-Scr.6"; // Default status
        public string CheckedBy { get; set; }
        public ReScr6ProcessStatus Status { get; set; }

        // For the Remarks text column, you can still use string type as it can hold large texts
        public string Remarks { get; set; }

        // Integer properties for int fields
        public int ProductId { get; set; }

        public int OutputQty { get; set; }
        public int RejectQty { get; set; }


    }

    public enum ReScr6ProcessStatus
    {
        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "On Hold")]
        OnHold
    }
}

