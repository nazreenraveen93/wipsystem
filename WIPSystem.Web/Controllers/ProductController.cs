using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WIPSystem.Web.Data;
using WIPSystem.Web.Models;
using System.Text.Json;
using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WIPSystem.Web.ViewModels;
using WIPSystem.Web.ViewModel;
using Process = WIPSystem.Web.Models.Process;


namespace WIPSystem.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;
        private readonly ILogger<ProductController> _logger;

        public ProductController(WIPDbContext wIPDbContext, ILogger<ProductController> logger)
        {
            _wIPDbContext = wIPDbContext;
            _logger = logger;
        }
        private bool CheckIfProcessFlowExists(int productId)
        {
            // Assuming ProductProcessMappings is the correct way to check for process flow existence
            return _wIPDbContext.Products
                .Include(p => p.ProductProcessMappings)
                .Any(p => p.ProductId == productId && p.ProductProcessMappings.Any());
        }
        public IActionResult Index()
        {
            var products = _wIPDbContext.Products.ToList();
            var processFlowExists = new Dictionary<int, bool>();

            foreach (var product in products)
            {
                processFlowExists[product.ProductId] = _wIPDbContext.ProductProcessMappings.Any(ppm => ppm.ProductId == product.ProductId);
            }

            ViewBag.ProcessFlowExists = processFlowExists;
            return View(products);
        }


        [HttpGet]
        public IActionResult CheckPartNoExistence(string partNo)
        {
            bool exists = _wIPDbContext.Products.Any(p => p.PartNo == partNo);

            // This will return true if the partNo exists, otherwise false
            return Json(new { exists = exists });

        }
        [HttpGet]
        public IActionResult Add()
        {
            return View(new Product());
        }

        [HttpPost]
        public IActionResult Add(Product product)
        {
            if (ModelState.IsValid)
            {
                // Check if a product with the same PartNo already exists
                bool PartNoExists = _wIPDbContext.Products.Any(p => p.PartNo == product.PartNo);

                if (PartNoExists)
                {
                    // Return a JSON response indicating failure due to existing part number
                    return Json(new { success = false, message = "Part Number already exists!" });
                }

                try
                {
                    _wIPDbContext.Products.Add(product);
                    _wIPDbContext.SaveChanges();

                    // Return a JSON response indicating success
                    return Json(new { success = true, message = "Product added successfully. Please register process flow." });
                }
                catch (Exception ex)
                {
                    // Log the exception here
                    // Return a JSON response indicating failure due to an exception
                    return Json(new { success = false, message = "An error occurred while adding the product." });
                }
            }

            // Extract validation messages and send them back to the client if needed
            var errorMessages = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();

            // Return a JSON response indicating validation failure
            return Json(new { success = false, message = errorMessages });
        }

        public IActionResult RegisterProcessFlow(int productId)
        {
            Console.WriteLine(productId);  // Log the productId

            var product = _wIPDbContext.Products.FirstOrDefault(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound("Product not found");
            }

            var model = new ProductProcessRegistrationViewModel
            {
                ProductId = product.ProductId,
                PartNo = product.PartNo,
                CustName = product.CustName,
                AvailableProcesses = _wIPDbContext.Process.ToList() // Assuming table name is Processes
            };

            return View(model);
        }
        [HttpPost]
        public IActionResult SaveProcessFlow(ProductProcessRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                return Json(new { success = false, errors = errors });
            }

            // HashSet to keep track of sequences added in the current request to avoid duplicates
            var sequences = new HashSet<int>();

            using (var transaction = _wIPDbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var selectedProcess in model.SelectedProcesses)
                    {
                        // Check if sequence is already used for the same ProductId in the database
                        if (_wIPDbContext.ProductProcessMappings.Any(ppm => ppm.ProductId == model.ProductId && ppm.Sequence == selectedProcess.Sequence))
                        {
                            return Json(new { success = false, message = $"Duplicate sequence {selectedProcess.Sequence} detected for ProductId {model.ProductId}." });
                        }

                        // Check if sequence is already used in the current transaction/request
                        if (!sequences.Add(selectedProcess.Sequence))
                        {
                            Console.WriteLine($"Duplicate sequence detected: {selectedProcess.Sequence}");
                            return Json(new { success = false, message = $"Duplicate sequence {selectedProcess.Sequence} detected." });
                        }

                        // Add the mapping
                        var mapping = new ProductProcessMapping
                        {
                            ProductId = model.ProductId,
                            ProcessId = selectedProcess.ProcessId,
                            Sequence = selectedProcess.Sequence
                        };

                        _wIPDbContext.ProductProcessMappings.Add(mapping);
                    }

                    _wIPDbContext.SaveChanges();
                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(ex.ToString()); // Outputs the entire exception to the console for debugging
                    return Json(new { success = false, message = "An error occurred while saving the process flow. " + ex.Message });
                }
            }
        }

        [HttpGet]
        public IActionResult Edit(int productId)
        {
            var product = _wIPDbContext.Products
                .Include(p => p.ProductProcessMappings)
                .FirstOrDefault(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound();
            }

            var processSteps = product.ProductProcessMappings
                .OrderBy(m => m.Sequence)
                .Select(m => new ProcessViewModel
                {
                    Sequence = m.Sequence,
                    ProcessId = m.ProcessId,
                    ProcessName = _wIPDbContext.Process.FirstOrDefault(p => p.ProcessId == m.ProcessId)?.ProcessName
                })
                .ToList();

            var viewModel = new ProductEditViewModel
            {
                Product = product,
                ProcessSteps = processSteps,
                AvailableProcesses = _wIPDbContext.Process.ToList()
            };

            return View(viewModel);
        }


        private List<Process> GetProcesses()
        {
            // Ensure that this method does not return null.
            // If _wIPDbContext.Process could be null, handle it here.
            var processes = _wIPDbContext.Process?.ToList(); // Using the null-conditional operator.
            return processes ?? new List<Process>(); // Return an empty list if null.
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductEditViewModel viewModel, string deletedProcessIds)
        {
            if (!ModelState.IsValid)
            {
                viewModel.AvailableProcesses = _wIPDbContext.Process.ToList();
                return View(viewModel);
            }

            // Parse the deleted indices
            var deletedIndices = !string.IsNullOrEmpty(deletedProcessIds)
                ? deletedProcessIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .OrderByDescending(i => i) // Important to delete from the end
                    .ToList()
                : new List<int>();

            var product = _wIPDbContext.Products
                .Include(p => p.ProductProcessMappings)
                .FirstOrDefault(p => p.ProductId == viewModel.Product.ProductId);

            if (product == null)
            {
                return NotFound();
            }

            // Update product details
            product.PartNo = viewModel.Product.PartNo;
            product.CustName = viewModel.Product.CustName;
            product.PackageSize = viewModel.Product.PackageSize;
            product.PiecesPerBlank = viewModel.Product.PiecesPerBlank;
            // ... update other properties ...

            // Handle deletions based on indices
            foreach (var index in deletedIndices)
            {
                if (index >= 0 && index < viewModel.ProcessSteps.Count)
                {
                    var processStepToDelete = viewModel.ProcessSteps[index];
                    var mappingToDelete = _wIPDbContext.ProductProcessMappings
                        .FirstOrDefault(m => m.ProductId == product.ProductId && m.ProcessId == processStepToDelete.ProcessId);
                    if (mappingToDelete != null)
                    {
                        _wIPDbContext.ProductProcessMappings.Remove(mappingToDelete);
                    }
                }
            }

            // Update existing mappings and add new ones
            foreach (var processStep in viewModel.ProcessSteps)
            {
                var mapping = product.ProductProcessMappings
                    .FirstOrDefault(m => m.ProcessId == processStep.ProcessId);

                if (mapping != null)
                {
                    mapping.Sequence = processStep.Sequence;
                    mapping.ProcessId = processStep.ProcessId;
                    // ... other mapping updates ...
                }
                else
                {
                    // Add new mapping
                    product.ProductProcessMappings.Add(new ProductProcessMapping
                    {
                        ProductId = product.ProductId,
                        ProcessId = processStep.ProcessId,
                        Sequence = processStep.Sequence
                    });
                }
            }

            _wIPDbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Delete(int ProductId)
        {
            Product product = _wIPDbContext.Products.FirstOrDefault(x => x.ProductId == ProductId);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int ProductId)
        {
            var product = _wIPDbContext.Products
                                       .Include(p => p.ProductProcessMappings) // Include the related entities
                                       .FirstOrDefault(x => x.ProductId == ProductId);

            if (product == null)
            {
                return NotFound();
            }

            try
            {
                // Remove the related ProductProcessMappings first if they exist
                if (product.ProductProcessMappings != null)
                {
                    _wIPDbContext.ProductProcessMappings.RemoveRange(product.ProductProcessMappings);
                }

                // Now remove the product itself
                _wIPDbContext.Products.Remove(product);
                _wIPDbContext.SaveChanges();

                TempData["error"] = "Record Deleted Successfully"; // Using "error" key for successful deletion might be confusing, consider using "success" or "message" instead
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details here
                // It's a good practice to provide a user-friendly message rather than exposing exception details
                TempData["error"] = "An error occurred while deleting the record.";
                return View("Error"); // Redirect to a custom error page with user-friendly error information
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
