﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient; // Ensure you have this using directive
using Microsoft.EntityFrameworkCore;
using System.Data;
using WIPSystem.Web.Data;
using WIPSystem.Web.Models;
using WIPSystem.Web.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Controllers
{
    public class Re2PressingController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Re2PressingController> _logger;

        public Re2PressingController(WIPDbContext wIPDbContext, IConfiguration configuration, ILogger<Re2PressingController> logger)
        {
            _wIPDbContext = wIPDbContext;
            _configuration = configuration;
            _logger = logger;
        }
        public IActionResult Index()
        {
            if (TempData["ShowSwal"] != null)
            {
                ViewBag.ShowSwal = true; // Pass a flag to the view to show the alert
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            // Adjusted query to include both conditions
            var query = "SELECT CurrentStatusId, PartNo, LotNo, ReceivedQuantity,Date, Status FROM CurrentStatus WHERE ProcessCurrentStatus = 'Re2-Pressing' AND Status = 'In Progress'";


            var model = new List<CurrentStatus>(); // Assuming CurrentStatus is your model class

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new CurrentStatus // Replace 'CurrentStatus' with your actual model class
                        {
                            // Assuming you have properties like Id, PartNo, etc. in your model
                            CurrentStatusId = reader.GetInt32(reader.GetOrdinal("CurrentStatusId")), // Fetch and assign CurrentStatusId
                            PartNo = reader.GetString(reader.GetOrdinal("PartNo")),
                            LotNo = reader.GetString(reader.GetOrdinal("LotNo")),
                            ReceivedQuantity = reader.GetInt32(reader.GetOrdinal("ReceivedQuantity")), // Corrected this line
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            Date = reader.GetDateTime(reader.GetOrdinal("Date")), // Use the correct column name

                            // Populate other fields similarly
                        };
                        model.Add(item);
                    }
                }
            }
            return View(model);
        }

        public IActionResult Index1()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            // Adjusted query to include both conditions
            var query = @"
            SELECT Re2PressingId, PartNo, LotNo, OutputQty, Status 
            FROM Re2Pressing 
            WHERE Status IN ('Completed', 'On Hold')";

            var model = new List<Re2PressingViewModel>(); // Assuming CurrentStatus is your model class
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new Re2PressingViewModel
                        {
                            Re2PressingId = reader.GetInt32(reader.GetOrdinal("Re2PressingId")),
                            PartNo = reader.IsDBNull(reader.GetOrdinal("PartNo")) ? null : reader.GetString(reader.GetOrdinal("PartNo")),
                            LotNo = reader.IsDBNull(reader.GetOrdinal("LotNo")) ? null : reader.GetString(reader.GetOrdinal("LotNo")),
                            OutputQty = reader.IsDBNull(reader.GetOrdinal("OutputQty")) ? 0 : reader.GetInt32(reader.GetOrdinal("OutputQty")),
                            Status = reader.IsDBNull(reader.GetOrdinal("Status")) ?
                            Re2PressingProcessStatus.Completed : // or some default status
                             EnumExtensions.ConvertToEnum<Re2PressingProcessStatus>(reader.GetString(reader.GetOrdinal("Status"))),

                            // Continue for other properties...
                        };
                        model.Add(item);
                    }

                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Create(int CurrentStatusId)
        {
            var viewModel = new Re2PressingViewModel

            {
                CurrentStatusId = CurrentStatusId,
                ProcessCurrentStatus = "Re2-Pressing"
            };
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Fetch details from CurrentStatus table
                string getCurrentStatusDetailsQuery = @"
                SELECT PartNo, LotNo, ReceivedQuantity ,ProcessCurrentStatus
                FROM CurrentStatus 
                WHERE CurrentStatusId = @CurrentStatusId";

                using (var command = new SqlCommand(getCurrentStatusDetailsQuery, connection))
                {
                    command.Parameters.AddWithValue("@CurrentStatusId", CurrentStatusId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            viewModel.PartNo = reader["PartNo"].ToString();
                            viewModel.LotNo = reader["LotNo"].ToString();
                            viewModel.ReceivedQuantity = reader.GetInt32(reader.GetOrdinal("ReceivedQuantity"));
                            viewModel.ProcessCurrentStatus = reader["ProcessCurrentStatus"].ToString();
                        }
                        else
                        {
                            // Handle the case where no record is found for the given CurrentStatusId
                            // This could involve setting a default state for the viewModel
                            // or adding an error message to display to the user
                        }
                    }
                }
            }

            // Retrieve PackageSize, CustomerName, and PiecesPerBlank from the Product table
            var product = _wIPDbContext.Products.FirstOrDefault(p => p.PartNo == viewModel.PartNo);
            viewModel.PackageSize = product?.PackageSize;
            viewModel.CustomerName = product?.CustName;
            viewModel.PiecesPerBlank = product?.PiecesPerBlank ?? 0;

            // Set the CheckedBy property based on the authenticated user
            viewModel.CheckedBy = User.Identity.Name;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Re2PressingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PrepareViewModel(model);
                return View(model);
            }

            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {


                            string insertCommandText = @"
                            INSERT INTO Re2Pressing
                            (PartNo, LotNo, ProcessCurrentStatus, CheckedBy, Status, Remarks, OutputQty, RejectQty, Date)
                            VALUES
                            (@PartNo, @LotNo, @ProcessCurrentStatus, @CheckedBy, @Status, @Remarks, @OutputQty, @RejectQty, @Date)";

                            using (var insertCommand = new SqlCommand(insertCommandText, connection, transaction))
                            {
                                insertCommand.Parameters.AddWithValue("@PartNo", model.PartNo);
                                insertCommand.Parameters.AddWithValue("@LotNo", model.LotNo);
                                insertCommand.Parameters.AddWithValue("@ProcessCurrentStatus", model.ProcessCurrentStatus);
                                insertCommand.Parameters.AddWithValue("@CheckedBy", model.CheckedBy);
                                // Convert enum to string before assigning to command parameter
                                insertCommand.Parameters.AddWithValue("@Status", ((Enum)model.Status).GetDisplayName());
                                insertCommand.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                                insertCommand.Parameters.AddWithValue("@OutputQty", model.OutputQty);
                                insertCommand.Parameters.AddWithValue("@RejectQty", model.RejectQty);
                                insertCommand.Parameters.AddWithValue("@Date", DateTime.Now);

                                int insertResult = await insertCommand.ExecuteNonQueryAsync();

                                if (insertResult > 0)
                                {
                                    // Now perform the update on CurrentStatus table
                                    string updateCommandText = @"
                                UPDATE CurrentStatus
                                SET Status = @Status, 
                                    ReceivedQuantity = @ReceivedQuantity, 
                                    Remarks = @Remarks, 
                                    PIC = @PIC, 
                                    Date = @Date
                                WHERE PartNo = @PartNo AND LotNo = @LotNo";

                                    using (var updateCommand = new SqlCommand(updateCommandText, connection, transaction))
                                    {
                                        // Assuming the Status in CurrentStatus table is stored as string, convert enum to string
                                        updateCommand.Parameters.AddWithValue("@Status", ((Enum)model.Status).GetDisplayName());
                                        updateCommand.Parameters.AddWithValue("@ReceivedQuantity", model.OutputQty);
                                        updateCommand.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                                        updateCommand.Parameters.AddWithValue("@PIC", model.CheckedBy);
                                        updateCommand.Parameters.AddWithValue("@Date", DateTime.Now);
                                        updateCommand.Parameters.AddWithValue("@PartNo", model.PartNo);
                                        updateCommand.Parameters.AddWithValue("@LotNo", model.LotNo);

                                        int updateResult = await updateCommand.ExecuteNonQueryAsync();
                                        if (updateResult == 1)
                                        {
                                            // Commit transaction if both insert and update are successful
                                            transaction.Commit();
                                            TempData["Success"] = "Data inserted and updated successfully.";
                                            return RedirectToAction("Index1");
                                        }
                                        else
                                        {
                                            _logger.LogWarning("No records were updated in CurrentStatus for PartNo: {PartNo}, LotNo: {LotNo}", model.PartNo, model.LotNo);
                                            // Decide if you want to roll back or not based on your application requirements
                                            transaction.Rollback();
                                            ModelState.AddModelError("", "Unable to update the record in the CurrentStatus table.");
                                        }
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError("", "No records were inserted into the RePressing table.");
                                    // Optionally roll back the transaction here as well
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback(); // Roll back the transaction on error
                            _logger.LogError(ex, "An error occurred while creating a RePressing record.");
                            ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while opening the database connection.");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            PrepareViewModel(model);
            return View(model);
        }
        private void PrepareViewModel(Re2PressingViewModel model)
        {
            // Retrieve the process ID based on the current status name.
            var processId = _wIPDbContext.Process
                                         .Where(p => p.ProcessName == model.ProcessCurrentStatus)
                                         .Select(p => p.ProcessId)
                                         .FirstOrDefault();


        }

        public async Task<IActionResult> Re2PressingDetails(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            Re2PressingViewModel viewModel = null;

            string commandText = @"
            SELECT 
                r.Re2PressingId, 
                r.PartNo, 
                r.LotNo, 
                r.ProcessCurrentStatus,
                r.CheckedBy,
                r.Status,
                r.Remarks,
                r.OutputQty, 
                r.RejectQty, 
                p.CustName AS CustomerName,
                p.PackageSize,
                p.PiecesPerBlank
            FROM Re2Pressing r
            INNER JOIN Products p ON r.PartNo = p.PartNo
            WHERE r.Re2PressingId = @Re2PressingId";


            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var command = new SqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@Re2PressingId", id);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        viewModel = new Re2PressingViewModel
                        {
                            Re2PressingId = reader.GetInt32(reader.GetOrdinal("Re2PressingId")),
                            PartNo = reader.GetString(reader.GetOrdinal("PartNo")),
                            LotNo = reader.GetString(reader.GetOrdinal("LotNo")),
                            ProcessCurrentStatus = reader.GetString(reader.GetOrdinal("ProcessCurrentStatus")),
                            CheckedBy = reader.GetString(reader.GetOrdinal("CheckedBy")),
                            Status = EnumExtensions.ConvertToEnum<Re2PressingProcessStatus>(reader.GetString(reader.GetOrdinal("Status"))),
                            Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                            OutputQty = reader.GetInt32(reader.GetOrdinal("OutputQty")),
                            RejectQty = reader.GetInt32(reader.GetOrdinal("RejectQty")),
                            CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                            PackageSize = reader.GetString(reader.GetOrdinal("PackageSize")),
                            PiecesPerBlank = reader.GetInt32(reader.GetOrdinal("PiecesPerBlank"))
                            // ...other properties as needed...
                        };

                    }
                }
            }

            if (viewModel == null)
            {
                return NotFound();
            }

            return PartialView("_Re2PressingDetailsModal", viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Re2PressingViewModel viewModel = new Re2PressingViewModel();

            string commandText = @"
            SELECT
               r.Re2PressingId, 
               r.PartNo, 
               r.LotNo, 
               r.ProcessCurrentStatus,
               r.CheckedBy,
               r.Status,
               r.Remarks,
               r.OutputQty, 
               r.RejectQty, 
               p.CustName AS CustomerName,
               p.PackageSize,
               p.PiecesPerBlank,
               cs.ReceivedQuantity
            FROM Re2Pressing r
            INNER JOIN Products p ON r.PartNo = p.PartNo 
            LEFT JOIN CurrentStatus cs ON r.PartNo = cs.PartNo AND r.LotNo = cs.LotNo
            WHERE r.Re2PressingId = @Re2PressingId";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@Re2PressingId", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            viewModel.Re2PressingId = reader.GetInt32(reader.GetOrdinal("Re2PressingId"));
                            viewModel.PartNo = reader.GetString(reader.GetOrdinal("PartNo"));
                            viewModel.LotNo = reader.GetString(reader.GetOrdinal("LotNo"));
                            viewModel.ProcessCurrentStatus = reader.GetString(reader.GetOrdinal("ProcessCurrentStatus"));
                            viewModel.CheckedBy = reader.GetString(reader.GetOrdinal("CheckedBy"));
                            string statusString = reader.GetString(reader.GetOrdinal("Status"));
                            if (Enum.TryParse<Re2PressingProcessStatus>(statusString, true, out var parsedStatus))
                            {
                                viewModel.Status = parsedStatus;
                            }
                            else
                            {
                                // Handle the case where parsing fails. Log an error, throw an exception, or set a default value.
                                // Setting a default value as an example:
                                viewModel.Status = Re2PressingProcessStatus.OnHold; // Or another appropriate default value
                            }

                            viewModel.Remarks = reader.GetString(reader.GetOrdinal("Remarks"));
                            viewModel.OutputQty = reader.GetInt32(reader.GetOrdinal("OutputQty"));
                            viewModel.RejectQty = reader.GetInt32(reader.GetOrdinal("RejectQty"));
                            viewModel.CustomerName = reader.GetString(reader.GetOrdinal("CustomerName"));
                            viewModel.PackageSize = reader.GetString(reader.GetOrdinal("PackageSize"));
                            viewModel.PiecesPerBlank = reader.GetInt32(reader.GetOrdinal("PiecesPerBlank"));
                            viewModel.ReceivedQuantity = reader.GetInt32(reader.GetOrdinal("ReceivedQuantity"));

                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }

            // Populate MachineOptions for dropdown list
            await PrepareViewModelForEdit(viewModel);

            return View(viewModel);
        }

        private async Task PrepareViewModelForEdit(Re2PressingViewModel model)
        {
            // Retrieve the process ID based on the current status name.
            var processId = await _wIPDbContext.Process
                                               .Where(p => p.ProcessName == model.ProcessCurrentStatus)
                                               .Select(p => p.ProcessId)
                                               .FirstOrDefaultAsync();


        }

        [HttpPost]
        public async Task<IActionResult> Edit(Re2PressingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareViewModelForEdit(model);
                return View(model);
            }
            string agUpdateCommandText = @"
            UPDATE Re2Pressing
            SET OutputQty = @OutputQty, RejectQty = @RejectQty,
                Remarks = @Remarks, Status = @Status
            WHERE Re2PressingId = @Re2PressingId";

            string currentStatusUpdateCommandText = @"
            UPDATE CurrentStatus
            SET Status = @Status, ReceivedQuantity = @OutputQty, 
                Remarks = @Remarks
            WHERE PartNo = @PartNo AND LotNo = @LotNo";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var agCommand = new SqlCommand(agUpdateCommandText, connection, transaction))
                    {
                        // Bind parameters for Sinter update
                        agCommand.Parameters.AddWithValue("@Re2PressingId", model.Re2PressingId);
                        agCommand.Parameters.AddWithValue("@OutputQty", model.OutputQty);
                        agCommand.Parameters.AddWithValue("@RejectQty", model.RejectQty);
                        agCommand.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value); // Handle potential NULL value
                        agCommand.Parameters.AddWithValue("@Status", model.Status.ToString()); // Convert enum to string

                        var sinterResult = await agCommand.ExecuteNonQueryAsync();

                        if (sinterResult > 0)
                        {
                            using (var currentStatusCommand = new SqlCommand(currentStatusUpdateCommandText, connection, transaction))
                            {
                                // Bind parameters for CurrentStatus update
                                currentStatusCommand.Parameters.AddWithValue("@PartNo", model.PartNo);
                                currentStatusCommand.Parameters.AddWithValue("@LotNo", model.LotNo);
                                currentStatusCommand.Parameters.AddWithValue("@Status", model.Status.ToString()); // Assuming you want to update this as well
                                currentStatusCommand.Parameters.AddWithValue("@OutputQty", model.OutputQty);
                                currentStatusCommand.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);

                                var currentStatusResult = await currentStatusCommand.ExecuteNonQueryAsync();
                                if (currentStatusResult > 0)
                                {
                                    transaction.Commit();
                                    TempData["Success"] = "RePressing process and current status updated successfully.";
                                    return RedirectToAction("Index1");
                                }
                                else
                                {
                                    transaction.Rollback();
                                    ModelState.AddModelError("", "Failed to update current status.");
                                    await PrepareViewModelForEdit(model);
                                    return View(model);
                                }
                            }
                        }
                        else
                        {
                            transaction.Rollback();
                            ModelState.AddModelError("", "No records were updated in the RePressing table.");
                            await PrepareViewModelForEdit(model);
                            return View(model);
                        }
                    }
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int Re2PressingId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Super Admin"))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            string commandText = "DELETE FROM Re2Pressing WHERE Re2PressingId = @Re2PressingId";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@Re2PressingId", Re2PressingId);

                    int result = await command.ExecuteNonQueryAsync();
                    if (result > 0)
                    {
                        // Record was successfully deleted
                        _logger.LogInformation($"Deleted Re2Pressing with ID {Re2PressingId}.");
                        // Redirect to Index1 action
                        return RedirectToAction("Index1");
                    }
                    else
                    {
                        // Record not found or already deleted
                        return Json(new { success = false, message = "Item not found or already deleted." });
                    }
                }
            }
        }

    }
}


