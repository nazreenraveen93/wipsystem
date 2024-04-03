using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class NiPlating2
    {

        // Assuming SinterId is an auto-incrementing primary key
        public int NiPlating2Id { get; set; }

        // String properties for the varchar fields
        public string PartNo { get; set; }
        public string LotNo { get; set; }

        // DateTime properties for date and datetime fields
        public DateTime Date { get; set; }
        public string ProcessCurrentStatus { get; set; } = "Ni Plating"; // Default status
        public string CheckedBy { get; set; }
        public NiPlating2ProcessStatus Status { get; set; }

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

    public enum NiPlating2ProcessStatus
    {
        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "On Hold")]
        OnHold
    }
}

