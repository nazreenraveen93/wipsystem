using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WIPSystem.Web.Data;
using WIPSystem.Web.Models;
using WIPSystem.Web.ViewModel;
using WIPSystem.Web.ViewModels;

namespace WIPSystem.Web.Controllers
{
    public class LotTravellerController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;

        public LotTravellerController(WIPDbContext wIPDbContext)
        {
            _wIPDbContext = wIPDbContext;
        }

        public IActionResult Index()
        {
            var lotTravellers = _wIPDbContext.LotTravellers
                .Include(lt => lt.Product)
                    .ThenInclude(p => p.ProductProcessMappings)
                        .ThenInclude(ppm => ppm.Process)
                .ToList();

            // Now you can pass lotTravellers to the view or further process it
            return View(lotTravellers);
        }

        public ActionResult Generate()
        {
            var viewModel = new GenerateLotTravellerViewModel
            {
                ProductSelectList = new SelectList(_wIPDbContext.Products, "ProductId", "PartNo")
                // Initialize other properties as needed
            };
            return View(viewModel);
        }
        public JsonResult GetCustomerName(int productId)
        {
            // Fetch the customer name based on the productId from your data source (e.g., database)
            string customerName = _wIPDbContext.Products
                .Where(p => p.ProductId == productId)
                .Select(p => p.CustName)
                .FirstOrDefault();

            return Json(customerName);
        }



    }

}
