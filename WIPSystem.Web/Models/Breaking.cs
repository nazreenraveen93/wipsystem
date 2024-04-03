using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class Breaking
    {
        // Assuming SinterId is an auto-incrementing primary key
        public int BreakingId { get; set; }

        // String properties for the varchar fields
        public string PartNo { get; set; }
        public string LotNo { get; set; }
        // DateTime properties for date and datetime fields
        public DateTime Date { get; set; }
        public string ProcessCurrentStatus { get; set; } = "Breaking"; // Default status
        public string CheckedBy { get; set; }
        public BreakingProcessStatus Status { get; set; }
        // For the Remarks text column, you can still use string type as it can hold large texts
        public string Remarks { get; set; }
        // Integer properties for int fields
        public int ProductId { get; set; }
        public int MachineId { get; set; }
        public int OutputQty { get; set; }
        public int RejectQty { get; set; }
        public int ChippingQty { get; set; }
        public int SheetBreakQty { get; set; }
        public int CrackQty { get; set; }
        public int OthersQty { get; set; }
        public int DifferencesQty { get; set; }
        public string MachineType { get; set; } // Add MachineType property
        // DateTime properties for datetime fields
        public DateTime MachineStartTime { get; set; }
        public DateTime MachineEndTime { get; set; }
        public string FirstBreak { get; set; }
        public string SecondBreak { get; set; }
        public string TargetOne { get; set; }
        public string TargetTwo { get; set; }
        public string MachineName { get; set; }
    }

    public enum BreakingProcessStatus
    {
        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "On Hold")]
        OnHold
    }
}
