using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using WIPSystem.Web.Data;
using WIPSystem.Web.Models;
using static System.Collections.Specialized.BitVector32;
using Process = WIPSystem.Web.Models.Process;

namespace WIPSystem.Web.Controllers
{
    public class ProcessController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;

        public ProcessController(WIPDbContext wIPDbContext)
        {
            _wIPDbContext = wIPDbContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Process> processes = _wIPDbContext.Process.ToList();

            return View(processes);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Process process )
        {
            if (ModelState.IsValid)
            {
                // Check if a process with the same processCode and processName already exists
                bool processExists = _wIPDbContext.Process
                    .Any(p => p.ProcessCode == process.ProcessCode && p.ProcessName == process.ProcessName);

                if (processExists)
                {
                    TempData["error"] = "A process with the same Code and Name already exists!";
                    return View(process);
                }

                _wIPDbContext.Process.Add(process);
                _wIPDbContext.SaveChanges();

                TempData["success"] = "Record Created Successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(process);
        }

        [HttpGet]
        public IActionResult Edit(int ProcessId)
        {
            Process process = _wIPDbContext.Process.FirstOrDefault(x => x.ProcessId == ProcessId);

            return View(process);
        }

        //Update
        [HttpPost]
        [ValidateAntiForgeryToken] // Ensure you're protecting against CSRF
        public IActionResult Edit(int ProcessId, Process process)
        {
            if (ProcessId != process.ProcessId)
            {
                return BadRequest();
            }

            var existingProcess = _wIPDbContext.Process.FirstOrDefault(p => p.ProcessId == ProcessId);

            if (existingProcess == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the valid properties you want to change
                    existingProcess.ProcessName = process.ProcessName;
                    existingProcess.ProcessCode = process.ProcessCode;
                    // Other relevant properties...

                    _wIPDbContext.SaveChanges();

                    TempData["warning"] = "Record Updated Successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Log exception, and/or
                    return BadRequest("Concurrency error occurred while updating the process. Please try again.");
                }
                catch (Exception ex)
                {
                    // Log exception
                    return BadRequest("An error occurred while updating the process.");
                }

                return RedirectToAction(nameof(Index));
            }
            // Model state is invalid
            return View(process);
        }

        [HttpGet]
        public IActionResult Delete(int ProcessId)
        {
            Process process = _wIPDbContext.Process.FirstOrDefault(x => x.ProcessId == ProcessId);

            if (process == null)
            {
                return NotFound();
            }

            return View(process);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] // It's important to prevent CSRF attacks, especially on delete operations
        public IActionResult DeleteConfirmed(int ProcessId) // This name makes the intention of the method clearer
        {
            var process = _wIPDbContext.Process.FirstOrDefault(x => x.ProcessId == ProcessId);

            if (process == null)
            {
                return NotFound();
            }

            try
            {
                _wIPDbContext.Process.Remove(process);
                _wIPDbContext.SaveChanges();

                TempData["error"] = "Record Deleted Successfully";
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details here
                // You should replace this with your actual logging mechanism
                // For instance, if you're using a logging framework like NLog, Serilog, or similar
                Console.WriteLine(ex.Message); // Simplified example: replace with your actual logging mechanism
                return View("Error"); // You might want to create a dedicated Error view
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
