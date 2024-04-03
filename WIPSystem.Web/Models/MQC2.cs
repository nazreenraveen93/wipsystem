using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class MQC2
    {
        // Assuming MqcId is an auto-incrementing primary key
        public int MQC2Id { get; set; }

        // String properties for the varchar fields
        public string PartNo { get; set; }
        public string LotNo { get; set; }

        // DateTime properties for date and datetime fields
        public DateTime Date { get; set; }
        public string ProcessCurrentStatus { get; set; } = "MQC2"; // Default status
        public string CheckedBy { get; set; }
        public MQC2ProcessStatus Status { get; set; }

        // For the Remarks text column, you can still use string type as it can hold large texts
        public string Remarks { get; set; }

        // Integer properties for int fields
        public int ProductId { get; set; }

        public int OutputQty { get; set; }
        public int RejectQty { get; set; }
        public int InputQty { get; set; }

    }

    public enum MQC2ProcessStatus
    {
        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "On Hold")]
        OnHold
    }
   
}

