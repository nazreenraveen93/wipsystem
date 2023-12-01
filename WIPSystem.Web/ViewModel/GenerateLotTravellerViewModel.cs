
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WIPSystem.Web.Models;

namespace WIPSystem.Web.ViewModel
{
    public class GenerateLotTravellerViewModel
    {
        [Display(Name = "Select Part Number")]
        [Required]
        public int SelectedProductId { get; set; } // This represents the selected product ID

        public SelectList ProductSelectList { get; set; } // This is the list of products for the dropdown

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Display(Name = "Lot Number")]
        [Required]
        public string LotNo { get; set; }

        [Display(Name = "Date")]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Process Flow")]
        public IEnumerable<ProcessViewModel> Sequence { get; set; }
        // Add any other properties required by the form
    }
}
