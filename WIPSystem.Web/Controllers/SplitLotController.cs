using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WIPSystem.Web.Data;
using WIPSystem.Web.Models;
using WIPSystem.Web.ViewModel;

namespace WIPSystem.Web.Controllers
{
    public class SplitLotController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;

        public SplitLotController(WIPDbContext wIPDbContext)
        {
            _wIPDbContext = wIPDbContext;

        }

        public IActionResult Index()
        {
            var viewModelList = _wIPDbContext.SplitLot
                .AsNoTracking()
                .Select(splitLot => new SplitLotIndexViewModel
                {
                    Id = splitLot.SplitLotId,
                    EmpNo = splitLot.EmpNo,
                    PartNo = splitLot.PartNo,
                    OriginalLot = splitLot.OriginalLot,
                    Quantity = splitLot.Quantity,
                    Camber = splitLot.Camber,
                    Date = DateTime.Now, // This sets the current date and time
                    SplitDetails = _wIPDbContext.SplitDetails
                                    .Where(detail => detail.SplitLotId == splitLot.SplitLotId)
                                    .Select(detail => new SplitDetailViewModel
                                    {
                                        LotNumber = detail.LotNumber,
                                        Quantity = detail.Quantity,
                                        Camber = detail.Camber
                                    })
                                    .ToList()
                }).ToList();

            return View(viewModelList);
        }


        [HttpGet]
        public IActionResult SplitLot()
        {
            var viewModel = new LotSplitViewModel();
            // Populate viewModel if necessary
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SplitLot(LotSplitViewModel model)
        {
            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                // Repopulate any necessary data for the form
                // Return the view with the current model to display any validation errors
                return View(model);
            }

            // Process the form data
            try
            {
                // Create a new SplitLot entity from the model data
                var splitLot = new SplitLot
                {
                    EmpNo = model.EmpNo,
                    PartNo = model.PartNo,
                    OriginalLot = model.OriginalLot,
                    Quantity = model.Quantity,
                    // Set other properties as needed
                };

                // Add the new SplitLot to the database context
                _wIPDbContext.SplitLot.Add(splitLot);
                // Save the SplitLot to get its ID for the foreign key in SplitDetail
                _wIPDbContext.SaveChanges();

                // Process each SplitDetail
                foreach (var detail in model.SplitDetails)
                {
                    var splitDetail = new SplitDetail
                    {
                        SplitLotId = splitLot.SplitLotId,
                        Quantity = detail.Quantity,
                        LotNumber = detail.LotNumber,
                        Camber = detail.Camber,
                        // Set other properties as needed
                    };

                    // Add each SplitDetail to the database context
                    _wIPDbContext.SplitDetails.Add(splitDetail);
                }

                // Save the changes to the database
                _wIPDbContext.SaveChanges();

                // Redirect to a confirmation page or back to the index
                return RedirectToAction("Index"); // Replace "Index" with your success action
            }
            catch (Exception ex)
            {
                // Log the exception (use your logging mechanism here)
                ModelState.AddModelError("", "An error occurred while processing your request.");

                // Return the view with the current model to display the error message
                return View(model);
            }
        }

    }
}