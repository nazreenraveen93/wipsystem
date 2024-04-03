using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WIPSystem.Web.Data;
using WIPSystem.Web.Models;
using WIPSystem.Web.ViewModel;
using Microsoft.Data.SqlClient;
using WIPSystem.Web.ViewModels;

namespace WIPSystem.Web.Controllers
{
    public class SplitLotController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;
        private readonly ILogger<SplitLotController> _logger;

        public SplitLotController(WIPDbContext wIPDbContext, ILogger<SplitLotController> logger)
        {
            _wIPDbContext = wIPDbContext;
            _logger = logger;

        }

        public IActionResult Index()
        {
            var viewModelList = _wIPDbContext.SplitLot
                .AsNoTracking()
                .Select(splitLot => new SplitLotIndexViewModel
                {
                    SplitLotId = splitLot.SplitLotId,
                    EmpNo = splitLot.EmpNo,
                    PartNo = splitLot.PartNo,
                    OriginalLot = splitLot.OriginalLot,
                    Quantity = splitLot.Quantity,
                    Camber = splitLot.Camber,
                    Date = splitLot.Date,

                // Fetch the ProcessName associated with the SplitLot
                ProcessName = _wIPDbContext.Process
                .Where(process => process.ProcessId == splitLot.ProcessId)
                .Select(process => process.ProcessName)
                .FirstOrDefault(),

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

            // Fetch part numbers from the database
            viewModel.PartNumbers = _wIPDbContext.Products
                .Select(p => new SelectListItem { Value = p.PartNo, Text = p.PartNo })
                .ToList();

            // Fetch process names from the database and populate the dropdown list
            viewModel.Processes = _wIPDbContext.Process
                .Select(process => new SelectListItem { Value = process.ProcessId.ToString(), Text = process.ProcessName })
                .ToList();

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SplitLot(LotSplitViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate PartNumbers if validation fails
                model.PartNumbers = _wIPDbContext.Products
                    .Select(p => new SelectListItem { Value = p.PartNo, Text = p.PartNo })
                    .ToList();

                model.Processes = _wIPDbContext.Process
                  .Select(process => new SelectListItem { Value = process.ProcessId.ToString(), Text = process.ProcessName })
                  .ToList();

                return View(model);
            }

            // Process the form data
            try
            {
                // Create a new SplitLot entity from the model data
                var splitLot = new SplitLot
                {
                    EmpNo = model.EmpNo,
                    PartNo = model.SelectedPartNo, // Use SelectedPartNo from the ViewModel
                    OriginalLot = model.OriginalLot,
                    Quantity = model.Quantity,
                    Date = DateTime.Now, // Set the current date
                           // Set other properties as needed
                };

                // Add the new SplitLot to the database context
                _wIPDbContext.SplitLot.Add(splitLot); // Ensure the DbSet name matches your entity name                
                _wIPDbContext.SaveChanges();

                // Retrieve the ID of the newly inserted SplitLot
                int splitLotId = splitLot.SplitLotId;

                // Execute raw SQL command to insert the ProcessId into the database
                string sql = "UPDATE SplitLot SET ProcessId = @ProcessId WHERE SplitLotId = @SplitLotId";
                _wIPDbContext.Database.ExecuteSqlRaw(sql,
                    new SqlParameter("@ProcessId", model.SelectedProcessId),
                    new SqlParameter("@SplitLotId", splitLotId));

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
                    _wIPDbContext.SplitDetails.Add(splitDetail); // Ensure the DbSet name matches your entity name
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

        [HttpGet]
        public IActionResult Edit(int SplitLotId)
        {
            var splitLot = _wIPDbContext.SplitLot
                .Include(s => s.SplitDetails) // Include the related SplitDetails
                .FirstOrDefault(s => s.SplitLotId == SplitLotId);

            if (splitLot == null)
            {
                return NotFound(); // Or handle the not found scenario as needed
            }

            // Fetch process name based on the process ID
            var processName = _wIPDbContext.Process
                .Where(p => p.ProcessId == splitLot.ProcessId)
                .Select(p => p.ProcessName)
                .FirstOrDefault();

            var viewModel = new LotSplitViewModel
            {
                SplitLotId = splitLot.SplitLotId,
                EmpNo = splitLot.EmpNo,
                SelectedPartNo = splitLot.PartNo,
                OriginalLot = splitLot.OriginalLot,
                Quantity = splitLot.Quantity,
                SelectedProcessId = splitLot.ProcessId,
                ProcessName = processName, // Assign the process name
                UpdatedBy = "Admin", // Replace with actual logic
               
                SplitDetails = splitLot.SplitDetails.Select(detail => new SplitDetailViewModel
                {
                    SplitDetailId = detail.SplitDetailId,
                    LotNumber = detail.LotNumber,
                    Quantity = detail.Quantity,
                    Camber = detail.Camber
                }).ToList()
            };

           

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(LotSplitViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var splitLot = _wIPDbContext.SplitLot
                .Include(s => s.SplitDetails)
                .FirstOrDefault(s => s.SplitLotId == model.SplitLotId);

            if (splitLot == null)
            {
                return NotFound(); // Or handle the not found scenario as needed
            }

          
            try
            {
                // Update the SplitLot properties
                splitLot.EmpNo = model.EmpNo;
                splitLot.PartNo = model.SelectedPartNo;
                splitLot.OriginalLot = model.OriginalLot;
                splitLot.Quantity = model.Quantity;
                splitLot.ProcessId = model.SelectedProcessId;
              


                // Update SplitDetails
                foreach (var detailViewModel in model.SplitDetails)
                {
                    var detail = splitLot.SplitDetails.FirstOrDefault(d => d.SplitDetailId == detailViewModel.SplitDetailId);
                    if (detail != null)
                    {
                        detail.LotNumber = detailViewModel.LotNumber;
                        detail.Quantity = detailViewModel.Quantity;
                        detail.Camber = detailViewModel.Camber;
                    }
                }

                _wIPDbContext.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError("", "An error occurred while updating the record.");
                return View(model);
            }
        }

        public IActionResult Details(int id)
        {
            var splitLot = _wIPDbContext.SplitLot
                .Include(s => s.SplitDetails) // Include the related SplitDetails
                .FirstOrDefault(s => s.SplitLotId == id);

            if (splitLot == null)
            {
                return NotFound(); // Or handle the not found scenario as needed
            }

            // Fetch the ProcessName associated with the SplitLot
            var processName = _wIPDbContext.Process
                .Where(process => process.ProcessId == splitLot.ProcessId)
                .Select(process => process.ProcessName)
                .FirstOrDefault();

            var viewModel = new LotSplitViewModel
            {
                SplitLotId = splitLot.SplitLotId,
                EmpNo = splitLot.EmpNo,
                SelectedPartNo = splitLot.PartNo,
                OriginalLot = splitLot.OriginalLot,
                Quantity = splitLot.Quantity,
                SelectedProcessId = splitLot.ProcessId,
                ProcessName = processName, // Assign the process name

                SplitDetails = splitLot.SplitDetails.Select(detail => new SplitDetailViewModel
                {
                   
                    LotNumber = detail.LotNumber,
                    Quantity = detail.Quantity,
                    Camber = detail.Camber
                }).ToList()
            };

            // Return the PartialView instead of a full view
            return PartialView("_DetailsModal", viewModel);
        }

        // DELETE action for SplitLot
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int splitLotId)
        {
            try
            {
                var splitLot = _wIPDbContext.SplitLot.FirstOrDefault(sl => sl.SplitLotId == splitLotId);

                if (splitLot == null)
                {
                    return Json(new { success = false, message = "Item not found or already deleted." });
                }

                _wIPDbContext.SplitLot.Remove(splitLot);
                _wIPDbContext.SaveChanges();
                _logger.LogInformation($"Deleted SplitLot with ID {splitLotId}.");

                var redirectUrl = Url.Action("Index", "SplitLot"); // Change 'YourController' to the actual controller name
                return Json(new { success = true, redirectUrl = redirectUrl });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Optimistic concurrency failure: attempted to delete a row that no longer exists.");
                return Json(new { success = false, message = "The record already has been deleted by another user." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting SplitLot with ID {ID}", splitLotId);
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}