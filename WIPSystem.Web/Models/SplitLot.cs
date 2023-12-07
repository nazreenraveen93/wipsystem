namespace WIPSystem.Web.Models
{
    public class SplitLot
    {
        public int SplitLotId { get; set; }
        public string LotNo { get; set; }
        public int Quantity { get; set; }
        public string OriginalLot { get; set; }
        public string SplitSuffix { get; set; }
        public string EmpNo { get; set; }
        public DateTime Date { get; set; }
        public string PartNo { get; set; }
        public string Camber { get; set; }

        // Navigation property to SplitDetail
        public virtual ICollection<SplitDetail> SplitDetails { get; set; }
    }
}
