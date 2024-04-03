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
    public class BreakingController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BreakingController> _logger;

        public BreakingController(WIPDbContext wIPDbContext, IConfiguration configuration, ILogger<BreakingController> logger)
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
            var query = "SELECT CurrentStatusId, PartNo, LotNo, ReceivedQuantity,Date, Status FROM CurrentStatus WHERE ProcessCurrentStatus = 'Breaking' AND Status = 'In Progress'";


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
            SELECT BreakingId, PartNo, LotNo, OutputQty, Status 
            FROM Breaking 
            WHERE Status IN ('Completed', 'On Hold')";

            var model = new List<BreakingViewModel>(); // Assuming CurrentStatus is your model class
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new BreakingViewModel
                        {
                            BreakingId = reader.GetInt32(reader.GetOrdinal("BreakingId")),
                            PartNo = reader.IsDBNull(reader.GetOrdinal("PartNo")) ? null : reader.GetString(reader.GetOrdinal("PartNo")),
                            LotNo = reader.IsDBNull(reader.GetOrdinal("LotNo")) ? null : reader.GetString(reader.GetOrdinal("LotNo")),
                            OutputQty = reader.IsDBNull(reader.GetOrdinal("OutputQty")) ? 0 : reader.GetInt32(reader.GetOrdinal("OutputQty")),
                            Status = reader.IsDBNull(reader.GetOrdinal("Status")) ?
                             BreakingProcessStatus.Completed : // or some default status
                             EnumExtensions.ConvertToEnum<BreakingProcessStatus>(reader.GetString(reader.GetOrdinal("Status"))),

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
            var viewModel = new BreakingViewModel
            {
                CurrentStatusId = CurrentStatusId,
                ProcessCurrentStatus = "Breaking"
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
        public async Task<IActionResult> Create(BreakingViewModel model)
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

                            // Step 2: Insert into Breaking Table
                            string insertCommandText = @"
                        INSERT INTO Breaking (PartNo, LotNo,Date, ProcessCurrentStatus, CheckedBy,Status,Remarks,MachineId,MachineType,OutputQty,RejectQty,ChippingQty,SheetBreakQty,CrackQty,OthersQty,DifferencesQty,MachineStartTime,MachineEndTime,FirstBreak,SecondBreak,TargetOne,TargetTwo)
                        VALUES (@PartNo, @LotNo,@Date, @ProcessCurrentStatus, @CheckedBy,@Status,@Remarks,@MachineId,@MachineType,@OutputQty,@RejectQty,@ChippingQty,@SheetBreakQty,@CrackQty,@OthersQty,@DifferencesQty,@MachineStartTime,@MachineEndTime,@FirstBreak,@SecondBreak,@TargetOne,@TargetTwo)";

                            using (var insertCommand = new SqlCommand(insertCommandText, sqlConnection, transaction))
                            {
                                insertCommand.Parameters.AddWithValue("@PartNo", model.PartNo);
                                insertCommand.Parameters.AddWithValue("@LotNo", model.LotNo);
                                insertCommand.Parameters.AddWithValue("@Date", DateTime.Now);
                                insertCommand.Parameters.AddWithValue("@ProcessCurrentStatus", model.ProcessCurrentStatus);
                                insertCommand.Parameters.AddWithValue("@CheckedBy", model.CheckedBy);
                                insertCommand.Parameters.AddWithValue("@Status", ((Enum)model.Status).GetDisplayName());
                                insertCommand.Parameters.AddWithValue("@Remarks", (object)model.Remarks ?? DBNull.Value);
                                insertCommand.Parameters.AddWithValue("@MachineId", machineId);
                                insertCommand.Parameters.AddWithValue("@MachineType", model.MachineType);
                                insertCommand.Parameters.AddWithValue("@OutputQty", model.OutputQty);
                                insertCommand.Parameters.AddWithValue("@RejectQty", model.RejectQty);
                                insertCommand.Parameters.AddWithValue("@DifferencesQty", model.DifferencesQty);
                                insertCommand.Parameters.AddWithValue("@ChippingQty", model.ChippingQty);
                                insertCommand.Parameters.AddWithValue("@SheetBreakQty", model.SheetBreakQty);
                                insertCommand.Parameters.AddWithValue("@CrackQty", model.CrackQty);
                                insertCommand.Parameters.AddWithValue("@OthersQty", model.OthersQty);
                                insertCommand.Parameters.AddWithValue("@MachineStartTime", model.MachineStartTime);
                                insertCommand.Parameters.AddWithValue("@MachineEndTime", model.MachineEndTime);
                                insertCommand.Parameters.AddWithValue("@FirstBreak", (object)model.FirstBreak ?? DBNull.Value);
                                insertCommand.Parameters.AddWithValue("@TargetOne", (object)model.TargetOne ?? DBNull.Value);
                                insertCommand.Parameters.AddWithValue("@SecondBreak", (object)model.SecondBreak ?? DBNull.Value);
                                insertCommand.Parameters.AddWithValue("@TargetTwo", (object)model.TargetTwo ?? DBNull.Value);
                               
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
                                    ModelState.AddModelError("", "No records were inserted into the Breaking table.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _logger.LogError(ex, "An error occurred while creating a Breaking record.");
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

        private void PrepareViewModel(BreakingViewModel model)
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


        public async Task<IActionResult> BreakingDetails(int id)
        {
            BreakingViewModel viewModel = null;

            string commandText = @"
        SELECT 
            b.BreakingId, 
            b.PartNo, 
            b.LotNo, 
            b.ProcessCurrentStatus,
            b.CheckedBy,
            b.Status,
            b.Remarks,
            b.MachineId,
            b.MachineType,
            b.OutputQty, 
            b.RejectQty, 
            b.DifferencesQty,
            b.ChippingQty,
            b.SheetBreakQty,
            b.CrackQty,
            b.OthersQty,
            b.MachineStartTime, 
            b.MachineEndTime,
            b.FirstBreak,
            b.TargetOne,
            b.SecondBreak,
            b.TargetTwo,
            m.MachineName,
            p.CustName AS CustomerName,
            p.PackageSize,
            p.PiecesPerBlank
        FROM Breaking b
        INNER JOIN Machines m ON b.MachineId = m.MachineId
        INNER JOIN Products p ON b.PartNo = p.PartNo
        WHERE b.BreakingId = @BreakingId";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var command = new SqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@BreakingId", id);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        viewModel = new BreakingViewModel
                        {
                            BreakingId = reader.GetInt32(reader.GetOrdinal("BreakingId")),
                            PartNo = reader.GetString(reader.GetOrdinal("PartNo")),
                            LotNo = reader.GetString(reader.GetOrdinal("LotNo")),
                            ProcessCurrentStatus = reader.GetString(reader.GetOrdinal("ProcessCurrentStatus")),
                            CheckedBy = reader.GetString(reader.GetOrdinal("CheckedBy")),
                            Status = EnumExtensions.ConvertToEnum<BreakingProcessStatus>(reader.GetString(reader.GetOrdinal("Status"))),
                            Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                            MachineId = reader.GetInt32(reader.GetOrdinal("MachineId")),
                            MachineName = reader.GetString(reader.GetOrdinal("MachineName")),
                            MachineType = reader.GetString(reader.GetOrdinal("MachineType")),
                            OutputQty = reader.GetInt32(reader.GetOrdinal("OutputQty")),
                            RejectQty = reader.GetInt32(reader.GetOrdinal("RejectQty")),
                            DifferencesQty = reader.GetInt32(reader.GetOrdinal("DifferencesQty")),
                            ChippingQty = reader.GetInt32(reader.GetOrdinal("ChippingQty")),
                            SheetBreakQty = reader.GetInt32(reader.GetOrdinal("SheetBreakQty")),
                            CrackQty = reader.GetInt32(reader.GetOrdinal("CrackQty")),
                            OthersQty = reader.GetInt32(reader.GetOrdinal("OthersQty")),
                            MachineStartTime = reader.GetDateTime(reader.GetOrdinal("MachineStartTime")),
                            MachineEndTime = reader.GetDateTime(reader.GetOrdinal("MachineEndTime")),
                            FirstBreak = reader.GetString(reader.GetOrdinal("FirstBreak")),
                            TargetOne = reader.GetString(reader.GetOrdinal("TargetOne")),
                            SecondBreak = reader.GetString(reader.GetOrdinal("SecondBreak")),
                            TargetTwo = reader.GetString(reader.GetOrdinal("TargetTwo")),
                            CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                            PackageSize = reader.GetString(reader.GetOrdinal("PackageSize")),
                            PiecesPerBlank = reader.GetInt32(reader.GetOrdinal("PiecesPerBlank"))
                            // Add other properties as needed
                        };
                    }
                }
            }

            if (viewModel == null)
            {
                return NotFound();
            }

            return PartialView("_BreakingDetailsModal", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            BreakingViewModel viewModel = new BreakingViewModel();

            string commandText = @"
            SELECT 
            b.BreakingId, 
            b.PartNo, 
            b.LotNo, 
            b.ProcessCurrentStatus,
            b.CheckedBy,
            b.Status,
            b.Remarks,
            b.MachineId,
            b.MachineType,
            b.OutputQty, 
            b.RejectQty, 
            b.DifferencesQty,
            b.ChippingQty,
            b.SheetBreakQty,
            b.CrackQty,
            b.OthersQty,
            b.MachineStartTime, 
            b.MachineEndTime,
            b.FirstBreak,
            b.TargetOne,
            b.SecondBreak,
            b.TargetTwo,
            m.MachineName,
            p.CustName AS CustomerName,
            p.PackageSize,
            p.PiecesPerBlank
        FROM Breaking b
        INNER JOIN Machines m ON b.MachineId = m.MachineId
        INNER JOIN Products p ON b.PartNo = p.PartNo
        WHERE b.BreakingId = @BreakingId";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@BreakingId", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            viewModel.BreakingId = reader.GetInt32(reader.GetOrdinal("BreakingId"));
                            viewModel.PartNo = reader.GetString(reader.GetOrdinal("PartNo"));
                            viewModel.LotNo = reader.GetString(reader.GetOrdinal("LotNo"));
                            viewModel.ProcessCurrentStatus = reader.GetString(reader.GetOrdinal("ProcessCurrentStatus"));
                            viewModel.CheckedBy = reader.GetString(reader.GetOrdinal("CheckedBy"));
                            string statusString = reader.GetString(reader.GetOrdinal("Status"));
                            if (Enum.TryParse<BreakingProcessStatus>(statusString, true, out var parsedStatus))
                            {
                                viewModel.Status = parsedStatus;
                            }
                            else
                            {
                                // Handle the case where parsing fails. Log an error, throw an exception, or set a default value.
                                // Setting a default value as an example:
                                viewModel.Status = BreakingProcessStatus.OnHold; // Or another appropriate default value
                            }

                            viewModel.Remarks = reader.GetString(reader.GetOrdinal("Remarks"));
                            viewModel.MachineId = reader.GetInt32(reader.GetOrdinal("MachineId"));
                            viewModel.MachineType = reader.GetString(reader.GetOrdinal("MachineType"));
                            viewModel.OutputQty = reader.GetInt32(reader.GetOrdinal("OutputQty"));
                            viewModel.RejectQty = reader.GetInt32(reader.GetOrdinal("RejectQty"));
                            viewModel.DifferencesQty = reader.GetInt32(reader.GetOrdinal("DifferencesQty"));
                            viewModel.ChippingQty = reader.GetInt32(reader.GetOrdinal("ChippingQty"));
                            viewModel.SheetBreakQty = reader.GetInt32(reader.GetOrdinal("SheetBreakQty"));
                            viewModel.CrackQty = reader.GetInt32(reader.GetOrdinal("CrackQty"));
                            viewModel.OthersQty = reader.GetInt32(reader.GetOrdinal("OthersQty"));
                            viewModel.MachineStartTime = reader.GetDateTime(reader.GetOrdinal("MachineStartTime"));
                            viewModel.MachineEndTime = reader.GetDateTime(reader.GetOrdinal("MachineEndTime"));
                            viewModel.FirstBreak = reader.GetString(reader.GetOrdinal("FirstBreak"));
                            viewModel.TargetOne = reader.GetString(reader.GetOrdinal("TargetOne"));
                            viewModel.SecondBreak = reader.GetString(reader.GetOrdinal("SecondBreak"));
                            viewModel.TargetTwo = reader.GetString(reader.GetOrdinal("TargetTwo"));
                            viewModel.MachineName = reader.GetString(reader.GetOrdinal("MachineName"));
                            viewModel.CustomerName = reader.GetString(reader.GetOrdinal("CustomerName"));
                            viewModel.PackageSize = reader.GetString(reader.GetOrdinal("PackageSize"));
                            viewModel.PiecesPerBlank = reader.GetInt32(reader.GetOrdinal("PiecesPerBlank"));
                         
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

        private async Task PrepareViewModelForEdit(BreakingViewModel model)
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
        public async Task<IActionResult> Edit(BreakingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareViewModelForEdit(model);
                return View(model);
            }
            string breakingUpdateCommandText = @"
            UPDATE Breaking
            SET 
                OutputQty = @OutputQty,
                RejectQty = @RejectQty,
                DifferencesQty = @DifferencesQty,
                ChippingQty = @ChippingQty,
                SheetBreakQty = @SheetBreakQty,
                CrackQty = @CrackQty,
                OthersQty = @OthersQty,
                MachineStartTime = @MachineStartTime,
                MachineEndTime = @MachineEndTime,
                FirstBreak = @FirstBreak,
                TargetOne = @TargetOne,
                SecondBreak = @SecondBreak,
                TargetTwo = @TargetTwo,
                Remarks = @Remarks,
                Status = @Status
            WHERE BreakingId = @BreakingId";

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
                    using (var breakingCommand = new SqlCommand(breakingUpdateCommandText, connection, transaction))
                    {
                        // Bind parameters for Breaking update
                        breakingCommand.Parameters.AddWithValue("@BreakingId", model.BreakingId);
                        breakingCommand.Parameters.AddWithValue("@OutputQty", model.OutputQty);
                        breakingCommand.Parameters.AddWithValue("@RejectQty", model.RejectQty);
                        breakingCommand.Parameters.AddWithValue("@DifferencesQty", model.DifferencesQty);
                        breakingCommand.Parameters.AddWithValue("@ChippingQty", model.ChippingQty);
                        breakingCommand.Parameters.AddWithValue("@SheetBreakQty", model.SheetBreakQty);
                        breakingCommand.Parameters.AddWithValue("@CrackQty", model.CrackQty);
                        breakingCommand.Parameters.AddWithValue("@OthersQty", model.OthersQty);
                        breakingCommand.Parameters.AddWithValue("@MachineStartTime", model.MachineStartTime);
                        breakingCommand.Parameters.AddWithValue("@MachineEndTime", model.MachineEndTime);
                        breakingCommand.Parameters.AddWithValue("@FirstBreak", model.FirstBreak);
                        breakingCommand.Parameters.AddWithValue("@TargetOne", model.TargetOne);
                        breakingCommand.Parameters.AddWithValue("@SecondBreak", model.SecondBreak);
                        breakingCommand.Parameters.AddWithValue("@TargetTwo", model.TargetTwo);
                        breakingCommand.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                        breakingCommand.Parameters.AddWithValue("@Status", model.Status.ToString());

                        var breakingResult = await breakingCommand.ExecuteNonQueryAsync();

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
                                    TempData["Success"] = "Breaking process and current status updated successfully.";
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
                            ModelState.AddModelError("", "No records were updated in the Breaking table.");
                            await PrepareViewModelForEdit(model);
                            return View(model);
                        }
                    }
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int BreakingId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Super Admin"))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            string commandText = "DELETE FROM Breaking WHERE BreakingId = @BreakingId";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@BreakingId", BreakingId);

                    int result = await command.ExecuteNonQueryAsync();
                    if (result > 0)
                    {
                        // Record was successfully deleted
                        _logger.LogInformation($"Deleted Breaking with ID {BreakingId}.");
                        // Redirect to the Index1 action
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
