using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class AupdPlating
    {
        // Assuming SinterId is an auto-incrementing primary key
        public int AupdPlatingId { get; set; }
        // String properties for the varchar fields
        public string PartNo { get; set; }
        public string LotNo { get; set; }
        // DateTime properties for date and datetime fields
        public DateTime Date { get; set; }
        public string ProcessCurrentStatus { get; set; } = "Aupd Plating"; // Default status
        public string CheckedBy { get; set; }
        public AupdPlatingProcessStatus Status { get; set; }
        // For the Remarks text column, you can still use string type as it can hold large texts
        public string Remarks { get; set; }
        // Integer properties for int fields
        public int ProductId { get; set; }
        public int MachineId { get; set; }
        public int OutputQty { get; set; }
        public int RejectQty { get; set; }
        // DateTime properties for datetime fields
        public DateTime MachineStartTime { get; set; }
        public DateTime MachineEndTime { get; set; }
        public string MachineName { get; set; }
    }

    public enum AupdPlatingProcessStatus
    {
        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "On Hold")]
        OnHold
    }
}

