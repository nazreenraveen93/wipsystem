using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WIPSystem.Web.Data;
using WIPSystem.Web.Models;
using WIPSystem.Web.ViewModel;

namespace WIPSystem.Web.Controllers
{
    public class IncomingController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;
        private readonly ILogger<IncomingController> _logger;

        public IncomingController(
            WIPDbContext wIPDbContext,
            ILogger<IncomingController> logger)
        {
            _wIPDbContext = wIPDbContext;
            _logger = logger;

        }

        public IActionResult Index()
        {
            var incomingProcesses = _wIPDbContext.IncomingProcesses.Select(ip => new IncomingProcessIndexViewModel
            {
                IncomingProcessId = ip.IncomingProcessId,
                PartNo = ip.PartNo,
                LotNo = ip.LotNo,
                Date = ip.Date,
                ReceivedQuantity = ip.ReceivedQuantity,
                CheckedBy = ip.CheckedBy,
                Remarks = ip.Remarks,
                Status = ip.Status
                // Map other properties as needed
            }).ToList();

            // Determine if the current user is an admin or super admin
            // This assumes you have some way to check the user's role, adjust as needed for your setup
            bool isUserAdmin = User.IsInRole("Admin") || User.IsInRole("Super Admin");
            ViewData["IsUserAdmin"] = isUserAdmin;

            return View(incomingProcesses);
        }

        private List<SelectListItem> GetPartNos()
        {
            var partNos = new List<SelectListItem>();
            foreach (var part in _wIPDbContext.Products)
            {
                partNos.Add(new SelectListItem { Value = part.PartNo, Text = part.PartNo });
            }
            return partNos;
        }

        private List<SelectListItem> GetLotNos()
        {
            var lotNos = new List<SelectListItem>();
            foreach (var lot in _wIPDbContext.LotTravellers)
            {
                lotNos.Add(new SelectListItem { Value = lot.LotNo, Text = lot.LotNo });
            }
            return lotNos;
        }

