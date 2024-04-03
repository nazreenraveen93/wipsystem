using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient; // Ensure you have this using directive
using Microsoft.EntityFrameworkCore;
using System.Data;
using WIPSystem.Web.Data;
using WIPSystem.Web.Models;
using WIPSystem.Web.ViewModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;

namespace WIPSystem.Web.Controllers
{
    public class CamberController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CamberController> _logger;

        public CamberController(WIPDbContext wIPDbContext, IConfiguration configuration, ILogger<CamberController> logger)
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
            var query = "SELECT CurrentStatusId, PartNo, LotNo, ReceivedQuantity,Date, Status FROM CurrentStatus WHERE ProcessCurrentStatus = 'Camber Selec' AND Status = 'In Progress'";


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
            SELECT CamberId, PartNo, LotNo, OutputQty, Status 
            FROM Camber 
            WHERE Status IN ('Completed', 'On Hold')";

            var model = new List<CamberViewModel>(); // Assuming CurrentStatus is your model class
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new CamberViewModel
                        {
                            CamberId = reader.GetInt32(reader.GetOrdinal("CamberId")),
                            PartNo = reader.IsDBNull(reader.GetOrdinal("PartNo")) ? null : reader.GetString(reader.GetOrdinal("PartNo")),
                            LotNo = reader.IsDBNull(reader.GetOrdinal("LotNo")) ? null : reader.GetString(reader.GetOrdinal("LotNo")),
                            OutputQty = reader.IsDBNull(reader.GetOrdinal("OutputQty")) ? 0 : reader.GetInt32(reader.GetOrdinal("OutputQty")),
                            Status = reader.IsDBNull(reader.GetOrdinal("Status")) ?
                             CamberProcessStatus.Completed : // or some default status
                             EnumExtensions.ConvertToEnum<CamberProcessStatus>(reader.GetString(reader.GetOrdinal("Status"))),

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
            var viewModel = new CamberViewModel
            {
                CurrentStatusId = CurrentStatusId,
                ProcessCurrentStatus = "Camber Selec"
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CamberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PrepareViewModel(model);
                return View(model);
            }

            try
            {
                using (var sqlConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await sqlConnection.OpenAsync();
                    //Console.WriteLine($"MachineId: {machineId}");
                    using (var transaction = sqlConnection.BeginTransaction())
                    {
                        try
                        {
                            // Step 1: Retrieve Machine ID
                            int machineId = await GetMachineId(sqlConnection, model.SelectedMachineOption, model.ProcessCurrentStatus, transaction);

                            // Step 2: Insert into Camber Table
                            string insertCommandText = @"
                        INSERT INTO Camber (PartNo, LotNo,Date,ReceivedQuantity,InputQty, ProcessCurrentStatus, CheckedBy,Status,Remarks,MachineId,AvgThickness,PieceWeight,Range,OutputQty,MachineRejectQty,CamberQty,Yield,RejectRate,TotalQty,TotalWeight,MachineStartTime,MachineEndTime)
                        VALUES (@PartNo, @LotNo,@Date,@ReceivedQuantity, @InputQty, @ProcessCurrentStatus, @CheckedBy,@Status,@Remarks,@MachineId,@AvgThickness,@PieceWeight,@Range,@OutputQty,@MachineRejectQty,@CamberQty,@Yield,@RejectRate,@TotalQty,@TotalWeight,@MachineStartTime,@MachineEndTime)";

                            using (var insertCommand = new SqlCommand(insertCommandText, sqlConnection, transaction))
                            {

                                insertCommand.Parameters.AddWithValue("@PartNo", model.PartNo);
                                insertCommand.Parameters.AddWithValue("@LotNo", model.LotNo);
                                insertCommand.Parameters.AddWithValue("@Date", DateTime.Now);
                                insertCommand.Parameters.AddWithValue("@ReceivedQuantity", model.ReceivedQuantity);
                                insertCommand.Parameters.AddWithValue("@ProcessCurrentStatus", model.ProcessCurrentStatus);
                                insertCommand.Parameters.AddWithValue("@CheckedBy", model.CheckedBy);
                                insertCommand.Parameters.AddWithValue("@Status", ((Enum)model.Status).GetDisplayName());
                                insertCommand.Parameters.AddWithValue("@Remarks", (object)model.Remarks ?? DBNull.Value);
                                insertCommand.Parameters.AddWithValue("@MachineId", machineId);
                                insertCommand.Parameters.AddWithValue("@OutputQty", model.OutputQty);
                                insertCommand.Parameters.AddWithValue("@MachineStartTime", model.MachineStartTime);
                                insertCommand.Parameters.AddWithValue("@MachineEndTime", model.MachineEndTime);
                                insertCommand.Parameters.AddWithValue("@AvgThickness", model.AvgThickness);
                                insertCommand.Parameters.AddWithValue("@PieceWeight", model.PieceWeight);
                                insertCommand.Parameters.AddWithValue("@Range", model.Range);
                                insertCommand.Parameters.AddWithValue("@MachineRejectQty", model.MachineRejectQty);
                                insertCommand.Parameters.AddWithValue("@CamberQty", model.CamberQty);
                                insertCommand.Parameters.AddWithValue("@Yield", model.Yield);
                                insertCommand.Parameters.AddWithValue("@RejectRate", model.RejectRate);
                                insertCommand.Parameters.AddWithValue("@TotalQty", model.TotalQty);
                                insertCommand.Parameters.AddWithValue("@TotalWeight", model.TotalWeight);
                               
                                insertCommand.Parameters.AddWithValue("@InputQty", model.InputQty);

                                int rowsInserted = await insertCommand.ExecuteNonQueryAsync();

                                if (rowsInserted > 0)
                                {
                                    // Step 3: Update CurrentStatus Table
                                    string updateCurrentStatusQuery = @"
                                UPDATE CurrentStatus
                                SET Status = @Status, 
                                    ReceivedQuantity = @ReceivedQuantity, 
                                    Remarks = @Remarks, 
                                    PIC = @PIC, 
                                    Date = @Date
                                WHERE PartNo = @PartNo AND LotNo = @LotNo";

                                    using (var updateCommand = new SqlCommand(updateCurrentStatusQuery, sqlConnection, transaction))
                                    {
                                        updateCommand.Parameters.AddWithValue("@Status", ((Enum)model.Status).GetDisplayName());
                                        updateCommand.Parameters.AddWithValue("@ReceivedQuantity", model.OutputQty);
                                        updateCommand.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                                        updateCommand.Parameters.AddWithValue("@PIC", model.CheckedBy);
                                        updateCommand.Parameters.AddWithValue("@Date", DateTime.Now);
                                        updateCommand.Parameters.AddWithValue("@PartNo", model.PartNo);
                                        updateCommand.Parameters.AddWithValue("@LotNo", model.LotNo);

                                        int rowsUpdated = await updateCommand.ExecuteNonQueryAsync();

                                        if (rowsUpdated == 1)
                                        {
                                            transaction.Commit();
                                            TempData["Success"] = "Data inserted and updated successfully.";
                                            return RedirectToAction("Index1");
                                        }
                                        else
                                        {
                                            transaction.Rollback();
                                            ModelState.AddModelError("", "Unable to update the record in the CurrentStatus table.");
                                        }
                                    }
                                }
                                else
                                {
                                    transaction.Rollback();
                                    ModelState.AddModelError("", "No records were inserted into the Camber table.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _logger.LogError(ex, "An error occurred while creating a Camber record.");
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

        private void PrepareViewModel(CamberViewModel model)
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

        public async Task<IActionResult> CamberDetails(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            CamberViewModel viewModel = null;

            string commandText = @"
                SELECT 
                    c.CamberId, 
                    c.PartNo, 
                    c.LotNo, 
                    c.ProcessCurrentStatus,
                    c.CheckedBy,
                    c.Status,
                    c.Remarks,
                    c.MachineId,
                    c.OutputQty, 
                    c.AvgThickness,
                    c.PieceWeight,
                    c.Range,
                    c.MachineRejectQty,
                    c.CamberQty,
                    c.Yield,
                    c.RejectRate,
                    c.TotalQty,
                    c.TotalWeight,
                    c.MachineStartTime, 
                    c.MachineEndTime,
                    c.ReceivedQuantity,
                    c.InputQty,
                    m.MachineName,
                    p.CustName AS CustomerName,
                    p.PackageSize,
                    p.PiecesPerBlank
                FROM Camber c
                INNER JOIN Machines m ON c.MachineId = m.MachineId
                INNER JOIN Products p ON c.PartNo = p.PartNo
                WHERE c.CamberId = @CamberId";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var command = new SqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@CamberId", id);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        viewModel = new CamberViewModel
                        {
                            CamberId = reader.GetInt32(reader.GetOrdinal("CamberId")),
                            PartNo = reader.GetString(reader.GetOrdinal("PartNo")),
                            LotNo = reader.GetString(reader.GetOrdinal("LotNo")),
                            ProcessCurrentStatus = reader.GetString(reader.GetOrdinal("ProcessCurrentStatus")),
                            CheckedBy = reader.GetString(reader.GetOrdinal("CheckedBy")),
                            Status = EnumExtensions.ConvertToEnum<CamberProcessStatus>(reader.GetString(reader.GetOrdinal("Status"))),
                            Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                            MachineId = reader.GetInt32(reader.GetOrdinal("MachineId")), // This line is no longer needed since you want the MachineName
                            MachineName = reader.GetString(reader.GetOrdinal("MachineName")),
                            OutputQty = reader.GetInt32(reader.GetOrdinal("OutputQty")),
                            AvgThickness = reader.GetDecimal(reader.GetOrdinal("AvgThickness")),
                            PieceWeight = reader.GetDecimal(reader.GetOrdinal("PieceWeight")), // Add this line
                            Range = reader.GetString(reader.GetOrdinal("Range")), // Add this line
                            MachineRejectQty = reader.GetInt32(reader.GetOrdinal("MachineRejectQty")), // Add this line
                            CamberQty = reader.GetInt32(reader.GetOrdinal("CamberQty")), // Add this line
                            Yield = reader.GetDecimal(reader.GetOrdinal("Yield")), // Add this line
                            RejectRate = reader.GetDecimal(reader.GetOrdinal("RejectRate")), // Add this line
                            TotalQty = reader.GetInt32(reader.GetOrdinal("TotalQty")), // Add this line
                            TotalWeight = reader.GetDecimal(reader.GetOrdinal("TotalWeight")), // Add this line
                            MachineStartTime = reader.GetDateTime(reader.GetOrdinal("MachineStartTime")),
                            MachineEndTime = reader.GetDateTime(reader.GetOrdinal("MachineEndTime")),
                            ReceivedQuantity = reader.GetInt32(reader.GetOrdinal("ReceivedQuantity")),
                            InputQty = reader.GetInt32(reader.GetOrdinal("InputQty")), // Add this line
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

            return PartialView("_CamberDetailsModal", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            CamberViewModel viewModel = new CamberViewModel();

            string commandText = @"
        SELECT
            c.CamberId, 
            c.PartNo, 
            c.LotNo, 
            c.ProcessCurrentStatus,
            c.CheckedBy,
            c.Status,
            c.Remarks,
            c.MachineId,
            c.OutputQty, 
            c.AvgThickness,
            c.PieceWeight,
            c.Range,
            c.MachineRejectQty,
            c.CamberQty,
            c.Yield,
            c.RejectRate,
            c.TotalQty,
            c.TotalWeight,
            c.MachineStartTime, 
            c.MachineEndTime,
            c.ReceivedQuantity,
            c.InputQty,
            m.MachineName,
            p.CustName AS CustomerName,
            p.PackageSize,
            p.PiecesPerBlank
        FROM Camber c
        INNER JOIN Machines m ON c.MachineId = m.MachineId
        INNER JOIN Products p ON c.PartNo = p.PartNo
        WHERE c.CamberId = @CamberId";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@CamberId", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            viewModel.CamberId = reader.GetInt32(reader.GetOrdinal("CamberId"));
                            viewModel.PartNo = reader.GetString(reader.GetOrdinal("PartNo"));
                            viewModel.LotNo = reader.GetString(reader.GetOrdinal("LotNo"));
                            viewModel.ProcessCurrentStatus = reader.GetString(reader.GetOrdinal("ProcessCurrentStatus"));
                            viewModel.CheckedBy = reader.GetString(reader.GetOrdinal("CheckedBy"));

                            string statusString = reader.GetString(reader.GetOrdinal("Status"));
                            if (Enum.TryParse<CamberProcessStatus>(statusString, true, out var parsedStatus))
                            {
                                viewModel.Status = parsedStatus;
                            }
                            else
                            {
                                viewModel.Status = CamberProcessStatus.OnHold; // Set default value
                            }

                            viewModel.Remarks = reader.GetString(reader.GetOrdinal("Remarks"));
                            viewModel.MachineId = reader.GetInt32(reader.GetOrdinal("MachineId"));
                            viewModel.OutputQty = reader.GetInt32(reader.GetOrdinal("OutputQty"));
                            viewModel.AvgThickness = reader.GetDecimal(reader.GetOrdinal("AvgThickness"));
                            viewModel.PieceWeight = reader.GetDecimal(reader.GetOrdinal("PieceWeight"));
                            viewModel.Range = reader.GetString(reader.GetOrdinal("Range"));
                            viewModel.MachineRejectQty = reader.GetInt32(reader.GetOrdinal("MachineRejectQty"));
                            viewModel.CamberQty = reader.GetInt32(reader.GetOrdinal("CamberQty"));
                            viewModel.Yield = reader.GetDecimal(reader.GetOrdinal("Yield"));
                            viewModel.RejectRate = reader.GetDecimal(reader.GetOrdinal("RejectRate"));
                            viewModel.TotalQty = reader.GetInt32(reader.GetOrdinal("TotalQty"));
                            viewModel.TotalWeight = reader.GetDecimal(reader.GetOrdinal("TotalWeight"));
                            viewModel.MachineStartTime = reader.GetDateTime(reader.GetOrdinal("MachineStartTime"));
                            viewModel.MachineEndTime = reader.GetDateTime(reader.GetOrdinal("MachineEndTime"));
                            viewModel.ReceivedQuantity = reader.GetInt32(reader.GetOrdinal("ReceivedQuantity"));
                            viewModel.InputQty = reader.GetInt32(reader.GetOrdinal("InputQty"));
                            viewModel.MachineName = reader.GetString(reader.GetOrdinal("MachineName"));
                            viewModel.CustomerName = reader.GetString(reader.GetOrdinal("CustomerName"));
                            viewModel.PackageSize = reader.GetString(reader.GetOrdinal("PackageSize"));
                            viewModel.PiecesPerBlank = reader.GetInt32(reader.GetOrdinal("PiecesPerBlank"));

                            //viewModel.SelectedMachineId = viewModel.MachineId; // Assuming MachineId holds the selected machine ID


                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }

            await PrepareViewModelForEdit(viewModel);

            return View(viewModel);
        }

        private async Task PrepareViewModelForEdit(CamberViewModel model)
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
        public async Task<IActionResult> Edit(CamberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareViewModelForEdit(model);
                return View(model);
            }
            string camberUpdateCommandText = @"
            UPDATE Camber
            SET 
                MachineStartTime = @MachineStartTime,
                MachineEndTime = @MachineEndTime,
                InputQty = @InputQty,
                AvgThickness = @AvgThickness,
                PieceWeight = @PieceWeight,
                Range = @Range,
                MachineRejectQty = @MachineRejectQty,
                CamberQty = @CamberQty,
                Yield = @Yield,
                RejectRate = @RejectRate,
                TotalQty = @TotalQty,
                TotalWeight = @TotalWeight,
                OutputQty = @OutputQty,
                Remarks = @Remarks,
                Status = @Status
            WHERE CamberId = @CamberId";

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
                    using (var camberCommand = new SqlCommand(camberUpdateCommandText, connection, transaction))
                    {
                        // Bind parameters for Camber update
                        camberCommand.Parameters.AddWithValue("@CamberId", model.CamberId);
                        camberCommand.Parameters.AddWithValue("@MachineStartTime", model.MachineStartTime);
                        camberCommand.Parameters.AddWithValue("@MachineEndTime", model.MachineEndTime);
                        camberCommand.Parameters.AddWithValue("@InputQty", model.InputQty);
                        camberCommand.Parameters.AddWithValue("@AvgThickness", model.AvgThickness);
                        camberCommand.Parameters.AddWithValue("@PieceWeight", model.PieceWeight);
                        camberCommand.Parameters.AddWithValue("@Range", model.Range);
                        camberCommand.Parameters.AddWithValue("@MachineRejectQty", model.MachineRejectQty);
                        camberCommand.Parameters.AddWithValue("@CamberQty", model.CamberQty);
                        camberCommand.Parameters.AddWithValue("@Yield", model.Yield);
                        camberCommand.Parameters.AddWithValue("@RejectRate", model.RejectRate);
                        camberCommand.Parameters.AddWithValue("@TotalQty", model.TotalQty);
                        camberCommand.Parameters.AddWithValue("@TotalWeight", model.TotalWeight);
                        camberCommand.Parameters.AddWithValue("@OutputQty", model.OutputQty);
                        camberCommand.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                        camberCommand.Parameters.AddWithValue("@Status", model.Status.ToString());


                        var breakingResult = await camberCommand.ExecuteNonQueryAsync();

                        if (breakingResult > 0)
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
                                    TempData["Success"] = "Camber process and current status updated successfully.";
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
                            ModelState.AddModelError("", "No records were updated in the Camber table.");
                            await PrepareViewModelForEdit(model);
                            return View(model);
                        }
                    }
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int CamberId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Super Admin"))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            string commandText = "DELETE FROM Camber WHERE CamberId = @CamberId";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@CamberId", CamberId);

                    int result = await command.ExecuteNonQueryAsync();
                    if (result > 0)
                    {
                        // Record was successfully deleted
                        _logger.LogInformation($"Deleted Camber with ID {CamberId}.");
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
