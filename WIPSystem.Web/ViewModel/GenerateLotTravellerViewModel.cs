
using Microsoft.AspNetCore.Mvc.Rendering;
using WIPSystem.Web.Models;

namespace WIPSystem.Web.ViewModel
{
    public class GenerateLotTravellerViewModel
    {
        public int SelectedProductId { get; set; } // This represents the selected product ID
        public SelectList ProductSelectList { get; set; } // This is the list of products for the dropdown
        public string CustomerName { get; set; }
        public string LotNo { get; set; }
        public DateTime DateCreated { get; set; }
        public IEnumerable<ProductProcessMapping> ProcessSequence { get; set; }
        // Add any other properties required by the form
    }
}
