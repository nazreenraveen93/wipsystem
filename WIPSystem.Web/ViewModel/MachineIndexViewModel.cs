namespace WIPSystem.Web.ViewModel
{
    public class MachineIndexViewModel
    {
        public int MachineId { get; set; }
        public string ProcessName { get; set; }
        public string MachineName { get; set; }
        public string JigName { get; set; }
        public int JigLifeSpan { get; set; }
        public int CurrentUsage { get; set; } // Add this line
        public string JigRemarks { get; set; }
    }

}
