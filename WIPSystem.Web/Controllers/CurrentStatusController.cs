using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WIPSystem.Web.Data;
using WIPSystem.Web.Services;


namespace WIPSystem.Web.Controllers
{
    public class CurrentStatusController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;
        private readonly ILogger<CurrentStatusController> _logger;
       

        public CurrentStatusController(
         WIPDbContext wIPDbContext,
         ILogger<CurrentStatusController> logger)
        {
            _wIPDbContext = wIPDbContext;
            _logger = logger;
            
        }

        public ActionResult Index()
        {
            var currentStatuses = _wIPDbContext.CurrentStatus.Include(cs => cs.Product).ToList();

            foreach (var status in currentStatuses)
            {
                // Get the ProcessId from the Process table where the name matches the ProcessStatus
                var currentProcessId = _wIPDbContext.Process
                                                    .Where(p => p.ProcessName == status.ProcessCurrentStatus)
                                                    .Select(p => p.ProcessId)
                                                    .FirstOrDefault();

                // Assuming ProductProcessMappings contains ProcessId, get the current sequence
                var currentSequence = _wIPDbContext.ProductProcessMappings
                                                   .Where(ppm => ppm.ProductId == status.ProductId && ppm.ProcessId == currentProcessId)
                                                   .Select(ppm => ppm.Sequence)
                                                   .FirstOrDefault();

                // Determine the next sequence
                var nextSequence = currentSequence + 1;

                // Get the next process mapping
                var nextProcessMapping = _wIPDbContext.ProductProcessMappings
                                                      .FirstOrDefault(ppm => ppm.ProductId == status.ProductId && ppm.Sequence == nextSequence);

                if (nextProcessMapping != null)
                {
                    // Get the ProcessName from the Process table
                    var nextProcessName = _wIPDbContext.Process
                                                       .Where(p => p.ProcessId == nextProcessMapping.ProcessId)
                                                       .Select(p => p.ProcessName)
                                                       .FirstOrDefault();

                    // Update the NextProcess in CurrentStatus
                    status.NextProcess = nextProcessName ?? "End of Process";
                }
                else
                {
                    // Handle the case where there is no next process
                    status.NextProcess = "End of Process";
                }
            }

            _wIPDbContext.SaveChanges();

            return View(currentStatuses);
        }


        public ActionResult ProcessNext(int id)
        {
            var currentStatus = _wIPDbContext.CurrentStatus.FirstOrDefault(cs => cs.CurrentStatusId == id);
            if (currentStatus == null)
            {
                return NotFound(); // Record not found
            }

            // Update the ProcessStatus, Status, and Date
            currentStatus.ProcessCurrentStatus = currentStatus.NextProcess;
            currentStatus.Status = "In Progress";
            currentStatus.Date = DateTime.Now;
            _wIPDbContext.SaveChanges();

            // Assuming PartNo is an integer in string format
            var nextProcessName = GetNextProcessNameForPartNo(currentStatus.PartNo);

            // Redirect to the view that shows the next process
            // Adjust redirection as necessary based on nextProcessName being available
            return RedirectToAction("ProcessDetails", new { partNo = currentStatus.PartNo, nextProcess = nextProcessName });
        }

        private string GetNextProcessNameForPartNo(string partNo)
        {
            if (!int.TryParse(partNo, out int productId))
            {
                _logger.LogError("Failed to convert PartNo to integer: PartNo={PartNo}", partNo);
                return null;
            }

            // Assuming that the ProcessStatus in CurrentStatus holds the current sequence number for the partNo
            var currentSequence = _wIPDbContext.CurrentStatus
                                               .Where(cs => cs.PartNo == partNo)
                                               .Select(cs => cs.ProcessCurrentStatus)
                                               .FirstOrDefault();

            if (!int.TryParse(currentSequence, out int currentSequenceInt))
            {
                _logger.LogError("Failed to convert current sequence to integer: CurrentSequence={CurrentSequence}", currentSequence);
                return null;
            }

            var maxSequenceForProduct = _wIPDbContext.ProductProcessMappings
                                                     .Where(ppm => ppm.ProductId == productId)
                                                     .Max(ppm => (int?)ppm.Sequence);

            if (!maxSequenceForProduct.HasValue || currentSequenceInt >= maxSequenceForProduct.Value)
            {
                // This means we are at the last process or an invalid sequence number was provided.
                return "End of Process";
            }

            var nextSequence = currentSequenceInt + 1;

            var nextProcessId = _wIPDbContext.ProductProcessMappings
                                             .Where(ppm => ppm.ProductId == productId && ppm.Sequence == nextSequence)
                                             .Select(ppm => ppm.ProcessId)
                                             .FirstOrDefault();

            var nextProcessName = _wIPDbContext.Process
                                               .Where(p => p.ProcessId == nextProcessId)
                                               .Select(p => p.ProcessName)
                                               .FirstOrDefault();

            return nextProcessName ?? "End of Process";
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var currentStatus = await _wIPDbContext.CurrentStatus.FindAsync(id);
            if (currentStatus == null)
            {
                return Json(new { success = false, message = "Status not found." });
            }
            currentStatus.ProcessCurrentStatus = currentStatus.NextProcess;
            currentStatus.Status = "In Progress";
            currentStatus.Date = DateTime.Now; // Set the date to current

            try
            {
                await _wIPDbContext.SaveChangesAsync();
                var redirectUrl = Url.Action("ProcessDetails", new { partNo = currentStatus.PartNo }); // Or whatever your logic for redirection is
                return Json(new { success = true, redirectUrl = redirectUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for ID: {ID}", id);
                return Json(new { success = false, message = "Error updating status." });
            }
        }


    }
}