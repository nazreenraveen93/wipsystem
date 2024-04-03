using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WIPSystem.Web.Data;
using WIPSystem.Web.Models;
using WIPSystem.Web.ViewModel;

namespace WIPSystem.Web.Controllers
{
    public class MachineController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;
        private readonly ILogger<MachineController> _logger;


        public MachineController(WIPDbContext wIPDbContext,
             ILogger<MachineController> logger)
        {
            _wIPDbContext = wIPDbContext;
            _logger = logger;

        }

        public IActionResult Index()
        {
            var machines = _wIPDbContext.Machines
            .Include(m => m.Jigs)
            .Select(m => new MachineIndexViewModel
            {
                MachineId = m.MachineId, // This should match the column name in your database
                MachineName = m.MachineName,
                ProcessName = m.Process.ProcessName,
                    
                        JigName = m.Jigs.FirstOrDefault() != null ? m.Jigs.FirstOrDefault().JigName : string.Empty,
                        JigLifeSpan = m.Jigs.FirstOrDefault() != null ? m.Jigs.FirstOrDefault().JigLifeSpan : 0
                    }).ToList();

            return View(machines);
        }


        public IActionResult CreateMachineAndJig()
        {
            var viewModel = new MachineJigViewModel
            {
                Processes = _wIPDbContext.Process
                            .Select(p => new SelectListItem
                            {
                                Value = p.ProcessId.ToString(),
                                Text = p.ProcessName
                            }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult CreateMachineAndJig(MachineJigViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Assuming you have Machine and Jig models that correspond to your database structure
                var newMachine = new Machine
                {
                    // Set properties from the form data
                    MachineName = model.MachineName,
                    MachineNumber = model.MachineNumber,
                    MachineQty = model.MachineQuantity,
                    // Assuming ProcessId is a foreign key in Machine
                    ProcessId = model.SelectedProcessId
                };

                var newJig = new Jig
                {
                    // Set properties from the form data
                    JigName = model.JigName,
                    JigLifeSpan = model.JigLifeSpan,
                    // Link this Jig to the Machine
                    Machine = newMachine
                };

                // Add the new Machine and Jig to the context
                _wIPDbContext.Machines.Add(newMachine);
                _wIPDbContext.Jigs.Add(newJig);

                // Save changes to the database
                _wIPDbContext.SaveChanges();

                // Redirect to a confirmation page or display a success message
                // Replace "SuccessAction" with the name of the action to redirect to
                return RedirectToAction("Index");
            }

            // If we got this far, something failed; redisplay the form
            // Reload Processes in case of failure
            model.Processes = _wIPDbContext.Process
                            .Select(p => new SelectListItem
                            {
                                Value = p.ProcessId.ToString(),
                                Text = p.ProcessName
                            }).ToList();
            return View(model);
        }

      
        public IActionResult Edit(int MachineId)
        {
            var machine = _wIPDbContext.Machines
                           .Include(m => m.Jigs)
                           .FirstOrDefault(m => m.MachineId == MachineId);

            if (machine == null)
            {
                return NotFound();
            }

            // Taking the first Jig for simplicity, adjust according to your needs
            var firstJig = machine.Jigs.FirstOrDefault();

            var viewModel = new MachineJigViewModel
            {
                MachineId = machine.MachineId, // Add this line to assign the MachineId
                MachineName = machine.MachineName,
                MachineNumber = machine.MachineNumber,
                MachineQuantity = machine.MachineQty,
                SelectedProcessId = machine.ProcessId,

                // Assign jig properties if a jig exists
                JigName = firstJig?.JigName,
                JigLifeSpan = firstJig?.JigLifeSpan ?? 0,

                Processes = _wIPDbContext.Process
                             .Select(p => new SelectListItem
                             {
                                 Value = p.ProcessId.ToString(),
                                 Text = p.ProcessName,
                                 Selected = p.ProcessId == machine.ProcessId
                             }).ToList()
            };

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult Edit(MachineJigViewModel model)
        {
            if (ModelState.IsValid)
            {
                var machine = _wIPDbContext.Machines
                               .Include(m => m.Jigs)
                               .FirstOrDefault(m => m.MachineId == model.MachineId);

                if (machine == null)
                {
                    return NotFound();
                }

                // Update machine properties
                machine.MachineName = model.MachineName;
                machine.MachineNumber = model.MachineNumber;
                machine.MachineQty = model.MachineQuantity;
                machine.ProcessId = model.SelectedProcessId;

                // Assuming we're only editing the first Jig
                var firstJig = machine.Jigs.FirstOrDefault();
                if (firstJig != null)
                {
                    firstJig.JigName = model.JigName;
                    firstJig.JigLifeSpan = model.JigLifeSpan;
                }

                _wIPDbContext.SaveChanges();
                return RedirectToAction("Index");
            }

            // Reload Processes
            model.Processes = _wIPDbContext.Process
                            .Select(p => new SelectListItem
                            {
                                Value = p.ProcessId.ToString(),
                                Text = p.ProcessName
                            }).ToList();

            return View(model);
        }

        // DELETE action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int MachineId)
        {
            try
            {
                // Log the attempt to delete the machine
                _logger.LogInformation($"Attempting to delete machine with ID {MachineId}.");

                var machine = _wIPDbContext.Machines.FirstOrDefault(m => m.MachineId == MachineId);
                if (machine == null)
                {
                    _logger.LogInformation($"Machine with ID {MachineId} not found.");
                    return Json(new { success = false, message = "Machine not found or already deleted." });
                }

                _wIPDbContext.Machines.Remove(machine);
                _wIPDbContext.SaveChanges();
                _logger.LogInformation($"Deleted machine with ID {MachineId}.");

                var redirectUrl = Url.Action("Index", "Machine"); // Adjust the controller name as needed
                return Json(new { success = true, redirectUrl = redirectUrl });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Optimistic concurrency failure: attempted to delete a machine that no longer exists.");
                return Json(new
                {
                    success = false,
                    message = "The record already has been deleted by another user."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting machine with ID {ID}", MachineId);
                return Json(new { success = false, message = ex.Message });
            }
        }

    }

}