        [HttpGet]
        public IActionResult CreateIncomingProcess()
        {
            var model = new IncomingProcessViewModel();
            model.PartNos = GetPartNos(); // Populate dropdown data as needed
            model.LotNos = GetLotNos();   // Populate dropdown data as needed
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIncomingProcess(IncomingProcessViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.PartNos = await GetPartNosAsync();
                model.LotNos = await GetLotNosAsync();
                return View(model);
            }

            using (var transaction = _wIPDbContext.Database.BeginTransaction())
            {
                try
                {
                    var product = await _wIPDbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.PartNo == model.PartNo);
                    if (product == null)
                    {
                        ModelState.AddModelError("", "Product not found for the given PartNo.");
                        model.PartNos = await GetPartNosAsync();
                        model.LotNos = await GetLotNosAsync();
                        return View(model);
                    }

                    var newIncomingProcess = new IncomingProcess
                    {
                        PartNo = model.PartNo,
                        ProductId = product.ProductId,
                        CustomerName = model.CustomerName,
                        PiecePerBlank = model.PiecePerBlank,
                        PackageSize = model.PackageSize,
                        LotNo = model.LotNo,
                        ReceivedQuantity = model.ReceivedQuantity,
                        Status = model.Status,
                        CheckedBy = User.Identity.Name, // Ensure the user's identity is correctly managed
                        Date = DateTime.UtcNow, // Consider using UTC for consistency across time zones
                        Remarks = model.Remarks
                    };

                    _wIPDbContext.IncomingProcesses.Add(newIncomingProcess);
                    await _wIPDbContext.SaveChangesAsync();

                    var nextProcess = DetermineNextProcess(model.PartNo); // This method needs to determine the next process based on your logic

                    var newCurrentStatus = new CurrentStatus
                    {
                        PartNo = model.PartNo,
                        ProductId = product.ProductId,
                        LotNo = model.LotNo,
                        ProcessCurrentStatus = "Incoming", // Assuming this is the default status for a new process
                        ReceivedQuantity = model.ReceivedQuantity,
                        Status = model.Status.ToString(),
                        Remarks = model.Remarks,
                        PIC = User.Identity.Name,
                        Date = DateTime.UtcNow, // Consider using UTC
                        NextProcess = nextProcess
                    };

                    _wIPDbContext.CurrentStatus.Add(newCurrentStatus);
                    await _wIPDbContext.SaveChangesAsync();
                    _logger.LogInformation($"New CurrentStatusId: {newCurrentStatus.CurrentStatusId}");

                    if (newCurrentStatus.CurrentStatusId == 0)
                    {
                        // If CurrentStatusId is 0 after SaveChangesAsync, it indicates a problem.
                        _logger.LogError("CurrentStatusId was not set by the database.");
                        // Handle the error appropriately here.
                        // You might want to throw an exception or handle it in another way.
                    }



                    await transaction.CommitAsync();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while creating the incoming process.");
                    ModelState.AddModelError("", "An error occurred while creating the process.");

                    model.PartNos = await GetPartNosAsync();
                    model.LotNos = await GetLotNosAsync();
                    return View(model);
                }
            }
        }

        private async Task<List<SelectListItem>> GetPartNosAsync()
        {
            return await _wIPDbContext.Products
                .Select(p => new SelectListItem { Value = p.PartNo, Text = p.PartNo })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetLotNosAsync()
        {
            return await _wIPDbContext.LotTravellers
                .Select(l => new SelectListItem { Value = l.LotNo, Text = l.LotNo })
                .ToListAsync();
        }

        private string DetermineNextProcess(string partNo)
        {
            // Implement your logic to determine the next process
            // This is a placeholder for your business logic
            return "NextProcessName"; // This should be replaced with actual logic to determine the next process
        }


        private string DetermineNextProcess(string partNo, string currentProcessName)
        {
            var productId = _wIPDbContext.Products
                            .FirstOrDefault(p => p.PartNo == partNo)?.ProductId ?? 0;

            if (productId == 0)
            {
                _logger.LogError($"Product not found for PartNo: {partNo}");
                return "Invalid Product";
            }

            // Retrieve the current process based on its name
            var currentProcess = _wIPDbContext.Process
                                .FirstOrDefault(proc => proc.ProcessName == currentProcessName);

            if (currentProcess == null)
            {
                _logger.LogError($"Process not found: {currentProcessName}");
                return "Process Not Found";
            }

            // Get the current sequence from the ProductProcessMappings
            var currentSequence = _wIPDbContext.ProductProcessMappings
                                .Where(ppm => ppm.ProductId == productId && ppm.ProcessId == currentProcess.ProcessId)
                                .Select(ppm => ppm.Sequence)
                                .FirstOrDefault();

            if (currentSequence == 0)
            {
                _logger.LogError($"Process sequence not found for ProductId: {productId}, ProcessName: {currentProcessName}");
                return "Sequence Not Found";
            }

            // Determine the next ProcessId based on the current sequence
            var nextProcessId = _wIPDbContext.ProductProcessMappings
                                .Where(ppm => ppm.ProductId == productId && ppm.Sequence == currentSequence + 1)
                                .Select(ppm => ppm.ProcessId)
                                .FirstOrDefault();

            if (nextProcessId == 0)
            {
                return "End of Process";
            }

            // Retrieve the next process name using the next ProcessId
            var nextProcessName = _wIPDbContext.Process
                                .Where(proc => proc.ProcessId == nextProcessId)
                                .Select(proc => proc.ProcessName)
                                .FirstOrDefault();

            return nextProcessName ?? "End of Process";
        }


        public IActionResult GetProductDetails(string partNo)
        {
            var product = _wIPDbContext.Products.FirstOrDefault(p => p.PartNo == partNo);

            if (product == null)
            {
                return Json(new { error = "Product not found" });
            }

            // Return product details as JSON
            return Json(new
            {
                customerName = product.CustName,
                packageSize = product.PackageSize,
                piecePerBlank = product.PiecesPerBlank
            });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Fetch the incoming process that matches the given id
            var incomingProcess = _wIPDbContext.IncomingProcesses
                .FirstOrDefault(ip => ip.IncomingProcessId == id);

            if (incomingProcess == null)
            {
                // If no process is found, redirect to the NotFound page
                return NotFound();
            }

            // Create a new instance of IncomingProcessViewModel and populate it with data from the incoming process
            var viewModel = new IncomingProcessViewModel
            {
                IncomingProcessId = incomingProcess.IncomingProcessId,
                PartNo = incomingProcess.PartNo,
                CustomerName = incomingProcess.CustomerName,
                PiecePerBlank = incomingProcess.PiecePerBlank,
                PackageSize = incomingProcess.PackageSize,
                LotNo = incomingProcess.LotNo,
                ReceivedQuantity = incomingProcess.ReceivedQuantity,
                CheckedBy = User.Identity.Name,
                Remarks = incomingProcess.Remarks,
                Status = incomingProcess.Status,
                Date = DateTime.Now,
                PartNos = GetPartNos(), // Populate PartNos dropdown list
                LotNos = GetLotNos()    // Populate LotNos dropdown list
            };

            // Pass the view model to the view
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IncomingProcessViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.PartNos = await GetPartNosAsync(); // Method to get PartNos
                viewModel.LotNos = await GetLotNosAsync();   // Method to get LotNos
                return View(viewModel);
            }

            var incomingProcess = await _wIPDbContext.IncomingProcesses
                                        .FirstOrDefaultAsync(p => p.IncomingProcessId == viewModel.IncomingProcessId);

            if (incomingProcess == null)
            {
                return NotFound();
            }

            // Update the incomingProcess entity from the viewModel
            incomingProcess.PartNo = viewModel.PartNo;
            incomingProcess.CustomerName = viewModel.CustomerName;
            incomingProcess.PiecePerBlank = viewModel.PiecePerBlank;
            incomingProcess.PackageSize = viewModel.PackageSize;
            incomingProcess.LotNo = viewModel.LotNo;
            incomingProcess.ReceivedQuantity = viewModel.ReceivedQuantity;
            incomingProcess.Date = viewModel.Date;
            incomingProcess.Remarks = viewModel.Remarks;
            incomingProcess.Status = viewModel.Status;
            incomingProcess.CheckedBy = User.Identity.Name; // Ensure you're capturing the right user identifier

            // Attempt to find the related CurrentStatus record to update
            var currentStatus = await _wIPDbContext.CurrentStatus
                                 .FirstOrDefaultAsync(cs => cs.PartNo == viewModel.PartNo && cs.LotNo == viewModel.LotNo);

            if (currentStatus != null)
            {
                // If found, update the CurrentStatus entity as needed
                currentStatus.Status = viewModel.Status.ToString(); // Assuming this maps directly
                // Update other relevant fields from the viewModel
                currentStatus.ReceivedQuantity = viewModel.ReceivedQuantity;
                currentStatus.Remarks = viewModel.Remarks;
                currentStatus.PIC = User.Identity.Name;
                currentStatus.Date = DateTime.UtcNow;
            }
            else
            {
                // Log or handle the case where no related CurrentStatus is found
                _logger.LogWarning($"No CurrentStatus record found for PartNo: {viewModel.PartNo} and LotNo: {viewModel.LotNo}.");
            }

            try
            {
                await _wIPDbContext.SaveChangesAsync();
                return RedirectToAction("Index"); // Or wherever you need to redirect to after successful edit
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the incoming process.");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");

                // Repopulate dropdowns if there's an error and return to view
                viewModel.PartNos = await GetPartNosAsync();
                viewModel.LotNos = await GetLotNosAsync();
                return View(viewModel);
            }
        }

        // DELETE action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int IncomingProcessId)
        {
            // Check if the user is in the "Admin" or "SuperAdmin" role
            if (!User.IsInRole("Admin") && !User.IsInRole("Super Admin"))
            {
                // If the user is not in the required role, return a JSON response indicating unauthorized access
                return Json(new { success = false, message = "Unauthorized access." });
            }

            try
            {
                var incomingProcess = _wIPDbContext.IncomingProcesses
                                                   .FirstOrDefault(ip => ip.IncomingProcessId == IncomingProcessId);
                if (incomingProcess == null)
                {
                    return Json(new { success = false, message = "Item not found or already deleted." });
                }

                _wIPDbContext.IncomingProcesses.Remove(incomingProcess);
                _wIPDbContext.SaveChanges();
                _logger.LogInformation($"Deleted IncomingProcess with ID {IncomingProcessId}.");

                var redirectUrl = Url.Action("Index", "Incoming");
                return Json(new { success = true, redirectUrl = redirectUrl });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Optimistic concurrency failure: attempted to delete a row that no longer exists.");
                return Json(new { success = false, message = "The record already has been deleted by another user." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting IncomingProcess with ID {ID}", IncomingProcessId);
                return Json(new { success = false, message = ex.Message });
            }
        }


        public IActionResult Details(int id)
        {
            var incomingProcess = _wIPDbContext.IncomingProcesses.FirstOrDefault(ip => ip.IncomingProcessId == id);

            if (incomingProcess == null)
            {
                return NotFound();
            }

            // Create and populate the ViewModel
            var viewModel = new IncomingProcessViewModel
            {
                IncomingProcessId = incomingProcess.IncomingProcessId,
                PartNo = incomingProcess.PartNo,
                CustomerName = incomingProcess.CustomerName,
                PackageSize = incomingProcess.PackageSize,
                PiecePerBlank = incomingProcess.PiecePerBlank,
                LotNo = incomingProcess.LotNo,
                ReceivedQuantity = incomingProcess.ReceivedQuantity,
                Remarks = incomingProcess.Remarks,
                CheckedBy = incomingProcess.CheckedBy
            };

            // Return the PartialView instead of a full view
            return PartialView("_DetailsModal", viewModel);

        }
    }
}
