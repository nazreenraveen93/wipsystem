using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WIPSystem.Web.Data;
using WIPSystem.Web.Models;
using WIPSystem.Web.Services;
using WIPSystem.Web.ViewModel;
using Newtonsoft.Json;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;



namespace WIPSystem.Web.Controllers
{
    //[Authorize(Roles = "Admin, Super Admin")]
    public class LotTravellerController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;
        private readonly BarcodeService _barcodeService;
        private readonly IViewRenderService _viewRenderService;
        private readonly IConverter _converter;
        private readonly ILogger<LotTravellerController> _logger;
        private readonly LinkGenerator _linkGenerator;


        public LotTravellerController(
            WIPDbContext wIPDbContext,
            BarcodeService barcodeService,
            IViewRenderService viewRenderService,
            IConverter converter,
            ILogger<LotTravellerController> logger,
            LinkGenerator linkGenerator)
        {
            _wIPDbContext = wIPDbContext;
            _barcodeService = barcodeService;
            _viewRenderService = viewRenderService;
            _converter = converter;
            _logger = logger;
            _linkGenerator = linkGenerator;
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
        // GET: Display the form to create a new LotTraveller
        [HttpGet]
        public ActionResult Generate()
        {
            var viewModel = new GenerateLotTravellerViewModel
            {
                ProductSelectList = new SelectList(_wIPDbContext.Products, "ProductId", "PartNo")
                // Initialize other properties as needed
            };
            return View(viewModel);
        }
        public JsonResult GetCustomerNameAndProcess(int productId)
        {
            var product = _wIPDbContext.Products
                .Where(p => p.ProductId == productId)
                .FirstOrDefault();

            if (product != null)
            {
                string customerName = product.CustName;

                var processInfo = _wIPDbContext.ProductProcessMappings
                    .Where(ppm => ppm.ProductId == productId)
                    .OrderBy(ppm => ppm.Sequence)
                    .Select(ppm => new
                    {
                        ProcessId = ppm.Process.ProcessId,
                        ProcessName = ppm.Process.ProcessName,
                        Sequence = ppm.Sequence
                    })
                    .ToList();

                var result = new
                {
                    CustomerName = product.CustName,
            PackageSize = product.PackageSize, // Assuming these properties exist
            PiecesPerBlank = product.PiecesPerBlank
                };

                return Json(result);
            }

            return Json(null); // Handle the case where the product is not found
        }

        public JsonResult GetProcessName(int processId)
        {
            var process = _wIPDbContext.Process.FirstOrDefault(p => p.ProcessId == processId);

            if (process != null)
            {
                var result = new
                {
                    ProcessName = process.ProcessName
                };

                return Json(result);
            }

            return Json(null); // Handle the case where the process is not found
        }


        // POST: Process the form submission for creating a new LotTraveller
        [HttpPost]
        public async Task<IActionResult> Generate(GenerateLotTravellerViewModel model)
        {
            _logger.LogInformation("Attempting to generate a new Lot Traveller.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid.");
                model.ProductSelectList = new SelectList(_wIPDbContext.Products, "ProductId", "PartNo");
                return View(model);
            }

            // Retrieve the PartNo associated with the selected ProductId
            var selectedProductPartNo = await _wIPDbContext.Products
                                         .Where(p => p.ProductId == model.SelectedProductId)
                                         .Select(p => p.PartNo)
                                         .FirstOrDefaultAsync();

            // Check if LotNo already exists for the selected PartNo
            bool lotExists = await _wIPDbContext.LotTravellers
                                 .Include(lt => lt.Product)
                                 .AnyAsync(lt => lt.LotNo == model.LotNo && lt.Product.PartNo == selectedProductPartNo);

            if (lotExists)
            {
                _logger.LogWarning($"Lot Number {model.LotNo} already exists for Part Number {selectedProductPartNo}.");
                ModelState.AddModelError("LotNo", $"The Lot Number already exists for Part Number {selectedProductPartNo}.");
                model.ProductSelectList = new SelectList(await _wIPDbContext.Products.ToListAsync(), "ProductId", "PartNo");
                return View(model);
            }

            // Create the Lot Traveller
            var lotTraveller = new LotTraveller
            {
                ProductId = model.SelectedProductId,
                LotNo = model.LotNo,
                DateCreated = DateTime.UtcNow
            };

            _wIPDbContext.LotTravellers.Add(lotTraveller);
            await _wIPDbContext.SaveChangesAsync();

            // After saving, lotTraveller object will have the ID set by the database
            int createdLotTravellerId = lotTraveller.LotTravellerId;

            // Fetch the product and include related data
            var product = await _wIPDbContext.Products
                .Include(p => p.ProductProcessMappings)
                .ThenInclude(ppm => ppm.Process)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == model.SelectedProductId);

