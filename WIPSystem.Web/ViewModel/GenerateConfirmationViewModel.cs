namespace WIPSystem.Web.ViewModel
{
    public class GenerateConfirmationViewModel
    {
        public int LotTravellerId { get; set; } // Assuming the ID is of type int
        public string QRCodeImage { get; set; }
        public string ConfirmationMessage { get; set; }
        public List<ProcessStepViewModel> ProcessSteps { get; set; }

        // Add new properties here
        public string PartNo { get; set; }
        public string CustName { get; set; }
        public string LotNo { get; set; }
        public string PackageSize { get; set; }
        public int PiecePerBlank { get; set; }
        // New property for the PDF download URL
        public string PdfDownloadUrl { get; set; }
    }


    public class ProcessStepViewModel
    {
        public int ProcessStepId { get; set; } // If you have a specific ID for the process step
        public int ProcessId { get; set; }
        public string Sequence { get; set; }
        public string ProcessName { get; set; }
        public int ProcessCode { get; set; } // Add this line
        // Add other properties for schedule quantity, result date, etc., as needed.
    }
}
