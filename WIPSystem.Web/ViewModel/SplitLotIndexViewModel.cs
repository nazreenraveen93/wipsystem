namespace WIPSystem.Web.ViewModel
{
    public class SplitLotIndexViewModel
    {
        public int SplitLotId { get; set; } // Add this line
        public int Id { get; set; }
        public string EmpNo { get; set; }
        public string PartNo { get; set; }
        public string OriginalLot { get; set; }
        public IEnumerable<SplitDetailViewModel> SplitDetails { get; set; } // Replaces SplitLots
        public int Quantity { get; set; }
        public string Camber { get; set; }
        public DateTime Date { get; set; }
        public string ProcessName { get; set; }
        // Any other properties you need to display
    }

   
}
