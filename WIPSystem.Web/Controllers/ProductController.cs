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


        public IActionResult Index()
        {
            List<Product> Product = _wIPDbContext.Products.ToList();

            return View(Product);
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

        // Modify the GET method to include ProductProcessMappings
        [HttpGet]
        public IActionResult Edit(int productId)
        {
            // First, ensure the related entities are loaded or checked for null before use.
            var product = _wIPDbContext.Products.FirstOrDefault(x => x.ProductId == productId);
            if (product == null)
            {
                return NotFound();
            }

            var productProcessMappings = _wIPDbContext.ProductProcessMappings
                                                      .Where(m => m.ProductId == productId)
                                                      .ToList() ?? new List<ProductProcessMapping>();

            var availableProcesses = GetProcesses() ?? new List<Process>();

            var viewModel = new ProductEditViewModel
            {
                Product = product,
                ProductProcessMappings = productProcessMappings,
                AvailableProcesses = availableProcesses
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

        //Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var product = viewModel.Product;
            var submittedMappings = viewModel.ProductProcessMappings ?? new List<ProductProcessMapping>(); // Ensure this is not null.

            // Load the current product from the DB along with its mappings
            var dbProduct = _wIPDbContext.Products
                                         .Include(p => p.ProductProcessMappings)
                                         .FirstOrDefault(p => p.ProductId == product.ProductId);

            if (dbProduct == null)
            {
                return NotFound();
            }

            // Update product properties
            dbProduct.PartNo = product.PartNo;
            dbProduct.CustName = product.CustName;
            dbProduct.PackageSize = product.PackageSize;
            dbProduct.PiecesPerBlank = product.PiecesPerBlank;

            // Remove any mappings that are not included in the submitted data
            var dbMappingsToRemove = dbProduct.ProductProcessMappings
                                              .Where(m => !submittedMappings.Any(ppm => ppm.ProductProcessMappingId == m.ProductProcessMappingId))
                                              .ToList();
            _wIPDbContext.ProductProcessMappings.RemoveRange(dbMappingsToRemove);

            // Update existing mappings and add new ones
            foreach (var mapping in submittedMappings)
            {
                var dbMapping = dbProduct.ProductProcessMappings
                                         .FirstOrDefault(m => m.ProductProcessMappingId == mapping.ProductProcessMappingId);

                if (dbMapping != null)
                {
                    // Update the existing mapping
                    dbMapping.Sequence = mapping.Sequence;
                    dbMapping.ProcessId = mapping.ProcessId;
                    // ... other properties as necessary
                }
                else
                {
                    // The mapping is new, so add it
                    _wIPDbContext.ProductProcessMappings.Add(new ProductProcessMapping
                    {
                        ProductId = product.ProductId,
                        ProcessId = mapping.ProcessId,
                        Sequence = mapping.Sequence
                        // Set any other properties you need here
                    });
                }
            }

            try
            {
                _wIPDbContext.SaveChanges();
                TempData["warning"] = "Record Updated Successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Another user has updated this record. Please reload and try again.");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while updating the product: {ex.Message}");
                return View(viewModel);
            }
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
