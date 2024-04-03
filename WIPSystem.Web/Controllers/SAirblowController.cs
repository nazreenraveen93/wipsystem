using Microsoft.AspNetCore.Mvc;
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
    public class SAirblowController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SAirblowController> _logger;

        public SAirblowController(WIPDbContext wIPDbContext, IConfiguration configuration, ILogger<SAirblowController> logger)
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
            var query = "SELECT CurrentStatusId, PartNo, LotNo, ReceivedQuantity,Date, Status FROM CurrentStatus WHERE ProcessCurrentStatus = 'S.Airblow' AND Status = 'In Progress'";


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
            SELECT SAirblowId, PartNo, LotNo, OutputQty, Status 
            FROM SAirblow 
            WHERE Status IN ('Completed', 'On Hold')";

            var model = new List<SAirblowViewModel>(); // Assuming CurrentStatus is your model class
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new SAirblowViewModel
                        {
                            SAirblowId = reader.GetInt32(reader.GetOrdinal("SAirblowId")),
                            PartNo = reader.IsDBNull(reader.GetOrdinal("PartNo")) ? null : reader.GetString(reader.GetOrdinal("PartNo")),
                            LotNo = reader.IsDBNull(reader.GetOrdinal("LotNo")) ? null : reader.GetString(reader.GetOrdinal("LotNo")),
                            OutputQty = reader.IsDBNull(reader.GetOrdinal("OutputQty")) ? 0 : reader.GetInt32(reader.GetOrdinal("OutputQty")),
                            Status = reader.IsDBNull(reader.GetOrdinal("Status")) ?
                             SAirblowProcessStatus.Completed : // or some default status
                             EnumExtensions.ConvertToEnum<SAirblowProcessStatus>(reader.GetString(reader.GetOrdinal("Status"))),

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
            var viewModel = new SAirblowViewModel
            {
                CurrentStatusId = CurrentStatusId,
                ProcessCurrentStatus = "S.Airblow"
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

            // Retrieve MachineId and MachineName from the Machines table
            viewModel.MachineOptions = _wIPDbContext.Machines
                .Where(m => m.ProcessId == _wIPDbContext.Process.FirstOrDefault(p => p.ProcessName == viewModel.ProcessCurrentStatus).ProcessId)
                .Select(m => new SelectListItem
                {
                    Value = m.MachineName,
                    Text = m.MachineName
                }).ToList();

            // Set the CheckedBy property based on the authenticated user
            viewModel.CheckedBy = User.Identity.Name;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SAirblowViewModel model)
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
                            int machineId = await GetMachineId(connection, model.SelectedMachineOption, model.ProcessCurrentStatus, transaction);

                            string insertCommandText = @"
                        INSERT INTO SAirblow
                        (PartNo, LotNo, ProcessCurrentStatus, CheckedBy, Status, Remarks, MachineId, OutputQty, RejectQty, MachineStartTime, MachineEndTime, Date)
                        VALUES
                        (@PartNo, @LotNo, @ProcessCurrentStatus, @CheckedBy, @Status, @Remarks, @MachineId, @OutputQty, @RejectQty, @MachineStartTime, @MachineEndTime, @Date)";

                            using (var insertCommand = new SqlCommand(insertCommandText, connection, transaction))
                            {
                                insertCommand.Parameters.AddWithValue("@PartNo", model.PartNo);
                                insertCommand.Parameters.AddWithValue("@LotNo", model.LotNo);
                                insertCommand.Parameters.AddWithValue("@ProcessCurrentStatus", model.ProcessCurrentStatus);
                                insertCommand.Parameters.AddWithValue("@CheckedBy", model.CheckedBy);
                                // Convert enum to string before assigning to command parameter
                                insertCommand.Parameters.AddWithValue("@Status", ((Enum)model.Status).GetDisplayName());
                                insertCommand.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                                insertCommand.Parameters.AddWithValue("@MachineId", machineId);
                                insertCommand.Parameters.AddWithValue("@OutputQty", model.OutputQty);
                                insertCommand.Parameters.AddWithValue("@RejectQty", model.RejectQty);
                                insertCommand.Parameters.AddWithValue("@MachineStartTime", model.MachineStartTime);
                                insertCommand.Parameters.AddWithValue("@MachineEndTime", model.MachineEndTime);
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
                                    ModelState.AddModelError("", "No records were inserted into the SAirblow table.");
                                    // Optionally roll back the transaction here as well
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback(); // Roll back the transaction on error
                            _logger.LogError(ex, "An error occurred while creating a SAirblow record.");
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


        private async Task<int> GetMachineId(SqlConnection connection, string machineName, string processStatus, SqlTransaction transaction)
        {
            string machineQuery = "SELECT TOP 1 MachineId FROM Machines WHERE MachineName = @MachineName";
            using (var machineCommand = new SqlCommand(machineQuery, connection, transaction))
            {
                machineCommand.Parameters.AddWithValue("@MachineName", machineName);
                var machineResult = await machineCommand.ExecuteScalarAsync();
                if (machineResult != null)
                {
                    return (int)machineResult;
                }
                else
                {
                    throw new InvalidOperationException("No machine found with the given name.");
                }
            }
        }

        private void PrepareViewModel(SAirblowViewModel model)
        {
            // Retrieve the process ID based on the current status name.
            var processId = _wIPDbContext.Process
                                         .Where(p => p.ProcessName == model.ProcessCurrentStatus)
                                         .Select(p => p.ProcessId)
                                         .FirstOrDefault();

            // Now filter machines that are associated with this process ID.
            model.MachineOptions = _wIPDbContext.Machines
                .Where(m => m.ProcessId == processId) // Ensure you have a ProcessId foreign key in Machine model
                .AsNoTracking()
                .Select(m => new SelectListItem { Value = m.MachineId.ToString(), Text = m.MachineName })
                .ToList();
        }

        public async Task<IActionResult> SAirblowDetails(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            SAirblowViewModel viewModel = null;

            string commandText = @"
                SELECT 
                    s.SAirblowId, 
                    s.PartNo, 
                    s.LotNo, 
                    s.ProcessCurrentStatus,
                    s.CheckedBy,
                    s.Status,
                    s.Remarks,
                    s.MachineId,
                    s.OutputQty, 
                    s.RejectQty, 
                    s.MachineStartTime, 
                    s.MachineEndTime,
                    m.MachineName,
                    p.CustName AS CustomerName,
                    p.PackageSize,
                    p.PiecesPerBlank
                FROM SAirblow s
                INNER JOIN Machines m ON s.MachineId = m.MachineId
                INNER JOIN Products p ON s.PartNo = p.PartNo
                WHERE s.SAirblowId = @SAirblowId";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var command = new SqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@SAirblowId", id);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        viewModel = new SAirblowViewModel
                        {
                            SAirblowId = reader.GetInt32(reader.GetOrdinal("SAirblowId")),
                            PartNo = reader.GetString(reader.GetOrdinal("PartNo")),
                            LotNo = reader.GetString(reader.GetOrdinal("LotNo")),
                            ProcessCurrentStatus = reader.GetString(reader.GetOrdinal("ProcessCurrentStatus")),
                            CheckedBy = reader.GetString(reader.GetOrdinal("CheckedBy")),
                            Status = EnumExtensions.ConvertToEnum<SAirblowProcessStatus>(reader.GetString(reader.GetOrdinal("Status"))),
                            Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                            MachineId = reader.GetInt32(reader.GetOrdinal("MachineId")), // This line is no longer needed since you want the MachineName
                            MachineName = reader.GetString(reader.GetOrdinal("MachineName")),
                            OutputQty = reader.GetInt32(reader.GetOrdinal("OutputQty")),
                            RejectQty = reader.GetInt32(reader.GetOrdinal("RejectQty")),
                            MachineStartTime = reader.GetDateTime(reader.GetOrdinal("MachineStartTime")),
                            MachineEndTime = reader.GetDateTime(reader.GetOrdinal("MachineEndTime")),
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

            return PartialView("_SAirblowDetailsModal", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            SAirblowViewModel viewModel = new SAirblowViewModel();

            string commandText = @"
                SELECT
                 s.SAirblowId, 
                    s.PartNo, 
                    s.LotNo, 
                    s.ProcessCurrentStatus,
                    s.CheckedBy,
                    s.Status,
                    s.Remarks,
                    s.MachineId,
                    s.OutputQty, 
                    s.RejectQty, 
                    s.MachineStartTime, 
                    s.MachineEndTime,
                    m.MachineName,
                    p.CustName AS CustomerName,
                    p.PackageSize,
                    p.PiecesPerBlank,
               cs.ReceivedQuantity
            FROM SAirblow s
            INNER JOIN Machines m ON s.MachineId = m.MachineId
            INNER JOIN Products p ON s.PartNo = p.PartNo 
            LEFT JOIN CurrentStatus cs ON s.PartNo = cs.PartNo AND s.LotNo = cs.LotNo
            WHERE s.SAirblowId = @SAirblowId";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@SAirblowId", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            viewModel.SAirblowId = reader.GetInt32(reader.GetOrdinal("SAirblowId"));
                            viewModel.PartNo = reader.GetString(reader.GetOrdinal("PartNo"));
                            viewModel.LotNo = reader.GetString(reader.GetOrdinal("LotNo"));
                            viewModel.ProcessCurrentStatus = reader.GetString(reader.GetOrdinal("ProcessCurrentStatus"));
                            viewModel.CheckedBy = reader.GetString(reader.GetOrdinal("CheckedBy"));
                            string statusString = reader.GetString(reader.GetOrdinal("Status"));
                            if (Enum.TryParse<SAirblowProcessStatus>(statusString, true, out var parsedStatus))
                            {
                                viewModel.Status = parsedStatus;
                            }
                            else
                            {
                                // Handle the case where parsing fails. Log an error, throw an exception, or set a default value.
                                // Setting a default value as an example:
                                viewModel.Status = SAirblowProcessStatus.OnHold; // Or another appropriate default value
                            }

                            viewModel.Remarks = reader.GetString(reader.GetOrdinal("Remarks"));
                            viewModel.MachineId = reader.GetInt32(reader.GetOrdinal("MachineId"));
                            viewModel.OutputQty = reader.GetInt32(reader.GetOrdinal("OutputQty"));
                            viewModel.RejectQty = reader.GetInt32(reader.GetOrdinal("RejectQty"));
                            viewModel.MachineStartTime = reader.GetDateTime(reader.GetOrdinal("MachineStartTime"));
                            viewModel.MachineEndTime = reader.GetDateTime(reader.GetOrdinal("MachineEndTime"));
                            viewModel.MachineName = reader.GetString(reader.GetOrdinal("MachineName"));
                            viewModel.CustomerName = reader.GetString(reader.GetOrdinal("CustomerName"));
                            viewModel.PackageSize = reader.GetString(reader.GetOrdinal("PackageSize"));
                            viewModel.PiecesPerBlank = reader.GetInt32(reader.GetOrdinal("PiecesPerBlank"));
                            viewModel.ReceivedQuantity = reader.GetInt32(reader.GetOrdinal("ReceivedQuantity"));
                            viewModel.SelectedMachineOption = viewModel.MachineId.ToString();
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

        private async Task PrepareViewModelForEdit(SAirblowViewModel model)
        {
            // Retrieve the process ID based on the current status name.
            var processId = await _wIPDbContext.Process
                                               .Where(p => p.ProcessName == model.ProcessCurrentStatus)
                                               .Select(p => p.ProcessId)
                                               .FirstOrDefaultAsync();

            // Filter machines that are associated with this process ID.
            model.MachineOptions = await _wIPDbContext.Machines
                                                      .Where(m => m.ProcessId == processId)
                                                      .Select(m => new SelectListItem
                                                      {
                                                          Value = m.MachineId.ToString(),
                                                          Text = m.MachineName
                                                      })
                                                      .ToListAsync();

            // This assumes that when editing, the machine ID will be part of the model you fetch.
            // If the MachineId is not in the model, you need to set it from the database before setting SelectedMachineOption.
            model.SelectedMachineOption = model.MachineId.ToString();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SAirblowViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareViewModelForEdit(model);
                return View(model);
            }

            string niplatingUpdateCommandText = @"
        UPDATE SAirblow
        SET OutputQty = @OutputQty, RejectQty = @RejectQty,
            Remarks = @Remarks, Status = @Status
        WHERE SAirblowId = @SAirblowId";

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
                    using (var niplatingCommand = new SqlCommand(niplatingUpdateCommandText, connection, transaction))
                    {
                        // Bind parameters for Sinter update
                        niplatingCommand.Parameters.AddWithValue("@SAirblowId", model.SAirblowId);
                        niplatingCommand.Parameters.AddWithValue("@OutputQty", model.OutputQty);
                        niplatingCommand.Parameters.AddWithValue("@RejectQty", model.RejectQty);
                        niplatingCommand.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value); // Handle potential NULL value
                        niplatingCommand.Parameters.AddWithValue("@Status", model.Status.ToString()); // Convert enum to string

                        var sinterResult = await niplatingCommand.ExecuteNonQueryAsync();

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
                                    TempData["Success"] = "SAirblow process and current status updated successfully.";
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
                            ModelState.AddModelError("", "No records were updated in the SAirblow table.");
                            await PrepareViewModelForEdit(model);
                            return View(model);
                        }
                    }
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int SAirblowId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Super Admin"))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            string commandText = "DELETE FROM SAirblow WHERE SAirblowId = @SAirblowId";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@SAirblowId", SAirblowId);

                    int result = await command.ExecuteNonQueryAsync();
                    if (result > 0)
                    {
                        // Record was successfully deleted
                        _logger.LogInformation($"Deleted SAirblowId with ID {SAirblowId}.");
                        return Json(new { success = true, message = "Record deleted successfully." });
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