            // Handle product not found
            if (product == null)
            {
                _logger.LogError($"Product with ID {model.SelectedProductId} not found.");
                ModelState.AddModelError("SelectedProductId", "The selected product was not found.");
                model.ProductSelectList = new SelectList(_wIPDbContext.Products, "ProductId", "PartNo");
                return View(model);
            }

            // Generate QR code and process steps
            var qrCodeData = $"PartNo:{product.PartNo}|CustomerName:{product.CustName}|LotNo:{lotTraveller.LotNo}" +
                  $"|PackageSize:{product.PackageSize}|PiecesPerBlank:{product.PiecesPerBlank}";

            byte[] qrCodeImage = _barcodeService.GenerateQRCode(qrCodeData, 250, 250);
            var processSteps = product.ProductProcessMappings.Select(ppm => new ProcessStepViewModel
            {
                Sequence = ppm.Sequence.ToString(),
                ProcessName = ppm.Process.ProcessName,
                ProcessCode = ppm.Process.ProcessCode
            }).ToList();

            // Serialize confirmation data for redirection
            var confirmationData = new GenerateConfirmationViewModel
            {
                QRCodeImage = Convert.ToBase64String(qrCodeImage),
                ConfirmationMessage = "Lot Traveller created successfully. You can now print the details.",
                PartNo = product.PartNo,
                CustName = product.CustName,
                PackageSize = product.PackageSize,  // Added line
                PiecePerBlank = product.PiecesPerBlank,  // Added line
                LotNo = lotTraveller.LotNo,
                ProcessSteps = processSteps
            };
            TempData["ConfirmationData"] = JsonConvert.SerializeObject(confirmationData);

