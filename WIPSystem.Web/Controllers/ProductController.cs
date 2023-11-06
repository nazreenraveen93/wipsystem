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
                _wIPDbContext.Products.Add(product);
                _wIPDbContext.SaveChanges();
                Console.WriteLine(product.ProductId); // Log the ProductId or use any other logging mechanism you have


                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Invalid product data." });
            }
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
            if (ModelState.IsValid)
            {
                using (var transaction = _wIPDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var selectedProcess in model.SelectedProcesses)
                        {
                            var mapping = new ProductProcessMapping
                            {
                                ProductId = model.ProductId,
                                ProcessId = selectedProcess.ProcessId,
                                Sequence = selectedProcess.Sequence
                            };

                            _wIPDbContext.ProductProcessMappings.Add(mapping);
                        }

                        _wIPDbContext.SaveChanges();
                        transaction.Commit(); // commit the transaction

                        // Return JSON indicating success
                        return Json(new { success = true });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // rollback the transaction in case of an error
                                                // Log the exception here
                                                // Return JSON indicating failure and include the exception message
                        return Json(new { success = false, message = "An unexpected error occurred. Please try again later.", exception = ex.Message });
                    }
                }
            }
            // Return JSON indicating validation failure and include the validation errors
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            return Json(new { success = false, errors = errors });
        }

    }
}
