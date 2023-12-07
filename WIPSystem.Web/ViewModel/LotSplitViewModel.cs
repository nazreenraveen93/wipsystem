using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WIPSystem.Web.Models;

namespace WIPSystem.Web.ViewModel
{
    public class LotSplitViewModel
    {
        public string EmpNo { get; set; }
        public string PartNo { get; set; }
        public string OriginalLot { get; set; }
        public int Quantity { get; set; }
        public int SplitSuffix { get; set; }

        public List<SplitDetailViewModel> SplitDetails { get; set; } = new List<SplitDetailViewModel>();
       
    }

    public class SplitDetailViewModel
    {
        public int Quantity { get; set; }
        public string LotNumber { get; set; }
        public string Camber { get; set; }
    }

}
