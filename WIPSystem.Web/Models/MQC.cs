using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class MQC
    {
        // Assuming MqcId is an auto-incrementing primary key
        public int MqcId { get; set; }

        // String properties for the varchar fields
        public string PartNo { get; set; }
        public string LotNo { get; set; }

        // DateTime properties for date and datetime fields
        public DateTime Date { get; set; }
        public string ProcessCurrentStatus { get; set; } = "MQC"; // Default status
        public string CheckedBy { get; set; }
        public MQCProcessStatus Status { get; set; }

        // For the Remarks text column, you can still use string type as it can hold large texts
        public string Remarks { get; set; }

        // Integer properties for int fields
        public int ProductId { get; set; }

        public int OutputQty { get; set; }
        public int RejectQty { get; set; }
        public int InputQty { get; set; }

        public BlankOrPcsEnum BlankOrPcs { get; set; }
        public PhysicalChkEnum PhysicalChk { get; set; }
        public JudgmentEnum Judgment { get; set; }
        public int ScatterCheck { get; set; }
        public string VisualSampling { get; set; }
        public string Defects { get; set; }


    }

    public enum MQCProcessStatus
    {
        [Display(Name = "Completed")]
        Completed,

        [Display(Name = "On Hold")]
        OnHold
    }
    public enum BlankOrPcsEnum
    {
        [Display(Name = "Blank")]
        Blank,

        [Display(Name = "Pcs")]
        Pcs
    }

    public enum PhysicalChkEnum
    {
        [Display(Name = "Accept")]
        Accept,

        [Display(Name = "Reject")]
        Reject
    }

    public enum JudgmentEnum
    {
        [Display(Name = "Accept")]
        Accept,

        [Display(Name = "Reject")]
        Reject
    }
}

