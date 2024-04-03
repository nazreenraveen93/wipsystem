namespace WIPSystem.Web.ViewModel
{
    public class CurrentStatusViewModel
    {
        public DateTime Date { get; set; }
        public string PartNo { get; set; }
        public string LotNo { get; set; }
        public int ReceivedQuantity { get; set; }
        public string ProcessStatus { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public string PIC { get; set; }
        public string NextProcess { get; set; }
    }

}
