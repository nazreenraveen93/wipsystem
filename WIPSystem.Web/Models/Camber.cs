using System.ComponentModel.DataAnnotations;


namespace WIPSystem.Web.Models
{
    public class Camber
    {
        // Assuming SinterId is an auto-incrementing primary key
        public int CamberId { get; set; }

        // String properties for the varchar fields
        public string PartNo { get; set; }
        public string LotNo { get; set; }
        // DateTime properties for date and datetime fields
        public DateTime Date { get; set; }
        public string ProcessCurrentStatus { get; set; } = "Camber Selec"; // Default status
        public string CheckedBy { get; set; }
        public CamberProcessStatus Status { get; set; }
        // For the Remarks text column, you can still use string type as it can hold large texts
        public string Remarks { get; set; }

        // Integer properties for int fields
        public int ProductId { get; set; }
        public int MachineId { get; set; }

        public int InputQty { get; set; }
        public int ReceivedQuantity { get; set; }
        public decimal AvgThickness { get; set; }
        public decimal PieceWeight { get; set; }
        public string Range { get; set; }
        public int OutputQty { get; set; }
        public int MachineRejectQty { get; set; }
        public int CamberQty { get; set; }
        public decimal Yield { get; set; }
        public decimal RejectRate { get; set; }
        public int TotalQty { get; set; }
        public decimal TotalWeight { get; set; }

        // DateTime properties for datetime fields
        public DateTime MachineStartTime { get; set; }
        public DateTime MachineEndTime { get; set; }

        public string MachineName { get; set; }
    }

    public enum CamberProcessStatus
    {
        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "On Hold")]
        OnHold
    }
}

