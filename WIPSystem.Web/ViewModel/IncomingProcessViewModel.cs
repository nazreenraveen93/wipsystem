using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using WIPSystem.Web.Models;

namespace WIPSystem.Web.ViewModel
{
    public class IncomingProcessViewModel
    {
        public int IncomingProcessId { get; set; }
        public string PartNo { get; set; }
        public string CustomerName { get; set; }
        public int PiecePerBlank { get; set; }
        public string PackageSize { get; set; }
        public string LotNo { get; set; }
        public int ReceivedQuantity { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Remarks { get; set; }
        public IncomingProcessStatus Status { get; set; }
        public string CheckedBy { get; set; }
        public string ProcessCurrentStatus { get; set; }
        public int ProductId { get; set; }
        // Dropdown lists
        public List<SelectListItem> PartNos { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> LotNos { get; set; } = new List<SelectListItem>();
    }
}