            _logger.LogInformation($"Redirecting to confirmation for Lot Traveller {model.LotNo}.");
            return RedirectToAction("GenerateConfirmation", new { lotTravellerId = lotTraveller.LotTravellerId });
        }

        // GET: List all LotTravellers with Product information
        public ActionResult ListLotTravellers()
        {
            var items = _wIPDbContext.LotTravellers
                .Include(lt => lt.Product) // Assuming there's a navigation property from LotTraveller to Product
                .ToList();

            return View(items);
        }

        public async Task<IActionResult> GenerateConfirmation(int lotTravellerId)
        {
            GenerateConfirmationViewModel viewModel; // Declare here

            // If TempData has confirmation data, deserialize and return the view
            if (TempData.TryGetValue("ConfirmationData", out var confirmationDataJson) && !string.IsNullOrEmpty(confirmationDataJson as string))
            {
                viewModel = JsonConvert.DeserializeObject<GenerateConfirmationViewModel>(confirmationDataJson as string); // Assign to existing variable
            }
            else
            {
                // Fetch the LotTraveller from the database including related entities
                var lotTraveller = await _wIPDbContext.LotTravellers
                    .Include(lt => lt.Product)
                    .ThenInclude(p => p.ProductProcessMappings)
                    .ThenInclude(ppm => ppm.Process)
                    .FirstOrDefaultAsync(lt => lt.LotTravellerId == lotTravellerId);

                if (lotTraveller == null || lotTraveller.Product == null)
                {
                    return NotFound();
                }

                // Generate the QR code
                var qrCodeData = $"PartNo:{lotTraveller.Product.PartNo}|CustomerName:{lotTraveller.Product.CustName}|LotNo:{lotTraveller.LotNo}|PiecePerBlank:{lotTraveller.Product.PiecesPerBlank}|PackageSize:{lotTraveller.Product.PackageSize}";

                var qrCodeImage = _barcodeService.GenerateQRCode(qrCodeData, 250, 250);

                // Fetch process steps
                var processSteps = await _wIPDbContext.ProductProcessMappings
                    .Where(ppm => ppm.ProductId == lotTraveller.ProductId)
                    .OrderBy(ppm => ppm.Sequence)
                    .Select(ppm => new ProcessStepViewModel
                    {
                        Sequence = ppm.Sequence.ToString(),
                        ProcessName = ppm.Process.ProcessName,
                        ProcessCode = ppm.ProcessId
                    }).ToListAsync();

                // Construct the ViewModel
                viewModel = new GenerateConfirmationViewModel // Assign to existing variable
                {
                    LotTravellerId = lotTravellerId,
                    PartNo = lotTraveller.Product.PartNo,
                    CustName = lotTraveller.Product.CustName,
                    LotNo = lotTraveller.LotNo,
                    PiecePerBlank = lotTraveller.Product.PiecesPerBlank,
                    PackageSize = lotTraveller.Product.PackageSize,
                    QRCodeImage = Convert.ToBase64String(qrCodeImage),
                    ConfirmationMessage = "Confirmation details",
                    ProcessSteps = processSteps
                };
            }

            return View(viewModel); // Pass the viewModel to the view
        }



        public async Task<IActionResult> GeneratePdf(int lotTravellerId)
        {
            //_logger.LogInformation($"Generating PDF for LotTravellerId: {lotTravellerId}");

            if (lotTravellerId <= 0)
            {
                _logger.LogWarning("Invalid LotTravellerId passed to GeneratePdf");
                return BadRequest("Invalid request for PDF generation.");
            }

            var lotTraveller = await _wIPDbContext.LotTravellers
                .Include(lt => lt.Product)
                .ThenInclude(p => p.ProductProcessMappings)
                .ThenInclude(ppm => ppm.Process)
                .FirstOrDefaultAsync(lt => lt.LotTravellerId == lotTravellerId);

            if (lotTraveller == null || lotTraveller.Product == null)
            {
                return NotFound();
            }

            var qrCodeData = $"PartNo:{lotTraveller.Product.PartNo}|CustomerName:{lotTraveller.Product.CustName}|LotNo:{lotTraveller.LotNo}";
            byte[] qrCodeImage = _barcodeService.GenerateQRCode(qrCodeData, 250, 250);

            // Use LinkGenerator to generate the URL instead of Url.Action
            var pdfDownloadUrl = _linkGenerator.GetPathByAction(
                HttpContext,
                action: "GeneratePdf",
                controller: "LotTraveller",
                values: new { lotTravellerId = lotTravellerId });

            // Log and check if the URL is generated correctly
            _logger.LogInformation($"PDF Download URL: {pdfDownloadUrl}");
            if (string.IsNullOrEmpty(pdfDownloadUrl))
            {
                _logger.LogWarning("The PDF download URL was not generated correctly.");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            if (string.IsNullOrEmpty(pdfDownloadUrl))
            {
                _logger.LogWarning("The PDF download URL was not generated correctly.");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            if (!lotTraveller.Product.ProductProcessMappings.Any())
            {
                return NotFound();
            }

            var processSteps = lotTraveller.Product.ProductProcessMappings
                .OrderBy(ppm => ppm.Sequence)
                .Select(ppm => new ProcessStepViewModel
                {
                    Sequence = ppm.Sequence.ToString(),
                    ProcessName = ppm.Process.ProcessName
                }).ToList();

            var viewModel = new GenerateConfirmationViewModel
            {
                LotTravellerId = lotTravellerId,
                QRCodeImage = Convert.ToBase64String(qrCodeImage),
                ConfirmationMessage = "Lot Traveller created successfully.",
                PartNo = lotTraveller.Product.PartNo,
                CustName = lotTraveller.Product.CustName,
                LotNo = lotTraveller.LotNo,
                ProcessSteps = processSteps,
                PdfDownloadUrl = pdfDownloadUrl
            };

            string htmlContent = await _viewRenderService.RenderToStringAsync("LotTraveller/GenerateConfirmation", viewModel);

            var pdfDoc = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10 },
                    DocumentTitle = "Lot Traveller Confirmation",
                },
                Objects = { new ObjectSettings { HtmlContent = htmlContent } }
            };

            byte[] pdfContent = _converter.Convert(pdfDoc);
            string fileName = $"LotTraveller_{lotTravellerId}.pdf";
            return File(pdfContent, "application/pdf", fileName);
        }

        // GET: LotTraveller/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? lotTravellerId)
        {
            if (lotTravellerId == null)
            {
                return NotFound();
            }

            var lotTraveller = await _wIPDbContext.LotTravellers
                .Include(lt => lt.Product)
                .ThenInclude(p => p.ProductProcessMappings) // Make sure to include this relationship
                .ThenInclude(ppm => ppm.Process) // Include this as well if you need details from the Process entity
                .FirstOrDefaultAsync(lt => lt.LotTravellerId == lotTravellerId);

            if (lotTraveller == null)
            {
                return NotFound();
            }

            // Initialize the ProcessSteps list
            var processSteps = lotTraveller.Product?.ProductProcessMappings?
                .Select(ppm => new ProcessStepViewModel
                {
                    ProcessId = ppm.ProcessId, // Make sure to include ProcessId
                    ProcessName = ppm.Process?.ProcessName, // This might not be needed if you're using a dropdown
                    Sequence = ppm.Sequence.ToString(), // Convert int to string
                }).ToList() ?? new List<ProcessStepViewModel>();

            var viewModel = new EditLotTravellerViewModel
            {
                LotTravellerId = lotTraveller.LotTravellerId,
                PartNo = lotTraveller.Product?.PartNo,
                CustName = lotTraveller.CustomerName,
                PackageSize = lotTraveller.Product.PackageSize,
                PiecesPerBlank = lotTraveller.Product.PiecesPerBlank,
                LotNo = lotTraveller.LotNo,
                DateCreated = lotTraveller.DateCreated,
                ProcessSteps = processSteps,
                AvailableProcesses = new SelectList(await _wIPDbContext.Process.ToListAsync(), "ProcessId", "ProcessName"),
                ProductSelectList = new SelectList(_wIPDbContext.Products, "ProductId", "PartNo", lotTraveller.ProductId)
            };

            return View(viewModel);
        }


        // POST: LotTraveller/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int lotTravellerId, EditLotTravellerViewModel viewModel)
        {
            if (lotTravellerId != viewModel.LotTravellerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var lotTraveller = await _wIPDbContext.LotTravellers.FindAsync(lotTravellerId);
                if (lotTraveller == null)
                {
                    return NotFound();
                }

                // Update only the LotNo
                lotTraveller.LotNo = viewModel.LotNo;

                try
                {
                    _wIPDbContext.Update(lotTraveller);
                    await _wIPDbContext.SaveChangesAsync();

                    TempData["success"] = "Lot Traveller updated successfully."; // Set success message here
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LotTravellerExists(lotTraveller.LotTravellerId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index)); // Redirect to the index or appropriate page
            }

            // If the model state is not valid, reload the form with the current data
            return View(viewModel);
        }

        private bool LotTravellerExists(int LotTravellerId)
        {
            // Implementation of LotTravellerExists
            return _wIPDbContext.LotTravellers.Any(e => e.LotTravellerId == LotTravellerId);
        }
        // GET: LotTraveller/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? LotTravellerId)
        {
            if (LotTravellerId == null)
            {
                return NotFound();
            }

            var lotTraveller = await _wIPDbContext.LotTravellers
                .Include(lt => lt.Product)
                .FirstOrDefaultAsync(m => m.LotTravellerId == LotTravellerId);

            if (lotTraveller == null)
            {
                return NotFound();
            }

            return View(lotTraveller);
        }

        // POST: LotTraveller/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int LotTravellerId)
        {
            var lotTraveller = await _wIPDbContext.LotTravellers.FindAsync(LotTravellerId);
            if (lotTraveller != null)
            {
                _wIPDbContext.LotTravellers.Remove(lotTraveller);
                await _wIPDbContext.SaveChangesAsync();
                TempData["success"] = "Lot Traveller deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Lot Traveller not found.";
            return RedirectToAction(nameof(Delete), new { LotTravellerId = LotTravellerId });
        }

    }

}
