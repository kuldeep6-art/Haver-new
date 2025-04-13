using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using haver.Data;
using haver.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using haver.ViewModels;
using haver.CustomControllers;
using haver.Utilities;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using System.Globalization;
using System;
using System.Text.Json;
using System.Drawing;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace haver.Controllers
{
    [Authorize]
    public class GanttDataController : ElephantController
    {
        private readonly HaverContext _context;

        public GanttDataController(HaverContext context)
        {
            _context = context;
        }

        // GET: GanttData


        [Authorize(Roles = "Admin,Engineering,Production,PIC,Sales")]
        public async Task<IActionResult> Index(int? page, int? pageSizeID,
            string? SearchString, string? actionButton, string sortDirection = "asc", string sortField = "Order Number", bool? isFinalized = null)
        {
            string[] sortOptions = new[] { "Order Number" };

            // Default filter to Active (not finalized) if isFinalized is null
            if (!isFinalized.HasValue)
            {
                isFinalized = false;
            }

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;


            var gData = from g in _context.GanttDatas
                        .Include(g => g.SalesOrder)
                .ThenInclude(m => m.Machines)
                .Include(m => m.Machine).ThenInclude(m => m.MachineType)
                .AsNoTracking()
                        select g;


            // Apply `isFinalized` filter only if it's not null
            if (isFinalized.HasValue)
            {
                gData = gData.Where(v => v.IsFinalized == isFinalized.Value);
            }

            // Set ViewBag.Status to control which tab is active
            ViewBag.Status = isFinalized.Value ? "Finalized" : "Active";


            if (!String.IsNullOrEmpty(SearchString))
            {
                gData = gData.Where(p => p.SalesOrder.OrderNumber.Contains(SearchString));
                numberFilters++;
            }




            //Give feedback about the state of the filters
            if (numberFilters != 0)
            {
                //Toggle the Open/Closed state of the collapse depending on if we are filtering
                ViewData["Filtering"] = " btn-danger";
                //Show how many filters have been applied
                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
                //Keep the Bootstrap collapse open
                @ViewData["ShowFilter"] = " show";
            }

            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            //Now we know which field and direction to sort by
            if (sortField == "Order Number")
            {
                if (sortDirection == "asc")
                {
                    gData = gData
                        .OrderByDescending(p => p.SalesOrder.OrderNumber);
                }
                else
                {
                    gData = gData
                        .OrderBy(p => p.SalesOrder.OrderNumber);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<GanttData>.CreateAsync(gData.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: GanttData/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganttData = _context.GanttDatas
                .Include(g => g.Machine)
                    .ThenInclude(m => m.SalesOrder) // Ensure SalesOrder is loaded via Machine
                .Include(m => m.Machine).ThenInclude(m => m.MachineType)
                .FirstOrDefault(g => g.ID == id);

            if (ganttData == null)
            {
                return NotFound();
            }

            var ganttTasks = GetMilestoneTasks(ganttData);

            var viewModel = new GanttDetailsViewModel
            {
                GanttData = ganttData,
                GanttTasks = ganttTasks
            };

            return View(viewModel);
        }



        // GET: GanttData/GetMachineData/5
        public IActionResult GetMachineData(int salesOrderID)
        {
            //Console.WriteLine($"Machine ID: {machineID}");

            // Find the Sales Order to check if the SalesOrderID is valid
            var salesOrder = _context.SalesOrders.FirstOrDefault(so => so.ID == salesOrderID);
            if (salesOrder == null)
            {
                return Json(new { success = false, message = "Sales Order not found" });
            }


            var machines = _context.Machines

                                      .Where(m => m.SalesOrderID == salesOrderID)
                                      .ToList();

            if (!machines.Any())
            {
                return Json(new { success = false, message = "No machines found for the selected Sales Order." });
            }

            var data = machines.Select(m => new
            {
                AppDRcd = m.SalesOrder?.AppDwgRel?.ToString("yyyy-MM-dd"),
                EngExpected = m.SalesOrder?.EngPExp?.ToString("yyyy-MM-dd"),
                EngReleased = m.SalesOrder?.EngPRel?.ToString("yyyy-MM-dd"),
                //CustomerApproval = machine.SalesOrder?.AppDwgRel?.ToString("yyyy-MM-dd"),
                //PackageReleased = machine.SalesOrder?.PreORel?.ToString("yyyy-MM-dd"),
                PurchaseOrdersIssued = m.SalesOrder?.SoDate?.ToString("yyyy-MM-dd"),
                //PurchaseOrdersDue = machine.SalesOrder?.
                //SupplierPODue = machine.SalesOrder?.SupplierPODue?.ToString("yyyy-MM-dd"),  // ✅ Now included
                //AssemblyStart = machine?.AssemblyStart?.ToString("yyyy-MM-dd"),  // ✅ Now included
                //AssemblyComplete = machine?.AssemblyComplete?.ToString("yyyy-MM-dd"),
                ShipExpected = m.RToShipExp?.ToString("yyyy-MM-dd"),
                ShipActual = m.RToShipA?.ToString("yyyy-MM-dd"),
                // DeliveryExpected = machine.SalesOrder?.AppDwgRet?.ToString("yyyy-MM-dd"),
                //DeliveryActual = machine.SalesOrder?.PreOExp.ToString("yyyy-MM-dd")
            });

            return Json(new { success = true, machines = data });
        }



        // GET: GanttData/Create
        // GET: GanttData/Create

        [Authorize(Roles = "Admin,Engineering,Production,PIC")]
        public IActionResult Create()
        {
            // Get all Sales Orders for selection
            var salesOrders = _context.SalesOrders.ToList();
            ViewData["SalesOrderID"] = new SelectList(salesOrders, "ID", "OrderNumber");

            return View();
        }



        // POST: GanttData/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,Engineering,Production,PIC")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SalesOrderID,AppDRcd,EngExpected,EngReleased,PackageReleased,PurchaseOrdersIssued,PurchaseOrdersCompleted,SupplierPODue,AssemblyStart,AssemblyComplete,ShipExpected,ShipActual,DeliveryExpected,DeliveryActual,Notes")] GanttData ganttData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var machines = _context.Machines.Where(m => m.SalesOrderID == ganttData.SalesOrderID).ToList();

                    if (machines.Any())
                    {
                        foreach (var machine in machines)
                        {
                            var newGanttData = new GanttData
                            {
                                SalesOrderID = ganttData.SalesOrderID,
                                MachineID = machine.ID,
                                AppDRcd = ganttData.AppDRcd,
                                EngExpected = ganttData.EngExpected,
                                EngReleased = ganttData.EngReleased,
                                PackageReleased = ganttData.PackageReleased,
                                PurchaseOrdersIssued = ganttData.PurchaseOrdersIssued,
                                PurchaseOrdersCompleted = ganttData.PurchaseOrdersCompleted,
                                SupplierPODue = ganttData.SupplierPODue,
                                AssemblyStart = ganttData.AssemblyStart,
                                AssemblyComplete = ganttData.AssemblyComplete,
                                ShipExpected = ganttData.ShipExpected,
                                ShipActual = ganttData.ShipActual,
                                DeliveryExpected = ganttData.DeliveryExpected,
                                DeliveryActual = ganttData.DeliveryActual,
                                Notes = ganttData.Notes,
                                IsFinalized = false,
                                StartOfWeek = (Models.WeekStartOption)WeekStartOption.Monday
							};

                            _context.GanttDatas.Add(newGanttData);
                        }

                        await _context.SaveChangesAsync();

                        _context.ActivityLogs.Add(new ActivityLog
                        {
                            Message = $"Gantt Data created for Sales Order {ganttData.SalesOrderID}.",
                            Timestamp = DateTime.UtcNow
                        });

                        await _context.SaveChangesAsync();
                        TempData["Message"] = "Gantt Data has been successfully created for all machines in the Sales Order.";
                    }
                    else
                    {
                        TempData["Error"] = "No machines found for the selected Sales Order.";
                    }

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber");
            return View(ganttData);
        }




        // GET: GanttData/Edit/5
        [Authorize(Roles = "Admin,Engineering,Production,PIC")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganttData = await _context.GanttDatas
                .Include(g => g.SalesOrder).ThenInclude(m => m.Machines)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (ganttData == null)
            {
                return NotFound();
            }
            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber");
            return View(ganttData);
        }

        // POST: GanttData/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,Engineering,Production,PIC")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var gDataToUpdate = await _context.GanttDatas
                .Include(g => g.SalesOrder)
                .Include(g => g.Machine)
                .FirstOrDefaultAsync(e => e.ID == id);

            if (gDataToUpdate == null)
            {
                return NotFound();
            }

            string previousState = gDataToUpdate.Notes; // Store previous notes for logging

            if (await TryUpdateModelAsync<GanttData>(gDataToUpdate, "",
                p => p.SalesOrderID, p => p.AppDRcd, p => p.AppDExp, p => p.EngExpected, p => p.EngReleased,
                p => p.PackageReleased, p => p.PurchaseOrdersIssued, p => p.PurchaseOrdersCompleted, p => p.PurchaseOrdersReceived,
                p => p.SupplierPODue, p => p.AssemblyStart, p => p.AssemblyComplete, p => p.ShipExpected, p => p.ShipActual, p => p.DeliveryExpected,
                p => p.DeliveryActual, p => p.Notes))
            {
                try
                {
                    bool isUpdated = false;

                    if (gDataToUpdate.SalesOrder != null)
                    {
                        if (gDataToUpdate.AppDExp.HasValue && gDataToUpdate.SalesOrder.AppDwgExp != gDataToUpdate.AppDExp.Value)
                        {
                            gDataToUpdate.SalesOrder.AppDwgExp = gDataToUpdate.AppDExp.Value;
                            isUpdated = true;
                        }

                        if (gDataToUpdate.AppDRcd != null && gDataToUpdate.SalesOrder.AppDwgRel != gDataToUpdate.AppDRcd)
                        {
                            gDataToUpdate.SalesOrder.AppDwgRel = gDataToUpdate.AppDRcd;
                            isUpdated = true;
                        }

                        if (gDataToUpdate.EngExpected != null && gDataToUpdate.SalesOrder.EngPExp != gDataToUpdate.EngExpected)
                        {
                            gDataToUpdate.SalesOrder.EngPExp = gDataToUpdate.EngExpected;
                            isUpdated = true;
                        }

                        if (gDataToUpdate.EngReleased != null && gDataToUpdate.SalesOrder.EngPRel != gDataToUpdate.EngReleased)
                        {
                            gDataToUpdate.SalesOrder.EngPRel = gDataToUpdate.EngReleased;
                            isUpdated = true;
                        }
                    }

                    if (gDataToUpdate.Machine != null)
                    {
                        if (gDataToUpdate.AssemblyStart.HasValue && gDataToUpdate.Machine.AssemblyStart != gDataToUpdate.AssemblyStart.Value)
                        {
                            gDataToUpdate.Machine.AssemblyStart= gDataToUpdate.AssemblyStart.Value;
                            isUpdated = true;
                        }

                        if (gDataToUpdate.AssemblyComplete.HasValue && gDataToUpdate.Machine.AssemblyComplete != gDataToUpdate.AssemblyComplete.Value)
                        {
                            gDataToUpdate.Machine.AssemblyComplete = gDataToUpdate.AssemblyComplete.Value;
                            isUpdated = true;
                        }

                        if (gDataToUpdate.PurchaseOrdersCompleted != null && gDataToUpdate.MachineID != null)
                        {
                            // Find the latest procurement record for the machine
                            var latestProcurement = await _context.Procurements
                                .Where(po => po.MachineID == gDataToUpdate.MachineID)
                                .OrderByDescending(po => po.PORcd) // Get the most recent record
                                .FirstOrDefaultAsync();

                            if (latestProcurement != null && latestProcurement.PORcd != gDataToUpdate.PurchaseOrdersCompleted)
                            {
                                // Update the procurement's PORcd (received date) to match the new Gantt date
                                latestProcurement.PORcd = gDataToUpdate.PurchaseOrdersCompleted;

                                // Save the updated procurement record
                                _context.Procurements.Update(latestProcurement);
                                await _context.SaveChangesAsync();

                                Console.WriteLine($"Procurement record {latestProcurement.PONumber} updated with new PORcd: {latestProcurement.PORcd}");
                            }
                            else
                            {
                                Console.WriteLine("No procurement records found or no changes to update.");
                            }
                        }


                    }



                    if (isUpdated)
                    {
                        await _context.SaveChangesAsync();
                    }

                    await _context.SaveChangesAsync();

                    _context.ActivityLogs.Add(new ActivityLog
                    {
                        Message = $"Gantt Data for Sales Order {gDataToUpdate.SalesOrderID} was updated.",
                        Timestamp = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Gantt Data successfully updated. Milestone dates have been updated on the related sales order and machine";
                    return RedirectToAction("Details", new { gDataToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GanttDataExists(gDataToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber");
            return View(gDataToUpdate);
        }


        // GET: GanttData/Delete/5
        [Authorize(Roles = "Admin,Engineering,Production,PIC")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganttData = await _context.GanttDatas
                .Include(g => g.SalesOrder).ThenInclude(m => m.Machines)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (ganttData == null)
            {
                return NotFound();
            }

            return View(ganttData);
        }

        // POST: GanttData/Delete/5
        [Authorize(Roles = "Admin,Engineering,Production,PIC")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ganttData = await _context.GanttDatas
                .Include(g => g.Machine)
                .Include(g => g.Machine).ThenInclude(s => s.SalesOrder)
                .Include(g => g.Machine).ThenInclude(s => s.MachineType)
                .FirstOrDefaultAsync(m => m.ID == id);

            try
            {
                if (ganttData != null)
                {
                    _context.GanttDatas.Remove(ganttData);
                    await _context.SaveChangesAsync();

                    _context.ActivityLogs.Add(new ActivityLog
                    {
                        Message = $"Gantt Data for Sales Order {ganttData.SalesOrder.OrderNumber} was deleted.",
                        Timestamp = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(ganttData);
        }



        //public IActionResult Chart()
        //{
        //    var ganttData = _context.GanttDatas
        //        .Include(g => g.Machine) // Ensure Machine is loaded
        //        .ToList() // Convert to memory before calling a custom method
        //        .Select(g => new GanttViewModel
        //        {
        //            ID = g.ID,
        //            MachineName = g.Machine?.Description ?? "Unknown",
        //            StartDate = g.AppDRcd,
        //            EndDate = g.DeliveryExpected,
        //            Progress = 0,
        //            MilestoneClass = GetMilestoneClass(g) // Now safe to use
        //        })
        //        .ToList();

        //    return View(ganttData);
        //}

        //      [HttpGet]
        //public IActionResult ExportSchedules()
        //{
        //	var options = LoadOptionsFromSession() ?? new ScheduleExportOptionsViewModel();


        //	return View("ExportSchedules", options);
        //}

        //[HttpGet]
        //public IActionResult ExportSchedules()
        //{
        //    // Load current options from session or create a new instance
        //    var options = LoadOptionsFromSession() ?? new ScheduleExportOptionsViewModel();

        //    // Load last 5 selections from the database
        //    var savedSelections = LoadSelectionsFromDatabase();

        //    // Pass the selections to the view
        //    ViewBag.SavedSelections = savedSelections;

        //    // Return the view with options
        //    return View("ExportSchedules", options);
        //}

        public IActionResult Chart()
        {
            var ganttData = _context.GanttDatas
                .Include(g => g.SalesOrder)
                .Include(g => g.Machine).ThenInclude(m => m.MachineType)
                .ToList() // Fetch all data first
                .SelectMany(g => GetMilestoneTasks(g)) // Break into multiple segments per machine

                .ToList();

            return View(ganttData);
        }

			[HttpGet]
			public IActionResult ExportSchedules()
			{
				var options = LoadOptionsFromSession() ?? new ScheduleExportOptionsViewModel();
				return View("ExportSchedules", options);
			}


    



<<<<<<< HEAD
			private void SaveSelectionToDatabase(ScheduleExportOptionsViewModel options)
			{
				// Convert the current selection to JSON
				var selectionJson = JsonSerializer.Serialize(options);

				var newSelection = new UserSelection
				{
					SelectionJson = selectionJson,
					CreatedAt = DateTime.UtcNow // Set current timestamp
				};

				// Save the selection to the database
				_context.UserSelections.Add(newSelection);
				_context.SaveChanges();

				// Keep only the last 5 selections by deleting older entries
				var selectionsToDelete = _context.UserSelections
					.OrderByDescending(s => s.CreatedAt)
					.ToList();

				if (selectionsToDelete.Any())
				{
					_context.UserSelections.RemoveRange(selectionsToDelete);
					_context.SaveChanges();
				}
			}

			private List<ScheduleExportOptionsViewModel> LoadSelectionsFromDatabase()
			{
				var selections = _context.UserSelections
					.ToList();

				// Deserialize and return as a list of ScheduleExportOptionsViewModel
				return selections
					.Select(s => JsonSerializer.Deserialize<ScheduleExportOptionsViewModel>(s.SelectionJson))
					.Where(s => s != null) // Ensure no null entries
					.ToList();
			}


			[HttpPost]
			public IActionResult SaveTemplate(string name, [FromBody] ScheduleExportOptionsViewModel options)
			{
				if (string.IsNullOrWhiteSpace(name) || options == null)
					return BadRequest("Invalid data");

				var existing = _context.UserSelections.FirstOrDefault(t => t.TemplateName == name);
				var json = JsonSerializer.Serialize(options);

				if (existing != null)
				{
					existing.SelectionJson = json;
					existing.CreatedAt = DateTime.UtcNow;
					_context.UserSelections.Update(existing);
				}
				else
				{
					var template = new UserSelection
					{
						TemplateName = name,
						SelectionJson = json,
						CreatedAt = DateTime.UtcNow
					};
					_context.UserSelections.Add(template);
				}

				_context.SaveChanges();
				return Ok();
			}


			[HttpPost]
			public IActionResult DeleteTemplate([FromBody] string name)
=======
        [HttpPost]
		public IActionResult DownloadSchedules(ScheduleExportOptionsViewModel options)
		{
			// Load previously saved options from the database if no form submission
			if (!HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) || options == null)
			{
				if (options != null)
				{
					SaveSelectionToDatabase(options);
				}

				var savedSelections = LoadSelectionsFromDatabase();
				if (savedSelections.Any())
				{
					options = savedSelections.OrderByDescending(s => s.SelectionDate).First(); // Load the most recent selection
				}
				else
				{
					options = new ScheduleExportOptionsViewModel(); // Fallback if nothing is found
				}
			}

			// Fetch sales orders with related data (unchanged)
			var salesOrders = _context.SalesOrders
				.Include(so => so.Machines).ThenInclude(m => m.MachineType)
				.Include(so => so.Machines).ThenInclude(m => m.Procurements).ThenInclude(p => p.Vendor)
				.Include(so => so.SalesOrderEngineers).ThenInclude(se => se.Engineer)
				.OrderByDescending(so => so.SoDate)
				.AsNoTracking()
				.AsEnumerable();

			// Machine Schedules Data
			var machineSchedules = salesOrders
				.SelectMany(so => so.Machines != null && so.Machines.Any()
					? so.Machines.Select(m => new MachineScheduleViewModel
					{
						OrderNumber = options.IncludeOrderNumber ? (so?.OrderNumber ?? "") : null,
						SalesOrderDate = options.IncludeSalesOrderDate ? (so?.SoDate?.ToShortDateString() ?? "N/A") : null,
						CustomerName = options.IncludeCustomerName ? (so?.CompanyName ?? "Unknown") : null,
						MachineModel = options.IncludeMachineModel ? (m?.MachineModel ?? "Unknown") : null,
						SerialNumbers = options.IncludeSerialNumbers ? (m?.SerialNumber ?? "N/A") : null,
						ProductionOrderNumbers = options.IncludeProductionOrderNumbers ? (m?.ProductionOrderNumber ?? "N/A") : null,
						PackageReleaseDateE = options.IncludePackageReleaseDateE ? "P - " + (so?.EngPExp?.ToShortDateString() ?? "N/A") : null,
						PackageReleaseDateA = options.IncludePackageReleaseDateA ? "A - " + (so?.EngPRel?.ToShortDateString() ?? "N/A") : null,
						VendorNames = options.IncludeVendorNames && m?.Procurements != null && m.Procurements.Any()
							? string.Join(", ", m.Procurements.Select(p => p?.Vendor?.Name ?? "N/A"))
							: null,
						PoNumbers = options.IncludePoNumbers && m?.Procurements != null && m.Procurements.Any()
							? string.Join(", ", m.Procurements.Select(p => p?.PONumber ?? "N/A"))
							: null,
						PoDueDates = options.IncludePoDueDates && m?.Procurements != null && m.Procurements.Any()
							? string.Join(", ", m.Procurements.Select(p => p.PODueDate.HasValue ? p.PODueDate.Value.ToString("yyyy-MM-dd") : "N/A"))
							: null,
						Media = options.IncludeMedia ? (m?.Media ?? false ? "✓" : "") : null,
						SpareParts = options.IncludeSpareParts ? (m?.SpareParts ?? false ? "✓" : "") : null,
						Base = options.IncludeBase ? (m?.Base ?? false ? "✓" : "") : null,
						AirSeal = options.IncludeAirSeal ? (m?.AirSeal ?? false ? "✓" : "") : null,
						CoatingLining = options.IncludeCoatingLining ? (m?.CoatingLining ?? false ? "✓" : "") : null,
						Disassembly = options.IncludeDisassembly ? (m?.Disassembly ?? false ? "✓" : "") : null,
						Comments = options.IncludeNotes ? (!string.IsNullOrEmpty(so?.Comments) ? Regex.Replace(so.Comments, "<.*?>", string.Empty) : "N/A") : null,
						PreOrder = options.IncludePreOrder ? (!string.IsNullOrEmpty(m?.PreOrder) ? Regex.Replace(m.PreOrder, "<.*?>", string.Empty) : "N/A") : null,
						Scope = options.IncludeScope ? (!string.IsNullOrEmpty(m?.Scope) ? Regex.Replace(m.Scope, "<.*?>", string.Empty) : "N/A") : null,
						ActualAssemblyHours = options.IncludeActualAssemblyHours ? (m?.ActualAssemblyHours != null ? $"{m.ActualAssemblyHours} hrs" : "N/A") : null,
						ReworkHours = options.IncludeReworkHours ? (m?.ReworkHours != null ? $"{m.ReworkHours} hrs" : "N/A") : null,
						NamePlate = options.IncludeNamePlate ? (m?.Nameplate?.ToString() ?? "N/A") : null
					})
					: new[] { new MachineScheduleViewModel
			{
				OrderNumber = options.IncludeOrderNumber ? (so?.OrderNumber ?? "") : null,
				SalesOrderDate = options.IncludeSalesOrderDate ? (so?.SoDate.HasValue == true ? so.SoDate.Value.ToShortDateString() : "N/A") : null,
				CustomerName = options.IncludeCustomerName ? (so?.CompanyName ?? "Unknown") : null,
				Media = options.IncludeMedia ? (so.Machines.Any(m => m.Media) ? "✓" : "") : null,
				SpareParts = options.IncludeSpareParts ? (so.Machines.Any(m => m.SpareParts) ? "✓" : "") : null,
				Base = options.IncludeBase ? (so.Machines.Any(m => m.Base) ? "✓" : "") : null,
				AirSeal = options.IncludeAirSeal ? (so.Machines.Any(m => m.AirSeal) ? "✓" : "") : null,
				CoatingLining = options.IncludeCoatingLining ? (so.Machines.Any(m => m.CoatingLining) ? "✓" : "") : null,
				Disassembly = options.IncludeDisassembly ? (so.Machines.Any(m => m.Disassembly) ? "✓" : "") : null,
				PackageReleaseDateE = options.IncludePackageReleaseDateE ? "P - " + (so?.EngPExp?.ToShortDateString() ?? "N/A") : null,
				PackageReleaseDateA = options.IncludePackageReleaseDateA ? "A - " + (so?.EngPRel?.ToShortDateString() ?? "N/A") : null,
				Comments = options.IncludeNotes ? (!string.IsNullOrEmpty(so?.Comments) ? Regex.Replace(so.Comments, "<.*?>", string.Empty) : "N/A") : null
			} })
				.ToList();

			// Gantt Schedules Data
			var ganttData = _context.GanttDatas
				.Include(g => g.SalesOrder)
				.Include(g => g.Machine).ThenInclude(m => m.MachineType)
				.AsNoTracking()
				.ToList();

			var milestoneTasks = ganttData.SelectMany(g => GetMilestoneTasks(g)).ToList();
			var orderNumberToIdMap = _context.SalesOrders.AsNoTracking().GroupBy(so => so.OrderNumber ?? "").ToDictionary(g => g.Key, g => g.First().ID);
			var groupedTasks = milestoneTasks.GroupBy(g => g.SalesOrder).ToList();
			var ganttDataLookup = new Dictionary<int, List<GanttViewModel>>();
			foreach (var group in groupedTasks)
			{
				if (orderNumberToIdMap.TryGetValue(group.Key, out var salesOrderId))
				{
					ganttDataLookup[salesOrderId] = group.ToList();
				}
			}

			var ganttSchedules = salesOrders
				.SelectMany(so => so.Machines.Select(m => new GanttScheduleViewModel
				{
					OrderNumber = options.IncludeOrderNumber ? (so?.OrderNumber ?? "") : null,
					Engineer = options.IncludeEngineer && so?.SalesOrderEngineers?.FirstOrDefault()?.Engineer != null
						? $"{so.SalesOrderEngineers.FirstOrDefault().Engineer.FirstName?[0]} {so.SalesOrderEngineers.FirstOrDefault().Engineer.LastName?[0]}"
						: null,
					CustomerName = options.IncludeCustomerName ? (so?.CompanyName ?? "Unknown") : null,
					Quantity = options.IncludeQuantity ? 1 : 0, // Each row is for one machine
					MachineModel = options.IncludeMachineModel ? (m?.MachineModel ?? "Unknown") : null,
					Media = options.IncludeMedia ? (m.Media ? "Yes" : "No") : null,
					SpareParts = options.IncludeSpareParts ? (m.SpareParts ? "Yes" : "No") : null,
					ApprovedDrawingReceived = options.IncludeApprovedDrawingReceived ? (so?.AppDwgExp ?? DateTime.MinValue) : DateTime.MinValue,
					GanttData = options.IncludeGanttData && ganttDataLookup.ContainsKey(so?.ID ?? 0) ? ganttDataLookup[so.ID].Where(g => g.ID == m.ID).ToList() : null,
					SpecialNotes = options.IncludeSpecialNotes && ganttDataLookup.ContainsKey(so?.ID ?? 0) && ganttDataLookup[so.ID].Any(g => g.ID == m.ID)
						? string.Join("; ", ganttDataLookup[so.ID].Where(g => g.ID == m.ID && !string.IsNullOrEmpty(g?.Notes)).Select(g => Regex.Replace(g.Notes, "<.*?>", string.Empty)).Distinct())
						: null
				}))
				.ToList();

			if ((options.ReportType == ReportType.MachineSchedules && !machineSchedules.Any()) ||
				(options.ReportType == ReportType.GanttSchedules && !ganttSchedules.Any()) ||
				(options.ReportType == ReportType.Both && !machineSchedules.Any() && !ganttSchedules.Any()))
			{
				return NotFound("No data available to export.");
			}

			// Save options to database if this is a form submission
			if (HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
>>>>>>> b998c9d23189efe44dc7bb325dd11c8609ac17c9
			{
				if (string.IsNullOrWhiteSpace(name))
					return BadRequest("No name provided");

				var template = _context.UserSelections.FirstOrDefault(t => t.TemplateName == name);
				if (template != null)
				{
					_context.UserSelections.Remove(template);
					_context.SaveChanges();
				}

				return Ok();
			}


			[HttpGet]
			public IActionResult GetAllTemplates()
			{
				var templates = _context.UserSelections
					.OrderByDescending(t => t.CreatedAt)
					.Select(t => t.TemplateName)
					.ToList();

				return Json(templates);
			}


			[HttpGet]
			public IActionResult LoadTemplate(string name)
			{
				var template = _context.UserSelections.FirstOrDefault(t => t.TemplateName == name);
				if (template == null) return NotFound();

				var options = JsonSerializer.Deserialize<ScheduleExportOptionsViewModel>(template.SelectionJson);
				return Json(options);
			}



			[HttpPost]
			public IActionResult DownloadSchedules(ScheduleExportOptionsViewModel options)
			{
				// Load previously saved options from the database if no form submission
				if (!HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) || options == null)
				{
					if(options != null)
					{
						SaveSelectionToDatabase(options);
					}

					var savedSelections = LoadSelectionsFromDatabase();
					if (savedSelections.Any())
					{
						options = savedSelections.First(); // Load the most recent selection
					}
					else
					{
						options = new ScheduleExportOptionsViewModel(); // Fallback if nothing is found
					}
				}

				// Fetch sales orders with related data (unchanged)
				var salesOrders = _context.SalesOrders
					.Include(so => so.Machines).ThenInclude(m => m.MachineType)
					.Include(so => so.Machines).ThenInclude(m => m.Procurements).ThenInclude(p => p.Vendor)
					.Include(so => so.SalesOrderEngineers).ThenInclude(se => se.Engineer)
					.OrderByDescending(so => so.SoDate)
					.AsNoTracking()
					.AsEnumerable();

				// Machine Schedules Data 
				var machineSchedules = salesOrders
					.SelectMany(so => so.Machines != null && so.Machines.Any()
						? so.Machines.Select(m => new MachineScheduleViewModel
						{
							SalesOrderNumber = options.IncludeSalesOrderNumber ? (so?.OrderNumber ?? "") : null,
							SalesOrderDate = options.IncludeSalesOrderDate ? (so?.SoDate?.ToShortDateString() ?? "N/A") : null,
							CustomerName = options.IncludeCustomerName ? (so?.CompanyName ?? "Unknown") : null,
							MachineDescriptions = options.IncludeMachineDescriptions ? (m?.MachineModel ?? "Unknown") : null,
							SerialNumbers = options.IncludeSerialNumbers ? (m?.SerialNumber ?? "N/A") : null,
							ProductionOrderNumbers = options.IncludeProductionOrderNumbers ? (m?.ProductionOrderNumber ?? "N/A") : null,
							PackageReleaseDateE = options.IncludePackageReleaseDateE ? "P - " + (so?.EngPExp?.ToShortDateString() ?? "N/A") : null,
							PackageReleaseDateA = options.IncludePackageReleaseDateA ? "A - " + (so?.EngPRel?.ToShortDateString() ?? "N/A") : null,
							VendorNames = options.IncludeVendorNames && m?.Procurements != null && m.Procurements.Any()
								? string.Join(", ", m.Procurements.Select(p => p?.Vendor?.Name ?? "N/A"))
								: null,
							PoNumbers = options.IncludePoNumbers && m?.Procurements != null && m.Procurements.Any()
								? string.Join(", ", m.Procurements.Select(p => p?.PONumber ?? "N/A"))
								: null,
							PoDueDates = options.IncludePoDueDates && m?.Procurements != null && m.Procurements.Any()
								? string.Join(", ", m.Procurements.Select(p => p.PODueDate.HasValue ? p.PODueDate.Value.ToString("yyyy-MM-dd") : "N/A"))
								: null,
							Media = options.IncludeMedia ? (m?.Media ?? false ? "✓" : "") : null,
							SpareParts = options.IncludeSpareParts ? (m?.SpareParts ?? false ? "✓" : "") : null,
							Base = options.IncludeBase ? (m?.Base ?? false ? "✓" : "") : null,
							AirSeal = options.IncludeAirSeal ? (m?.AirSeal ?? false ? "✓" : "") : null,
							CoatingLining = options.IncludeCoatingLining ? (m?.CoatingLining ?? false ? "✓" : "") : null,
							Disassembly = options.IncludeDisassembly ? (m?.Disassembly ?? false ? "✓" : "") : null,
							Comments = options.IncludeNotes ? (!string.IsNullOrEmpty(so?.Comments) ? Regex.Replace(so.Comments, "<.*?>", string.Empty) : "N/A") : null,
							PreOrder = options.IncludePreOrder ? (!string.IsNullOrEmpty(m?.PreOrder) ? Regex.Replace(m.PreOrder, "<.*?>", string.Empty) : "N/A") : null,
							Scope = options.IncludeScope ? (!string.IsNullOrEmpty(m?.Scope) ? Regex.Replace(m.Scope, "<.*?>", string.Empty) : "N/A") : null,
							ActualAssemblyHours = options.IncludeActualAssemblyHours ? (m?.ActualAssemblyHours != null ? $"{m.ActualAssemblyHours} hrs" : "N/A") : null,
							ReworkHours = options.IncludeReworkHours ? (m?.ReworkHours != null ? $"{m.ReworkHours} hrs" : "N/A") : null,
							NamePlate = options.IncludeNamePlate ? (m?.Nameplate?.ToString() ?? "N/A") : null
						})
						: new[] { new MachineScheduleViewModel
				{
					SalesOrderNumber = options.IncludeSalesOrderNumber ? (so?.OrderNumber ?? "") : null,
					SalesOrderDate = options.IncludeSalesOrderDate ? (so?.SoDate.HasValue == true ? so.SoDate.Value.ToShortDateString() : "N/A") : null,
					CustomerName = options.IncludeCustomerName ? (so?.CompanyName ?? "Unknown") : null,
					Media = options.IncludeMedia ? (so.Machines.Any(m => m.Media) ? "✓" : "") : null,
					SpareParts = options.IncludeSpareParts ? (so.Machines.Any(m => m.SpareParts) ? "✓" : "") : null,
					Base = options.IncludeBase ? (so.Machines.Any(m => m.Base) ? "✓" : "") : null,
					AirSeal = options.IncludeAirSeal ? (so.Machines.Any(m => m.AirSeal) ? "✓" : "") : null,
					CoatingLining = options.IncludeCoatingLining ? (so.Machines.Any(m => m.CoatingLining) ? "✓" : "") : null,
					Disassembly = options.IncludeDisassembly ? (so.Machines.Any(m => m.Disassembly) ? "✓" : "") : null,
					PackageReleaseDateE = options.IncludePackageReleaseDateE ? "P - " + (so?.EngPExp?.ToShortDateString() ?? "N/A") : null,
					PackageReleaseDateA = options.IncludePackageReleaseDateA ? "A - " + (so?.EngPRel?.ToShortDateString() ?? "N/A") : null,
					Comments = options.IncludeNotes ? (!string.IsNullOrEmpty(so?.Comments) ? Regex.Replace(so.Comments, "<.*?>", string.Empty) : "N/A") : null
				} })
					.ToList();

				// Gantt Schedules Data (unchanged)
				var ganttData = _context.GanttDatas
					.Include(g => g.SalesOrder)
					.Include(g => g.Machine).ThenInclude(m => m.MachineType)
					.AsNoTracking()
					.ToList();

				var milestoneTasks = ganttData.SelectMany(g => GetMilestoneTasks(g)).ToList();
				var orderNumberToIdMap = _context.SalesOrders.AsNoTracking().GroupBy(so => so.OrderNumber ?? "").ToDictionary(g => g.Key, g => g.First().ID);
				var groupedTasks = milestoneTasks.GroupBy(g => g.SalesOrder).ToList();
				var ganttDataLookup = new Dictionary<int, List<GanttViewModel>>();
				foreach (var group in groupedTasks)
				{
					if (orderNumberToIdMap.TryGetValue(group.Key, out var salesOrderId))
					{
						ganttDataLookup[salesOrderId] = group.ToList();
					}
				}
				var ganttSchedules = salesOrders
	  .SelectMany(so => so.Machines.Select(m => new GanttScheduleViewModel
	  {
		  OrderNumber = options.IncludeOrderNumber ? (so?.OrderNumber ?? "") : null,
		  Engineer = options.IncludeEngineer && so?.SalesOrderEngineers?.FirstOrDefault()?.Engineer != null
			  ? $"{so.SalesOrderEngineers.FirstOrDefault().Engineer.FirstName?[0]} {so.SalesOrderEngineers.FirstOrDefault().Engineer.LastName?[0]}"
			  : null,
		  CustomerName = options.IncludeGanttCustomerName ? (so?.CompanyName ?? "Unknown") : null,
		  Quantity = options.IncludeQuantity ? 1 : 0,  // Each row is for one machine
		  MachineModel = options.IncludeMachineModel ? (m?.MachineModel ?? "Unknown") : null,  //  Each machine gets its own row
		  Media = options.IncludeGanttMedia ? (m.Media ? "Yes" : "No") : null,
		  SpareParts = options.IncludeGanttSpareParts ? (m.SpareParts ? "Yes" : "No") : null,
		  ApprovedDrawingReceived = options.IncludeApprovedDrawingReceived ? (so?.AppDwgExp ?? DateTime.MinValue) : DateTime.MinValue,
		  GanttData = options.IncludeGanttData && ganttDataLookup.ContainsKey(so?.ID ?? 0) ? ganttDataLookup[so.ID].Where(g => g.ID == m.ID).ToList() : null,
		  SpecialNotes = options.IncludeSpecialNotes && ganttDataLookup.ContainsKey(so?.ID ?? 0) && ganttDataLookup[so.ID].Any(g => g.ID == m.ID)
			  ? string.Join("; ", ganttDataLookup[so.ID].Where(g => g.ID == m.ID && !string.IsNullOrEmpty(g?.Notes)).Select(g => Regex.Replace(g.Notes, "<.*?>", string.Empty)).Distinct())
			  : null
	  }))
	  .ToList();

				if ((options.ReportType == ReportType.MachineSchedules && !machineSchedules.Any()) ||
					(options.ReportType == ReportType.GanttSchedules && !ganttSchedules.Any()) ||
					(options.ReportType == ReportType.Both && !machineSchedules.Any() && !ganttSchedules.Any()))
				{
					return NotFound("No data available to export.");
				}

				// Save options to session if this is a form submission
				if (HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
				{
					SaveSelectionToDatabase(options);
				}

				using (ExcelPackage excel = new ExcelPackage())
				{
					if (options.ReportType == ReportType.MachineSchedules)
					{
						var workSheet = excel.Workbook.Worksheets.Add("Combined Schedule");
						SetupCombinedFlowSchedulesWorksheet(workSheet, machineSchedules, ganttSchedules, options);
					}
					else if (options.ReportType == ReportType.GanttSchedules)
					{
						var workSheet = excel.Workbook.Worksheets.Add("Combined Schedule");
						SetupCombinedFlowSchedulesWorksheet(workSheet, machineSchedules, ganttSchedules, options);
					}
					else if (options.ReportType == ReportType.Both)
					{
						var workSheet = excel.Workbook.Worksheets.Add("Combined Schedule");
						SetupCombinedFlowSchedulesWorksheet(workSheet, machineSchedules, ganttSchedules, options);
					}

					try
					{
						byte[] theData = excel.GetAsByteArray();
						string fileName = options.ReportType == ReportType.MachineSchedules ? "Combined Schedule.xlsx" :
										  options.ReportType == ReportType.GanttSchedules ? "Combined Schedule.xlsx" :
										  "Combined Schedule.xlsx";
						return File(theData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
					}
					catch (Exception ex)
					{
						return BadRequest($"Could not build and download the file: {ex.Message}");
					}
				}
			}
<<<<<<< HEAD

			// Helper methods for session management
			private void SaveOptionsToSession(ScheduleExportOptionsViewModel options)
			{
				var optionsJson = JsonSerializer.Serialize(options);
				HttpContext.Session.SetString("ScheduleExportOptions", optionsJson);
			}

			private ScheduleExportOptionsViewModel LoadOptionsFromSession()
			{
				var optionsJson = HttpContext.Session.GetString("ScheduleExportOptions");
				return string.IsNullOrEmpty(optionsJson) ? null : JsonSerializer.Deserialize<ScheduleExportOptionsViewModel>(optionsJson);
			}

			// Existing SetupMachineSchedulesWorksheet and SetupGanttSchedulesWorksheet methods remain unchanged
			// I'll include them here for completeness, but they are the same as in the previous response

			private void SetupCombinedFlowSchedulesWorksheet(ExcelWorksheet workSheet, List<MachineScheduleViewModel> machineSchedules, List<GanttScheduleViewModel> ganttSchedules, ScheduleExportOptionsViewModel options)
			{
				if (workSheet == null) throw new ArgumentNullException(nameof(workSheet));
				if (machineSchedules == null) throw new ArgumentNullException(nameof(machineSchedules));
				if (ganttSchedules == null) throw new ArgumentNullException(nameof(ganttSchedules));
				if (options == null) throw new ArgumentNullException(nameof(options));

				// Calculate column counts
				int staticCols = GetStaticColumnCount(options);
				int noteCols = GetNoteColumnCount(options);
				int totalWeeks = options.IncludeGanttData ? GetWeeksRemainingUntilEndOfYear(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)) : 0;
				int totalCols = staticCols + totalWeeks + noteCols + (options.IncludeSpecialNotes ? 1 : 0);

				// Title
				workSheet.Cells[1, 1].Value = "Combined Machine & Gantt Schedule";
				using (ExcelRange title = workSheet.Cells[1, 1, 1, totalCols])
				{
					title.Merge = true;
					title.Style.Font.Name = "Calibri";
					title.Style.Font.Bold = true;
					title.Style.Font.Size = 20;
					title.Style.Fill.PatternType = ExcelFillStyle.Solid;
					title.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 32, 96));
					title.Style.Font.Color.SetColor(Color.White);
					title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					title.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
					title.Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(0, 32, 96));
				}
				workSheet.Row(1).Height = 30;

				// Timestamp
				DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
				using (ExcelRange timestamp = workSheet.Cells[2, 1, 2, totalCols])
				{
					timestamp.Merge = true;
					timestamp.Value = $"Generated: {localDate:yyyy-MM-dd HH:mm}";
					timestamp.Style.Font.Name = "Calibri";
					timestamp.Style.Font.Bold = true;
					timestamp.Style.Font.Size = 12;
					timestamp.Style.Fill.PatternType = ExcelFillStyle.Solid;
					timestamp.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(230, 230, 230));
					timestamp.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					timestamp.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(150, 150, 150));
				}
				workSheet.Row(2).Height = 20;

				// Section Headers (Row 3)
				using (ExcelRange staticHeader = workSheet.Cells[3, 1, 3, staticCols])
				{
					staticHeader.Value = "Machine & Gantt Details";
					staticHeader.Merge = true;
					staticHeader.Style.Font.Name = "Calibri";
					staticHeader.Style.Font.Bold = true;
					staticHeader.Style.Font.Size = 11;
					staticHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
					staticHeader.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(91, 155, 213));
					staticHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				}

				if (options.IncludeGanttData)
				{
					using (ExcelRange ganttHeader = workSheet.Cells[3, staticCols + 1, 3, staticCols + totalWeeks])
					{
						ganttHeader.Value = "Gantt Schedule";
						ganttHeader.Merge = true;
						ganttHeader.Style.Font.Name = "Calibri";
						ganttHeader.Style.Font.Bold = true;
						ganttHeader.Style.Font.Size = 11;
						ganttHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
						ganttHeader.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
						ganttHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					}
				}

				int notesStart = staticCols + totalWeeks + 1;
				if (noteCols > 0)
				{
					using (ExcelRange notesHeader = workSheet.Cells[3, notesStart, 3, notesStart + noteCols - 1])
					{
						notesHeader.Value = "Additional Notes";
						notesHeader.Merge = true;
						notesHeader.Style.Font.Name = "Calibri";
						notesHeader.Style.Font.Bold = true;
						notesHeader.Style.Font.Size = 11;
						notesHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
						notesHeader.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(135, 206, 235));
						notesHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
						notesHeader.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
					}
				}

				int specialNotesCol = options.IncludeSpecialNotes ? notesStart + noteCols : 0;
				if (options.IncludeSpecialNotes)
				{
					using (ExcelRange specialNotesHeader = workSheet.Cells[3, specialNotesCol, 3, specialNotesCol])
					{
						specialNotesHeader.Value = "Special Notes";
						specialNotesHeader.Merge = true;
						specialNotesHeader.Style.Font.Name = "Calibri";
						specialNotesHeader.Style.Font.Bold = true;
						specialNotesHeader.Style.Font.Size = 11;
						specialNotesHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
						specialNotesHeader.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(91, 155, 213));
						specialNotesHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
						specialNotesHeader.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
					}
				}

				// Sub-headers (Row 4 and 5)
				var headers = new List<(string Name, bool Include, Color Color)>
=======
		}
		// Helper methods for session management
		private void SaveOptionsToSession(ScheduleExportOptionsViewModel options)
>>>>>>> b998c9d23189efe44dc7bb325dd11c8609ac17c9
		{
			("Sales Order", options.IncludeSalesOrderNumber, Color.FromArgb(91, 155, 213)),
			("Order Date", options.IncludeSalesOrderDate, Color.FromArgb(91, 155, 213)),
			("Customer", options.IncludeCustomerName, Color.FromArgb(91, 155, 213)),
			("Model", options.IncludeMachineDescriptions, Color.FromArgb(91, 155, 213)),
			("Serial #", options.IncludeSerialNumbers, Color.FromArgb(91, 155, 213)),
			("Prod Order", options.IncludeProductionOrderNumbers, Color.FromArgb(91, 155, 213)),
			("Pkg Rel Exp", options.IncludePackageReleaseDateE, Color.FromArgb(91, 155, 213)),
			("Pkg Rel Act", options.IncludePackageReleaseDateA, Color.FromArgb(91, 155, 213)),
			("Vendors", options.IncludeVendorNames, Color.FromArgb(91, 155, 213)),
			("PO #", options.IncludePoNumbers, Color.FromArgb(91, 155, 213)),
			("PO Due", options.IncludePoDueDates, Color.FromArgb(91, 155, 213)),
			("Media", options.IncludeMedia, Color.FromArgb(91, 155, 213)),
			("Spares", options.IncludeSpareParts, Color.FromArgb(91, 155, 213)),
			("Base", options.IncludeBase, Color.FromArgb(91, 155, 213)),
			("Air Seal", options.IncludeAirSeal, Color.FromArgb(91, 155, 213)),
			("Coating", options.IncludeCoatingLining, Color.FromArgb(91, 155, 213)),
			("Disasm", options.IncludeDisassembly, Color.FromArgb(91, 155, 213)),
			("Comments", options.IncludeNotes, Color.FromArgb(91, 155, 213)),
			("ENG.", options.IncludeEngineer, Color.FromArgb(91, 155, 213)),
			("QTY", options.IncludeQuantity, Color.FromArgb(91, 155, 213)),
			("App Dwg Rec'd", options.IncludeApprovedDrawingReceived, Color.FromArgb(91, 155, 213))
		};

				int colIndex = 1;
				foreach (var header in headers.Where(h => h.Include))
				{
<<<<<<< HEAD
					workSheet.Cells[5, colIndex].Value = header.Name;
=======
					ganttHeader.Value = "Gantt Schedule";
					ganttHeader.Merge = true;
					ganttHeader.Style.Font.Name = "Calibri";
					ganttHeader.Style.Font.Bold = true;
					ganttHeader.Style.Font.Size = 11;
					ganttHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
					ganttHeader.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
					ganttHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				}
			}

			int notesStart = staticCols + totalWeeks + 1;
			if (noteCols > 0)
			{
				using (ExcelRange notesHeader = workSheet.Cells[3, notesStart, 3, notesStart + noteCols - 1])
				{
					notesHeader.Value = "Additional Notes";
					notesHeader.Merge = true;
					notesHeader.Style.Font.Name = "Calibri";
					notesHeader.Style.Font.Bold = true;
					notesHeader.Style.Font.Size = 11;
					notesHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
					notesHeader.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(135, 206, 235));
					notesHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					notesHeader.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
				}
			}

			int specialNotesCol = options.IncludeSpecialNotes ? notesStart + noteCols : 0;
			if (options.IncludeSpecialNotes)
			{
				using (ExcelRange specialNotesHeader = workSheet.Cells[3, specialNotesCol, 3, specialNotesCol])
				{
					specialNotesHeader.Value = "Special Notes";
					specialNotesHeader.Merge = true;
					specialNotesHeader.Style.Font.Name = "Calibri";
					specialNotesHeader.Style.Font.Bold = true;
					specialNotesHeader.Style.Font.Size = 11;
					specialNotesHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
					specialNotesHeader.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(91, 155, 213));
					specialNotesHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					specialNotesHeader.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
				}
			}

			// Sub-headers (Row 4 and 5)
			var headers = new List<(string Name, bool Include, Color Color)>
	{
		("Order #", options.IncludeOrderNumber, Color.FromArgb(91, 155, 213)),
		("Order Date", options.IncludeSalesOrderDate, Color.FromArgb(91, 155, 213)),
		("Customer", options.IncludeCustomerName, Color.FromArgb(91, 155, 213)),
		("Model", options.IncludeMachineModel, Color.FromArgb(91, 155, 213)),
		("Serial #", options.IncludeSerialNumbers, Color.FromArgb(91, 155, 213)),
		("Prod Order", options.IncludeProductionOrderNumbers, Color.FromArgb(91, 155, 213)),
		("Pkg Rel Exp", options.IncludePackageReleaseDateE, Color.FromArgb(91, 155, 213)),
		("Pkg Rel Act", options.IncludePackageReleaseDateA, Color.FromArgb(91, 155, 213)),
		("Vendors", options.IncludeVendorNames, Color.FromArgb(91, 155, 213)),
		("PO #", options.IncludePoNumbers, Color.FromArgb(91, 155, 213)),
		("PO Due", options.IncludePoDueDates, Color.FromArgb(91, 155, 213)),
		("Media", options.IncludeMedia, Color.FromArgb(91, 155, 213)),
		("Spares", options.IncludeSpareParts, Color.FromArgb(91, 155, 213)),
		("Base", options.IncludeBase, Color.FromArgb(91, 155, 213)),
		("Air Seal", options.IncludeAirSeal, Color.FromArgb(91, 155, 213)),
		("Coating", options.IncludeCoatingLining, Color.FromArgb(91, 155, 213)),
		("Disasm", options.IncludeDisassembly, Color.FromArgb(91, 155, 213)),
		("Comments", options.IncludeNotes, Color.FromArgb(91, 155, 213)),
		("ENG.", options.IncludeEngineer, Color.FromArgb(91, 155, 213)),
		("QTY", options.IncludeQuantity, Color.FromArgb(91, 155, 213)),
		("App Dwg Rec'd", options.IncludeApprovedDrawingReceived, Color.FromArgb(91, 155, 213))
	};

			int colIndex = 1;
			foreach (var header in headers.Where(h => h.Include))
			{
				workSheet.Cells[5, colIndex].Value = header.Name;
				workSheet.Cells[5, colIndex].Style.Font.Name = "Calibri";
				workSheet.Cells[5, colIndex].Style.Font.Size = 11;
				workSheet.Cells[5, colIndex].Style.Font.Bold = true;
				workSheet.Cells[5, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
				workSheet.Cells[5, colIndex].Style.Fill.BackgroundColor.SetColor(header.Color);
				workSheet.Cells[5, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				workSheet.Cells[5, colIndex].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
				colIndex++;
			}

			if (options.IncludeGanttData)
			{
				DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
				for (int week = 1; week <= totalWeeks; week++)
				{
					int ganttCol = staticCols + week;
					int weekNumber = GetWeekOfYear(startDate, WeekStartOption.Monday);

					workSheet.Cells[4, ganttCol].Value = $"W{weekNumber}";
					workSheet.Cells[4, ganttCol].Style.Font.Name = "Calibri";
					workSheet.Cells[4, ganttCol].Style.Font.Size = 10;
					workSheet.Cells[4, ganttCol].Style.Font.Bold = true;
					workSheet.Cells[4, ganttCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Cells[4, ganttCol].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
					workSheet.Cells[4, ganttCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					workSheet.Cells[4, ganttCol].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));

					workSheet.Cells[5, ganttCol].Value = startDate.ToString("MMM d");
					workSheet.Cells[5, ganttCol].Style.Font.Name = "Calibri";
					workSheet.Cells[5, ganttCol].Style.Font.Size = 8;
					workSheet.Cells[5, ganttCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Cells[5, ganttCol].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
					workSheet.Cells[5, ganttCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					workSheet.Cells[5, ganttCol].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));

					startDate = startDate.AddDays(7);
				}
			}

			var noteHeaders = new List<(string Name, bool Include, Color Color)>
	{
		("PreOrder", options.IncludePreOrder, Color.FromArgb(255, 218, 185)),
		("Scope", options.IncludeScope, Color.FromArgb(240, 230, 140)),
		("Act Hrs", options.IncludeActualAssemblyHours, Color.FromArgb(144, 238, 144)),
		("Rework Hrs", options.IncludeReworkHours, Color.FromArgb(173, 216, 230)),
		("NamePlate", options.IncludeNamePlate, Color.FromArgb(221, 160, 221))
	};
			var includedNoteHeaders = noteHeaders.Where(nh => nh.Include).ToList();

			if (includedNoteHeaders.Any())
			{
				colIndex = notesStart;
				foreach (var nh in includedNoteHeaders)
				{
					workSheet.Cells[5, colIndex].Value = nh.Name;
>>>>>>> b998c9d23189efe44dc7bb325dd11c8609ac17c9
					workSheet.Cells[5, colIndex].Style.Font.Name = "Calibri";
					workSheet.Cells[5, colIndex].Style.Font.Size = 11;
					workSheet.Cells[5, colIndex].Style.Font.Bold = true;
					workSheet.Cells[5, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Cells[5, colIndex].Style.Fill.BackgroundColor.SetColor(header.Color);
					workSheet.Cells[5, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					workSheet.Cells[5, colIndex].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
					colIndex++;
				}

<<<<<<< HEAD
				if (options.IncludeGanttData)
				{
					DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
					for (int week = 1; week <= totalWeeks; week++)
					{
						int ganttCol = staticCols + week;
						int weekNumber = GetWeekOfYear(startDate, WeekStartOption.Monday);

						workSheet.Cells[4, ganttCol].Value = $"W{weekNumber}";
						workSheet.Cells[4, ganttCol].Style.Font.Name = "Calibri";
						workSheet.Cells[4, ganttCol].Style.Font.Size = 10;
						workSheet.Cells[4, ganttCol].Style.Font.Bold = true;
						workSheet.Cells[4, ganttCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
						workSheet.Cells[4, ganttCol].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
						workSheet.Cells[4, ganttCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
						workSheet.Cells[4, ganttCol].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));

						workSheet.Cells[5, ganttCol].Value = startDate.ToString("MMM d");
						workSheet.Cells[5, ganttCol].Style.Font.Name = "Calibri";
						workSheet.Cells[5, ganttCol].Style.Font.Size = 8;
						workSheet.Cells[5, ganttCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
						workSheet.Cells[5, ganttCol].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
						workSheet.Cells[5, ganttCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
						workSheet.Cells[5, ganttCol].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));

						startDate = startDate.AddDays(7);
					}
				}

				var noteHeaders = new List<(string Name, bool Include, Color Color)>
		{
			("PreOrder", options.IncludePreOrder, Color.FromArgb(255, 218, 185)),
			("Scope", options.IncludeScope, Color.FromArgb(240, 230, 140)),
			("Act Hrs", options.IncludeActualAssemblyHours, Color.FromArgb(144, 238, 144)),
			("Rework Hrs", options.IncludeReworkHours, Color.FromArgb(173, 216, 230)),
			("NamePlate", options.IncludeNamePlate, Color.FromArgb(221, 160, 221))
		};
				var includedNoteHeaders = noteHeaders.Where(nh => nh.Include).ToList();
=======
			if (options.IncludeSpecialNotes && specialNotesCol > 0)
			{
				workSheet.Cells[5, specialNotesCol].Value = "Notes"; // Sub-header under "Special Notes"
				workSheet.Cells[5, specialNotesCol].Style.Font.Name = "Calibri";
				workSheet.Cells[5, specialNotesCol].Style.Font.Bold = true;
				workSheet.Cells[5, specialNotesCol].Style.Font.Size = 11;
				workSheet.Cells[5, specialNotesCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
				workSheet.Cells[5, specialNotesCol].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(91, 155, 213));
				workSheet.Cells[5, specialNotesCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				workSheet.Cells[5, specialNotesCol].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
			}

			// Data population
			var combinedData = machineSchedules
				.GroupJoin(ganttSchedules,
					m => m.OrderNumber,
					g => g.OrderNumber,
					(m, gs) => new { Machine = m, Gantts = gs.DefaultIfEmpty() })
				.SelectMany(x => x.Gantts.Select(g => new { x.Machine, Gantt = g ?? null }));

			int row = 6;
			foreach (var item in combinedData)
			{
				colIndex = 1;
				// Row 1: Machine Operations
				if (options.IncludeOrderNumber) workSheet.Cells[row, colIndex++].Value = item.Machine?.OrderNumber ?? item.Gantt?.OrderNumber;
				if (options.IncludeSalesOrderDate) workSheet.Cells[row, colIndex++].Value = item.Machine?.SalesOrderDate;
				if (options.IncludeCustomerName) workSheet.Cells[row, colIndex++].Value = item.Machine?.CustomerName ?? item.Gantt?.CustomerName;
				if (options.IncludeMachineModel) workSheet.Cells[row, colIndex++].Value = item.Machine?.MachineModel ?? item.Gantt?.MachineModel;
				if (options.IncludeSerialNumbers) workSheet.Cells[row, colIndex++].Value = item.Machine?.SerialNumbers;
				if (options.IncludeProductionOrderNumbers) workSheet.Cells[row, colIndex++].Value = item.Machine?.ProductionOrderNumbers;
				if (options.IncludePackageReleaseDateE) workSheet.Cells[row, colIndex++].Value = item.Machine?.PackageReleaseDateE;
				if (options.IncludePackageReleaseDateA) workSheet.Cells[row, colIndex++].Value = item.Machine?.PackageReleaseDateA;
				if (options.IncludeVendorNames) workSheet.Cells[row, colIndex++].Value = item.Machine?.VendorNames;
				if (options.IncludePoNumbers) workSheet.Cells[row, colIndex++].Value = item.Machine?.PoNumbers;
				if (options.IncludePoDueDates) workSheet.Cells[row, colIndex++].Value = item.Machine?.PoDueDates;
				if (options.IncludeMedia) workSheet.Cells[row, colIndex++].Value = item.Machine?.Media ?? (item.Gantt?.Media == "Yes" ? "✓" : "");
				if (options.IncludeSpareParts) workSheet.Cells[row, colIndex++].Value = item.Machine?.SpareParts ?? (item.Gantt?.SpareParts == "Yes" ? "✓" : "");
				if (options.IncludeBase) workSheet.Cells[row, colIndex++].Value = item.Machine?.Base;
				if (options.IncludeAirSeal) workSheet.Cells[row, colIndex++].Value = item.Machine?.AirSeal;
				if (options.IncludeCoatingLining) workSheet.Cells[row, colIndex++].Value = item.Machine?.CoatingLining;
				if (options.IncludeDisassembly) workSheet.Cells[row, colIndex++].Value = item.Machine?.Disassembly;
				if (options.IncludeNotes) workSheet.Cells[row, colIndex++].Value = item.Machine?.Comments;
				if (options.IncludeEngineer) workSheet.Cells[row, colIndex++].Value = item.Gantt?.Engineer;
				if (options.IncludeQuantity) workSheet.Cells[row, colIndex++].Value = item.Gantt?.Quantity;
				if (options.IncludeApprovedDrawingReceived)
				{
					var date = item.Gantt?.ApprovedDrawingReceived;
					workSheet.Cells[row, colIndex++].Value = (date != DateTime.MinValue) ? date : null;
				}

				// Gantt Row (Row below operations row)
				int ganttRow = row + 1;
				if (options.IncludeGanttData && item.Gantt?.GanttData != null)
				{
					ApplyAllMilestonesToGantt(workSheet, ganttRow, staticCols, item.Gantt.GanttData, options);
				}
>>>>>>> b998c9d23189efe44dc7bb325dd11c8609ac17c9

				if (includedNoteHeaders.Any())
				{
					colIndex = notesStart;
<<<<<<< HEAD
					foreach (var nh in includedNoteHeaders)
=======
					if (options.IncludePreOrder) workSheet.Cells[row, colIndex++].Value = item.Machine?.PreOrder;
					if (options.IncludeScope) workSheet.Cells[row, colIndex++].Value = item.Machine?.Scope;
					if (options.IncludeActualAssemblyHours) workSheet.Cells[row, colIndex++].Value = item.Machine?.ActualAssemblyHours;
					if (options.IncludeReworkHours) workSheet.Cells[row, colIndex++].Value = item.Machine?.ReworkHours;
					if (options.IncludeNamePlate) workSheet.Cells[row, colIndex++].Value = item.Machine?.NamePlate;
				}

				// Special Notes (Row 2)
				if (options.IncludeSpecialNotes && specialNotesCol > 0)
				{
					workSheet.Cells[ganttRow, specialNotesCol].Value = item.Gantt?.SpecialNotes;
					workSheet.Cells[ganttRow, specialNotesCol].Style.WrapText = true;
					workSheet.Cells[ganttRow, specialNotesCol].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
				}

				// Set row heights
				workSheet.Row(row).Height = 20;
				workSheet.Row(row + 1).Height = 20;
				workSheet.Row(row + 2).Height = 15;

				row += 3;
			}

			// Styling
			if (row > 6)
			{
				for (int i = 6; i < row; i += 3)
				{
					// Row 1: Machine Info (gray background + borders)
					var opRow = workSheet.Cells[i, 1, i, totalCols];
					opRow.Style.Font.Name = "Calibri";
					opRow.Style.Font.Size = 10;
					opRow.Style.Fill.PatternType = ExcelFillStyle.Solid;
					opRow.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
					opRow.Style.Border.Top.Style = ExcelBorderStyle.Thin;
					opRow.Style.Border.Left.Style = ExcelBorderStyle.Thin;
					opRow.Style.Border.Right.Style = ExcelBorderStyle.Thin;
					opRow.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
					opRow.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
					opRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

					// Row 2: Gantt Info (no fill, borders only)
					var ganttRow = workSheet.Cells[i + 1, 1, i + 1, totalCols];
					ganttRow.Style.Font.Name = "Calibri";
					ganttRow.Style.Font.Size = 10;
					ganttRow.Style.Border.Top.Style = ExcelBorderStyle.Thin;
					ganttRow.Style.Border.Left.Style = ExcelBorderStyle.Thin;
					ganttRow.Style.Border.Right.Style = ExcelBorderStyle.Thin;
					ganttRow.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
					ganttRow.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
					ganttRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

					// Row 3: Spacer — no styling at all, no border, no fill
				}
			}

			// Column Widths
			colIndex = 1;
			if (options.IncludeOrderNumber) workSheet.Column(colIndex++).Width = 15;
			if (options.IncludeSalesOrderDate) workSheet.Column(colIndex++).Width = 12;
			if (options.IncludeCustomerName) { workSheet.Column(colIndex).Width = 25; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludeMachineModel) { workSheet.Column(colIndex).Width = 24; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludeSerialNumbers) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludeProductionOrderNumbers) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludePackageReleaseDateE) workSheet.Column(colIndex++).Width = 15;
			if (options.IncludePackageReleaseDateA) workSheet.Column(colIndex++).Width = 15;
			if (options.IncludeVendorNames) { workSheet.Column(colIndex).Width = 25; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludePoNumbers) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludePoDueDates) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludeMedia) workSheet.Column(colIndex++).Width = 8;
			if (options.IncludeSpareParts) workSheet.Column(colIndex++).Width = 12;
			if (options.IncludeBase) workSheet.Column(colIndex++).Width = 10;
			if (options.IncludeAirSeal) workSheet.Column(colIndex++).Width = 10;
			if (options.IncludeCoatingLining) workSheet.Column(colIndex++).Width = 10;
			if (options.IncludeDisassembly) workSheet.Column(colIndex++).Width = 10;
			if (options.IncludeNotes) { workSheet.Column(colIndex).Width = 30; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludeEngineer) workSheet.Column(colIndex++).Width = 6;
			if (options.IncludeQuantity) workSheet.Column(colIndex++).Width = 5;
			if (options.IncludeApprovedDrawingReceived) workSheet.Column(colIndex++).Width = 19;

			if (options.IncludeGanttData)
			{
				for (int i = staticCols + 1; i <= staticCols + totalWeeks; i++)
					workSheet.Column(i).Width = 5.5;
			}

			if (includedNoteHeaders.Any())
			{
				colIndex = notesStart;
				if (options.IncludePreOrder) { workSheet.Column(colIndex).Width = 20; workSheet.Column(colIndex++).Style.WrapText = true; }
				if (options.IncludeScope) { workSheet.Column(colIndex).Width = 20; workSheet.Column(colIndex++).Style.WrapText = true; }
				if (options.IncludeActualAssemblyHours) { workSheet.Column(colIndex).Width = 12; workSheet.Column(colIndex++).Style.WrapText = true; }
				if (options.IncludeReworkHours) { workSheet.Column(colIndex).Width = 12; workSheet.Column(colIndex++).Style.WrapText = true; }
				if (options.IncludeNamePlate) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }
			}

			if (options.IncludeSpecialNotes && specialNotesCol > 0)
			{
				workSheet.Column(specialNotesCol).Width = 40;
				workSheet.Column(specialNotesCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
				workSheet.Column(specialNotesCol).Style.WrapText = true;
			}

			// Print Settings
			var print = workSheet.PrinterSettings;
			print.FitToPage = true;
			print.FitToWidth = 1;
			print.FitToHeight = 0; // Allow any number of vertical pages
			print.Orientation = eOrientation.Landscape;
			print.PaperSize = ePaperSize.Ledger;
			print.HorizontalCentered = true;
			print.TopMargin = 0.5M;
			print.BottomMargin = 0.5M;
			print.LeftMargin = 0.25M;
			print.RightMargin = 0.25M;

			// Freeze panes
			workSheet.View.FreezePanes(6, 1); // Freeze only rows above 6
		}

		private int GetStaticColumnCount(ScheduleExportOptionsViewModel options)
		{
			int count = 0;
			if (options.IncludeOrderNumber) count++;
			if (options.IncludeSalesOrderDate) count++;
			if (options.IncludeCustomerName) count++;
			if (options.IncludeMachineModel) count++;
			if (options.IncludeSerialNumbers) count++;
			if (options.IncludeProductionOrderNumbers) count++;
			if (options.IncludePackageReleaseDateE) count++;
			if (options.IncludePackageReleaseDateA) count++;
			if (options.IncludeVendorNames) count++;
			if (options.IncludePoNumbers) count++;
			if (options.IncludePoDueDates) count++;
			if (options.IncludeMedia) count++;
			if (options.IncludeSpareParts) count++;
			if (options.IncludeBase) count++;
			if (options.IncludeAirSeal) count++;
			if (options.IncludeCoatingLining) count++;
			if (options.IncludeDisassembly) count++;
			if (options.IncludeNotes) count++;
			if (options.IncludeEngineer) count++;
			if (options.IncludeQuantity) count++;
			if (options.IncludeApprovedDrawingReceived) count++;
			return count;
		}

		private int GetNoteColumnCount(ScheduleExportOptionsViewModel options)
		{
			int count = 0;
			if (options.IncludePreOrder) count++;
			if (options.IncludeScope) count++;
			if (options.IncludeActualAssemblyHours) count++;
			if (options.IncludeReworkHours) count++;
			if (options.IncludeNamePlate) count++;
			return count;
		}

		private void SetupMachineSchedulesWorksheet(ExcelWorksheet workSheet, List<MachineScheduleViewModel> schedules, ScheduleExportOptionsViewModel options)
		{
			workSheet.Cells[1, 1].Value = "Machine Schedule Report";
			using (ExcelRange title = workSheet.Cells[1, 1, 1, 25])
			{
				title.Merge = true;
				title.Style.Font.Name = "Arial";
				title.Style.Font.Bold = true;
				title.Style.Font.Size = 20;
				title.Style.Fill.PatternType = ExcelFillStyle.Solid;
				title.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 51, 102));
				title.Style.Font.Color.SetColor(Color.White);
				title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				title.Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(0, 51, 102));
			}
			workSheet.Row(1).Height = 30;

			DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
			using (ExcelRange timestamp = workSheet.Cells[2, 1, 2, 25])
			{
				timestamp.Merge = true;
				timestamp.Value = $"Generated: {localDate:yyyy-MM-dd HH:mm}";
				timestamp.Style.Font.Name = "Arial";
				timestamp.Style.Font.Bold = true;
				timestamp.Style.Font.Size = 12;
				timestamp.Style.Fill.PatternType = ExcelFillStyle.Solid;
				timestamp.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(230, 230, 230));
				timestamp.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				timestamp.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(150, 150, 150));
			}
			workSheet.Row(2).Height = 20;

			var headers = new List<(string Name, bool Include, Color Color)>
	{
		("Order #", options.IncludeOrderNumber, Color.FromArgb(173, 216, 230)),
		("Order Date", options.IncludeSalesOrderDate, Color.FromArgb(173, 216, 230)),
		("Customer", options.IncludeCustomerName, Color.FromArgb(173, 216, 230)),
		("Model", options.IncludeMachineModel, Color.FromArgb(173, 216, 230)),
		("Serial #", options.IncludeSerialNumbers, Color.FromArgb(173, 216, 230)),
		("Prod Order", options.IncludeProductionOrderNumbers, Color.FromArgb(173, 216, 230)),
		("Pkg Rel Exp", options.IncludePackageReleaseDateE, Color.FromArgb(173, 216, 230)),
		("Pkg Rel Act", options.IncludePackageReleaseDateA, Color.FromArgb(173, 216, 230)),
		("Vendors", options.IncludeVendorNames, Color.FromArgb(173, 216, 230)),
		("PO #", options.IncludePoNumbers, Color.FromArgb(173, 216, 230)),
		("PO Due", options.IncludePoDueDates, Color.FromArgb(173, 216, 230)),
		("Media", options.IncludeMedia, Color.FromArgb(173, 216, 230)),
		("Spares", options.IncludeSpareParts, Color.FromArgb(173, 216, 230)),
		("Base", options.IncludeBase, Color.FromArgb(173, 216, 230)),
		("Air Seal", options.IncludeAirSeal, Color.FromArgb(173, 216, 230)),
		("Coating", options.IncludeCoatingLining, Color.FromArgb(173, 216, 230)),
		("Disasm", options.IncludeDisassembly, Color.FromArgb(173, 216, 230)),
		("Comments", options.IncludeNotes, Color.FromArgb(173, 216, 230))
	};

			int colIndex = 1;
			foreach (var header in headers.Where(h => h.Include))
			{
				var cell = workSheet.Cells[3, colIndex];
				cell.Value = header.Name;
				cell.Style.Font.Name = "Arial";
				cell.Style.Font.Bold = true;
				cell.Style.Font.Size = 11;
				cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
				cell.Style.Fill.BackgroundColor.SetColor(header.Color);
				cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
				cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				colIndex++;
			}

			var noteHeaders = new List<(string Name, bool Include, Color Color)>
	{
		("PreOrder", options.IncludePreOrder, Color.FromArgb(255, 218, 185)),
		("Scope", options.IncludeScope, Color.FromArgb(240, 230, 140)),
		("Act Hrs", options.IncludeActualAssemblyHours, Color.FromArgb(144, 238, 144)),
		("Rework Hrs", options.IncludeReworkHours, Color.FromArgb(173, 216, 230)),
		("NamePlate", options.IncludeNamePlate, Color.FromArgb(221, 160, 221))
	};
			var includedNoteHeaders = noteHeaders.Where(nh => nh.Include).ToList();

			if (includedNoteHeaders.Any())
			{
				int notesStartCol = colIndex;
				int notesEndCol = notesStartCol + includedNoteHeaders.Count - 1;

				using (var notesHeaderRange = workSheet.Cells[3, notesStartCol, 3, notesEndCol])
				{
					notesHeaderRange.Merge = true;
					notesHeaderRange.Value = "Additional Notes";
					notesHeaderRange.Style.Font.Name = "Arial";
					notesHeaderRange.Style.Font.Bold = true;
					notesHeaderRange.Style.Font.Size = 11;
					notesHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
					notesHeaderRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(135, 206, 235));
					notesHeaderRange.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
					notesHeaderRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				}

				int subCol = notesStartCol;
				foreach (var nh in includedNoteHeaders)
				{
					var cell = workSheet.Cells[4, subCol];
					cell.Value = nh.Name;
					cell.Style.Font.Name = "Arial";
					cell.Style.Font.Bold = true;
					cell.Style.Font.Size = 10;
					cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
					cell.Style.Fill.BackgroundColor.SetColor(nh.Color);
					cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
					cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					subCol++;
				}
				colIndex = notesEndCol + 1;
			}

			int row = 5;
			foreach (var schedule in schedules)
			{
				colIndex = 1;
				if (options.IncludeOrderNumber) workSheet.Cells[row, colIndex++].Value = schedule.OrderNumber ?? "";
				if (options.IncludeSalesOrderDate) workSheet.Cells[row, colIndex++].Value = schedule.SalesOrderDate ?? "";
				if (options.IncludeCustomerName) workSheet.Cells[row, colIndex++].Value = schedule.CustomerName ?? "";
				if (options.IncludeMachineModel) workSheet.Cells[row, colIndex++].Value = schedule.MachineModel ?? "";
				if (options.IncludeSerialNumbers) workSheet.Cells[row, colIndex++].Value = schedule.SerialNumbers ?? "";
				if (options.IncludeProductionOrderNumbers) workSheet.Cells[row, colIndex++].Value = schedule.ProductionOrderNumbers ?? "";
				if (options.IncludePackageReleaseDateE) workSheet.Cells[row, colIndex++].Value = schedule.PackageReleaseDateE ?? "";
				if (options.IncludePackageReleaseDateA) workSheet.Cells[row, colIndex++].Value = schedule.PackageReleaseDateA ?? "";
				if (options.IncludeVendorNames) workSheet.Cells[row, colIndex++].Value = schedule.VendorNames ?? "";
				if (options.IncludePoNumbers) workSheet.Cells[row, colIndex++].Value = schedule.PoNumbers ?? "";
				if (options.IncludePoDueDates) workSheet.Cells[row, colIndex++].Value = schedule.PoDueDates ?? "";
				if (options.IncludeMedia) workSheet.Cells[row, colIndex++].Value = schedule.Media ?? "";
				if (options.IncludeSpareParts) workSheet.Cells[row, colIndex++].Value = schedule.SpareParts ?? "";
				if (options.IncludeBase) workSheet.Cells[row, colIndex++].Value = schedule.Base ?? "";
				if (options.IncludeAirSeal) workSheet.Cells[row, colIndex++].Value = schedule.AirSeal ?? "";
				if (options.IncludeCoatingLining) workSheet.Cells[row, colIndex++].Value = schedule.CoatingLining ?? "";
				if (options.IncludeDisassembly) workSheet.Cells[row, colIndex++].Value = schedule.Disassembly ?? "";
				if (options.IncludeNotes) workSheet.Cells[row, colIndex++].Value = schedule.Comments ?? "";
				if (options.IncludePreOrder) workSheet.Cells[row, colIndex++].Value = schedule.PreOrder ?? "";
				if (options.IncludeScope) workSheet.Cells[row, colIndex++].Value = schedule.Scope ?? "";
				if (options.IncludeActualAssemblyHours) workSheet.Cells[row, colIndex++].Value = schedule.ActualAssemblyHours ?? "";
				if (options.IncludeReworkHours) workSheet.Cells[row, colIndex++].Value = schedule.ReworkHours ?? "";
				if (options.IncludeNamePlate) workSheet.Cells[row, colIndex++].Value = schedule.NamePlate ?? "";
				row++;
			}

			if (row > 5)
			{
				using (var range = workSheet.Cells[5, 1, row - 1, colIndex - 1])
				{
					range.Style.Font.Name = "Arial";
					range.Style.Font.Size = 10;
					range.Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(0, 51, 102));
					range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
					range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
					range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
					range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
					range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
				}

				for (int i = 5; i < row; i++)
				{
					using (var rowRange = workSheet.Cells[i, 1, i, colIndex - 1])
>>>>>>> b998c9d23189efe44dc7bb325dd11c8609ac17c9
					{
						workSheet.Cells[5, colIndex].Value = nh.Name;
						workSheet.Cells[5, colIndex].Style.Font.Name = "Calibri";
						workSheet.Cells[5, colIndex].Style.Font.Bold = true;
						workSheet.Cells[5, colIndex].Style.Font.Size = 10;
						workSheet.Cells[5, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
						workSheet.Cells[5, colIndex].Style.Fill.BackgroundColor.SetColor(nh.Color);
						workSheet.Cells[5, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
						workSheet.Cells[5, colIndex].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
						colIndex++;
					}
				}
<<<<<<< HEAD

				if (options.IncludeSpecialNotes && specialNotesCol > 0)
				{
					workSheet.Cells[5, specialNotesCol].Value = "Notes"; // Sub-header under "Special Notes"
					workSheet.Cells[5, specialNotesCol].Style.Font.Name = "Calibri";
					workSheet.Cells[5, specialNotesCol].Style.Font.Bold = true;
					workSheet.Cells[5, specialNotesCol].Style.Font.Size = 11;
					workSheet.Cells[5, specialNotesCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Cells[5, specialNotesCol].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(91, 155, 213));
					workSheet.Cells[5, specialNotesCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					workSheet.Cells[5, specialNotesCol].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
				}

				// Data population
				var combinedData = machineSchedules
					.GroupJoin(ganttSchedules,
						m => m.SalesOrderNumber,
						g => g.OrderNumber,
						(m, gs) => new { Machine = m, Gantts = gs.DefaultIfEmpty() })
					.SelectMany(x => x.Gantts.Select(g => new { x.Machine, Gantt = g ?? null }));

				int row = 6;
				foreach (var item in combinedData)
				{
					colIndex = 1;
					// ROW 1: MACHINE OPERATIONS
					if (options.IncludeSalesOrderNumber) workSheet.Cells[row, colIndex++].Value = item.Machine?.SalesOrderNumber;
					if (options.IncludeSalesOrderDate) workSheet.Cells[row, colIndex++].Value = item.Machine?.SalesOrderDate;
					if (options.IncludeCustomerName) workSheet.Cells[row, colIndex++].Value = item.Machine?.CustomerName;
					if (options.IncludeMachineDescriptions) workSheet.Cells[row, colIndex++].Value = item.Machine?.MachineDescriptions;
					if (options.IncludeSerialNumbers) workSheet.Cells[row, colIndex++].Value = item.Machine?.SerialNumbers;
					if (options.IncludeProductionOrderNumbers) workSheet.Cells[row, colIndex++].Value = item.Machine?.ProductionOrderNumbers;
					if (options.IncludePackageReleaseDateE) workSheet.Cells[row, colIndex++].Value = item.Machine?.PackageReleaseDateE;
					if (options.IncludePackageReleaseDateA) workSheet.Cells[row, colIndex++].Value = item.Machine?.PackageReleaseDateA;
					if (options.IncludeVendorNames) workSheet.Cells[row, colIndex++].Value = item.Machine?.VendorNames;
					if (options.IncludePoNumbers) workSheet.Cells[row, colIndex++].Value = item.Machine?.PoNumbers;
					if (options.IncludePoDueDates) workSheet.Cells[row, colIndex++].Value = item.Machine?.PoDueDates;
					if (options.IncludeMedia) workSheet.Cells[row, colIndex++].Value = item.Machine?.Media;
					if (options.IncludeSpareParts) workSheet.Cells[row, colIndex++].Value = item.Machine?.SpareParts;
					if (options.IncludeBase) workSheet.Cells[row, colIndex++].Value = item.Machine?.Base;
					if (options.IncludeAirSeal) workSheet.Cells[row, colIndex++].Value = item.Machine?.AirSeal;
					if (options.IncludeCoatingLining) workSheet.Cells[row, colIndex++].Value = item.Machine?.CoatingLining;
					if (options.IncludeDisassembly) workSheet.Cells[row, colIndex++].Value = item.Machine?.Disassembly;
					if (options.IncludeNotes) workSheet.Cells[row, colIndex++].Value = item.Machine?.Comments;
					if (options.IncludeEngineer) workSheet.Cells[row, colIndex++].Value = item.Gantt?.Engineer;
					if (options.IncludeQuantity) workSheet.Cells[row, colIndex++].Value = item.Gantt?.Quantity;
					if (options.IncludeApprovedDrawingReceived)
					{
						var date = item.Gantt?.ApprovedDrawingReceived;
						workSheet.Cells[row, colIndex++].Value = (date.HasValue && date.Value != DateTime.MinValue)
							? date.Value.ToString("MMM-dd-yyyy") : null;
					}

					// GANTT ROW (Row below operations row)
					int ganttRow = row + 1;
					if (options.IncludeGanttData && item.Gantt?.GanttData != null)
					{
						ApplyAllMilestonesToGantt(workSheet, ganttRow, staticCols, item.Gantt.GanttData, options);
					}

					// Notes (Row 1)
					if (includedNoteHeaders.Any())
					{
						colIndex = notesStart;
						if (options.IncludePreOrder) workSheet.Cells[row, colIndex++].Value = item.Machine?.PreOrder;
						if (options.IncludeScope) workSheet.Cells[row, colIndex++].Value = item.Machine?.Scope;
						if (options.IncludeActualAssemblyHours) workSheet.Cells[row, colIndex++].Value = item.Machine?.ActualAssemblyHours;
						if (options.IncludeReworkHours) workSheet.Cells[row, colIndex++].Value = item.Machine?.ReworkHours;
						if (options.IncludeNamePlate) workSheet.Cells[row, colIndex++].Value = item.Machine?.NamePlate;
					}

					// Special Notes (Row 2)
					if (options.IncludeSpecialNotes && specialNotesCol > 0)
					{
						workSheet.Cells[ganttRow, specialNotesCol].Value = item.Gantt?.SpecialNotes;
						workSheet.Cells[ganttRow, specialNotesCol].Style.WrapText = true;
						workSheet.Cells[ganttRow, specialNotesCol].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
					}

					// Set row heights
					workSheet.Row(row).Height = 20;
					workSheet.Row(row + 1).Height = 20;
					workSheet.Row(row + 2).Height = 15;
					//workSheet.Row(ganttRow).Height = 18;
				

					row += 3;
				}


				// Styling
				if (row > 6)
				{
					for (int i = 6; i < row; i += 3)
					{
						// Row 1: Machine Info (gray background + borders)
						var opRow = workSheet.Cells[i, 1, i, totalCols];
						opRow.Style.Font.Name = "Calibri";
						opRow.Style.Font.Size = 10;
						opRow.Style.Fill.PatternType = ExcelFillStyle.Solid;
						opRow.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
						opRow.Style.Border.Top.Style = ExcelBorderStyle.Thin;
						opRow.Style.Border.Left.Style = ExcelBorderStyle.Thin;
						opRow.Style.Border.Right.Style = ExcelBorderStyle.Thin;
						opRow.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
						opRow.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
						opRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

						// Row 2: Gantt Info (no fill, borders only)
						var ganttRow = workSheet.Cells[i + 1, 1, i + 1, totalCols];
						ganttRow.Style.Font.Name = "Calibri";
						ganttRow.Style.Font.Size = 10;
						ganttRow.Style.Border.Top.Style = ExcelBorderStyle.Thin;
						ganttRow.Style.Border.Left.Style = ExcelBorderStyle.Thin;
						ganttRow.Style.Border.Right.Style = ExcelBorderStyle.Thin;
						ganttRow.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
						ganttRow.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
						ganttRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

						// Row 3: Spacer — no styling at all, no border, no fill
					}
				}

				// Column Widths
				colIndex = 1;
				if (options.IncludeSalesOrderNumber) workSheet.Column(colIndex++).Width = 15;
				if (options.IncludeSalesOrderDate) workSheet.Column(colIndex++).Width = 12;
				if (options.IncludeCustomerName) { workSheet.Column(colIndex).Width = 25; workSheet.Column(colIndex++).Style.WrapText = true; }
				if (options.IncludeMachineDescriptions) { workSheet.Column(colIndex).Width = 24; workSheet.Column(colIndex++).Style.WrapText = true; }
				if (options.IncludeSerialNumbers) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }
				if (options.IncludeProductionOrderNumbers) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }
				if (options.IncludePackageReleaseDateE) workSheet.Column(colIndex++).Width = 15;
				if (options.IncludePackageReleaseDateA) workSheet.Column(colIndex++).Width = 15;
				if (options.IncludeVendorNames) { workSheet.Column(colIndex).Width = 25; workSheet.Column(colIndex++).Style.WrapText = true; }
				if (options.IncludePoNumbers) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }
				if (options.IncludePoDueDates) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }
				if (options.IncludeMedia) workSheet.Column(colIndex++).Width = 8;
				if (options.IncludeSpareParts) workSheet.Column(colIndex++).Width = 12;
				if (options.IncludeBase) workSheet.Column(colIndex++).Width = 10;
				if (options.IncludeAirSeal) workSheet.Column(colIndex++).Width = 10;
				if (options.IncludeCoatingLining) workSheet.Column(colIndex++).Width = 10;
				if (options.IncludeDisassembly) workSheet.Column(colIndex++).Width = 10;
				if (options.IncludeNotes) { workSheet.Column(colIndex).Width = 30; workSheet.Column(colIndex++).Style.WrapText = true; }
				if (options.IncludeEngineer) workSheet.Column(colIndex++).Width = 6;
				if (options.IncludeQuantity) workSheet.Column(colIndex++).Width = 5;
				if (options.IncludeApprovedDrawingReceived) workSheet.Column(colIndex++).Width = 19;

				if (options.IncludeGanttData)
				{
					for (int i = staticCols + 1; i <= staticCols + totalWeeks; i++)
						workSheet.Column(i).Width = 5.5;
				}

				if (includedNoteHeaders.Any())
				{
					colIndex = notesStart;
					if (options.IncludePreOrder) { workSheet.Column(colIndex).Width = 20; workSheet.Column(colIndex++).Style.WrapText = true; }
					if (options.IncludeScope) { workSheet.Column(colIndex).Width = 20; workSheet.Column(colIndex++).Style.WrapText = true; }
					if (options.IncludeActualAssemblyHours) { workSheet.Column(colIndex).Width = 12; workSheet.Column(colIndex++).Style.WrapText = true; }
					if (options.IncludeReworkHours) { workSheet.Column(colIndex).Width = 12; workSheet.Column(colIndex++).Style.WrapText = true; }
					if (options.IncludeNamePlate) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }
				}

				if (options.IncludeSpecialNotes && specialNotesCol > 0)
				{
					workSheet.Column(specialNotesCol).Width = 40;
					workSheet.Column(specialNotesCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
					workSheet.Column(specialNotesCol).Style.WrapText = true;
				}

				// ===== PRINT SETTINGS =====
				var print = workSheet.PrinterSettings;

				// Auto-fit width for printing
				print.FitToPage = true;
				print.FitToWidth = 1;
				print.FitToHeight = 0; // allow any number of vertical pages

				// Landscape orientation for wide reports
				print.Orientation = eOrientation.Landscape;

				// Paper size — use Ledger by default (for 11x17 inches)
				// You can change this to ePaperSize.Legal if you prefer 8.5x14
				print.PaperSize = ePaperSize.Ledger;

				// Optional: center horizontally on page
				print.HorizontalCentered = true;

				// Optional: set margins (in inches)
				print.TopMargin = 0.5M;
				print.BottomMargin = 0.5M;
				print.LeftMargin = 0.25M;
				print.RightMargin = 0.25M;



				// Freeze panes
				workSheet.View.FreezePanes(6, 1); // Freeze only rows above 6
			}

			private int GetStaticColumnCount(ScheduleExportOptionsViewModel options)
			{
				int count = 0;
				if (options.IncludeSalesOrderNumber) count++;
				if (options.IncludeSalesOrderDate) count++;
				if (options.IncludeCustomerName) count++;
				if (options.IncludeMachineDescriptions) count++;
				if (options.IncludeSerialNumbers) count++;
				if (options.IncludeProductionOrderNumbers) count++;
				if (options.IncludePackageReleaseDateE) count++;
				if (options.IncludePackageReleaseDateA) count++;
				if (options.IncludeVendorNames) count++;
				if (options.IncludePoNumbers) count++;
				if (options.IncludePoDueDates) count++;
				if (options.IncludeMedia) count++;
				if (options.IncludeSpareParts) count++;
				if (options.IncludeBase) count++;
				if (options.IncludeAirSeal) count++;
				if (options.IncludeCoatingLining) count++;
				if (options.IncludeDisassembly) count++;
				if (options.IncludeNotes) count++;
				if (options.IncludeEngineer) count++;
				if (options.IncludeQuantity) count++;
				if (options.IncludeApprovedDrawingReceived) count++;
				return count;
			}

			private int GetNoteColumnCount(ScheduleExportOptionsViewModel options)
			{
				int count = 0;
				if (options.IncludePreOrder) count++;
				if (options.IncludeScope) count++;
				if (options.IncludeActualAssemblyHours) count++;
				if (options.IncludeReworkHours) count++;
				if (options.IncludeNamePlate) count++;
				return count;
			}
			private void SetupMachineSchedulesWorksheet(ExcelWorksheet workSheet, List<MachineScheduleViewModel> schedules, ScheduleExportOptionsViewModel options)
			{
				workSheet.Cells[1, 1].Value = "Machine Schedule Report";
				using (ExcelRange title = workSheet.Cells[1, 1, 1, 25])
				{
					title.Merge = true;
					title.Style.Font.Name = "Arial";
					title.Style.Font.Bold = true;
					title.Style.Font.Size = 20;
					title.Style.Fill.PatternType = ExcelFillStyle.Solid;
					title.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 51, 102));
					title.Style.Font.Color.SetColor(Color.White);
					title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					title.Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(0, 51, 102));
				}
				workSheet.Row(1).Height = 30;

				DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
				using (ExcelRange timestamp = workSheet.Cells[2, 1, 2, 25])
				{
					timestamp.Merge = true;
					timestamp.Value = $"Generated: {localDate:yyyy-MM-dd HH:mm}";
					timestamp.Style.Font.Name = "Arial";
					timestamp.Style.Font.Bold = true;
					timestamp.Style.Font.Size = 12;
					timestamp.Style.Fill.PatternType = ExcelFillStyle.Solid;
					timestamp.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(230, 230, 230));
					timestamp.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					timestamp.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(150, 150, 150));
				}
				workSheet.Row(2).Height = 20;

				var headers = new List<(string Name, bool Include, Color Color)>();
				if (options.IncludeSalesOrderNumber) headers.Add(("Sales Order", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludeSalesOrderDate) headers.Add(("Order Date", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludeCustomerName) headers.Add(("Customer", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludeMachineDescriptions) headers.Add(("Model", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludeSerialNumbers) headers.Add(("Serial #", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludeProductionOrderNumbers) headers.Add(("Prod Order", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludePackageReleaseDateE) headers.Add(("Pkg Rel Exp", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludePackageReleaseDateA) headers.Add(("Pkg Rel Act", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludeVendorNames) headers.Add(("Vendors", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludePoNumbers) headers.Add(("PO #", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludePoDueDates) headers.Add(("PO Due", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludeMedia) headers.Add(("Media", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludeSpareParts) headers.Add(("Spares", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludeBase) headers.Add(("Base", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludeAirSeal) headers.Add(("Air Seal", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludeCoatingLining) headers.Add(("Coating", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludeDisassembly) headers.Add(("Disasm", true, Color.FromArgb(173, 216, 230)));
				if (options.IncludeNotes) headers.Add(("Comments", true, Color.FromArgb(173, 216, 230)));

				int colIndex = 1;
				foreach (var header in headers)
				{
					var cell = workSheet.Cells[3, colIndex];
					cell.Value = header.Name;
					cell.Style.Font.Name = "Arial";
					cell.Style.Font.Bold = true;
					cell.Style.Font.Size = 11;
					cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
					cell.Style.Fill.BackgroundColor.SetColor(header.Color);
					cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
					cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					colIndex++;
				}

				var noteHeaders = new List<(string Name, bool Include, Color Color)>
=======
			}

			colIndex = 1;
			if (options.IncludeOrderNumber) workSheet.Column(colIndex++).Width = 15;
			if (options.IncludeSalesOrderDate) workSheet.Column(colIndex++).Width = 12;
			if (options.IncludeCustomerName) { workSheet.Column(colIndex).Width = 25; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludeMachineModel) { workSheet.Column(colIndex).Width = 20; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludeSerialNumbers) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludeProductionOrderNumbers) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludePackageReleaseDateE) workSheet.Column(colIndex++).Width = 15;
			if (options.IncludePackageReleaseDateA) workSheet.Column(colIndex++).Width = 15;
			if (options.IncludeVendorNames) { workSheet.Column(colIndex).Width = 25; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludePoNumbers) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludePoDueDates) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludeMedia) workSheet.Column(colIndex++).Width = 10;
			if (options.IncludeSpareParts) workSheet.Column(colIndex++).Width = 10;
			if (options.IncludeBase) workSheet.Column(colIndex++).Width = 10;
			if (options.IncludeAirSeal) workSheet.Column(colIndex++).Width = 10;
			if (options.IncludeCoatingLining) workSheet.Column(colIndex++).Width = 10;
			if (options.IncludeDisassembly) workSheet.Column(colIndex++).Width = 10;
			if (options.IncludeNotes) { workSheet.Column(colIndex).Width = 30; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludePreOrder) { workSheet.Column(colIndex).Width = 20; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludeScope) { workSheet.Column(colIndex).Width = 20; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludeActualAssemblyHours) { workSheet.Column(colIndex).Width = 12; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludeReworkHours) { workSheet.Column(colIndex).Width = 12; workSheet.Column(colIndex++).Style.WrapText = true; }
			if (options.IncludeNamePlate) { workSheet.Column(colIndex).Width = 15; workSheet.Column(colIndex++).Style.WrapText = true; }

			workSheet.View.FreezePanes(5, 1);
		}
		private void SetupGanttSchedulesWorksheet(ExcelWorksheet workSheet, List<GanttScheduleViewModel> schedules, ScheduleExportOptionsViewModel options)
>>>>>>> b998c9d23189efe44dc7bb325dd11c8609ac17c9
		{
			("PreOrder", options.IncludePreOrder, Color.FromArgb(255, 218, 185)),
			("Scope", options.IncludeScope, Color.FromArgb(240, 230, 140)),
			("Act Hrs", options.IncludeActualAssemblyHours, Color.FromArgb(144, 238, 144)),
			("Rework Hrs", options.IncludeReworkHours, Color.FromArgb(173, 216, 230)),
			("NamePlate", options.IncludeNamePlate, Color.FromArgb(221, 160, 221))
		};
				var includedNoteHeaders = noteHeaders.Where(nh => nh.Include).ToList();

				if (includedNoteHeaders.Any())
				{
					int notesStartCol = colIndex;
					int notesEndCol = notesStartCol + includedNoteHeaders.Count - 1;

<<<<<<< HEAD
					using (var notesHeaderRange = workSheet.Cells[3, notesStartCol, 3, notesEndCol])
					{
						notesHeaderRange.Merge = true;
						notesHeaderRange.Value = "Additional Notes";
						notesHeaderRange.Style.Font.Name = "Arial";
						notesHeaderRange.Style.Font.Bold = true;
						notesHeaderRange.Style.Font.Size = 11;
						notesHeaderRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
						notesHeaderRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(135, 206, 235));
						notesHeaderRange.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
						notesHeaderRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					}

					int subCol = notesStartCol;
					foreach (var nh in includedNoteHeaders)
					{
						var cell = workSheet.Cells[4, subCol];
						cell.Value = nh.Name;
						cell.Style.Font.Name = "Arial";
						cell.Style.Font.Bold = true;
						cell.Style.Font.Size = 10;
						cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
						cell.Style.Fill.BackgroundColor.SetColor(nh.Color);
						cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(100, 100, 100));
						cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
						subCol++;
					}
					colIndex = notesEndCol + 1;
				}

				int row = 5;
				foreach (var schedule in schedules)
				{
					colIndex = 1;
					if (options.IncludeSalesOrderNumber) workSheet.Cells[row, colIndex++].Value = schedule.SalesOrderNumber ?? "";
					if (options.IncludeSalesOrderDate) workSheet.Cells[row, colIndex++].Value = schedule.SalesOrderDate ?? "";
					if (options.IncludeCustomerName) workSheet.Cells[row, colIndex++].Value = schedule.CustomerName ?? "";
					if (options.IncludeMachineDescriptions) workSheet.Cells[row, colIndex++].Value = schedule.MachineDescriptions ?? "";
					if (options.IncludeSerialNumbers) workSheet.Cells[row, colIndex++].Value = schedule.SerialNumbers ?? "";
					if (options.IncludeProductionOrderNumbers) workSheet.Cells[row, colIndex++].Value = schedule.ProductionOrderNumbers ?? "";
					if (options.IncludePackageReleaseDateE) workSheet.Cells[row, colIndex++].Value = schedule.PackageReleaseDateE ?? "";
					if (options.IncludePackageReleaseDateA) workSheet.Cells[row, colIndex++].Value = schedule.PackageReleaseDateA ?? "";
					if (options.IncludeVendorNames) workSheet.Cells[row, colIndex++].Value = schedule.VendorNames ?? "";
					if (options.IncludePoNumbers) workSheet.Cells[row, colIndex++].Value = schedule.PoNumbers ?? "";
					if (options.IncludePoDueDates) workSheet.Cells[row, colIndex++].Value = schedule.PoDueDates ?? "";
					if (options.IncludeMedia) workSheet.Cells[row, colIndex++].Value = schedule.Media ?? "";
					if (options.IncludeSpareParts) workSheet.Cells[row, colIndex++].Value = schedule.SpareParts ?? "";
					if (options.IncludeBase) workSheet.Cells[row, colIndex++].Value = schedule.Base ?? "";
					if (options.IncludeAirSeal) workSheet.Cells[row, colIndex++].Value = schedule.AirSeal ?? "";
					if (options.IncludeCoatingLining) workSheet.Cells[row, colIndex++].Value = schedule.CoatingLining ?? "";
					if (options.IncludeDisassembly) workSheet.Cells[row, colIndex++].Value = schedule.Disassembly ?? "";
					if (options.IncludeNotes) workSheet.Cells[row, colIndex++].Value = schedule.Comments ?? "";
					if (options.IncludePreOrder) workSheet.Cells[row, colIndex++].Value = schedule.PreOrder ?? "";
					if (options.IncludeScope) workSheet.Cells[row, colIndex++].Value = schedule.Scope ?? "";
					if (options.IncludeActualAssemblyHours) workSheet.Cells[row, colIndex++].Value = schedule.ActualAssemblyHours ?? "";
					if (options.IncludeReworkHours) workSheet.Cells[row, colIndex++].Value = schedule.ReworkHours ?? "";
					if (options.IncludeNamePlate) workSheet.Cells[row, colIndex++].Value = schedule.NamePlate ?? "";
					row++;
				}

				if (row > 5)
				{
					using (var range = workSheet.Cells[5, 1, row - 1, colIndex - 1])
					{
						range.Style.Font.Name = "Arial";
						range.Style.Font.Size = 10;
						range.Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(0, 51, 102));
						range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
						range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
						range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
						range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
						range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
					}

					for (int i = 5; i < row; i++)
					{
						using (var rowRange = workSheet.Cells[i, 1, i, colIndex - 1])
						{
							rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
							rowRange.Style.Fill.BackgroundColor.SetColor(i % 2 == 0 ? Color.FromArgb(245, 245, 245) : Color.White);
						}
					}

               
				}

				colIndex = 1;
				if (options.IncludeSalesOrderNumber) colIndex++;
				if (options.IncludeSalesOrderDate) colIndex++;
				if (options.IncludeCustomerName) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;
				if (options.IncludeMachineDescriptions) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;
				if (options.IncludeSerialNumbers) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;
				if (options.IncludeProductionOrderNumbers) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;
				if (options.IncludePackageReleaseDateE) colIndex++;
				if (options.IncludePackageReleaseDateA) colIndex++;
				if (options.IncludeVendorNames) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;
				if (options.IncludePoNumbers) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;
				if (options.IncludePoDueDates) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;
				if (options.IncludeMedia) colIndex++;
				if (options.IncludeSpareParts) colIndex++;
				if (options.IncludeBase) colIndex++;
				if (options.IncludeAirSeal) colIndex++;
				if (options.IncludeCoatingLining) colIndex++;
				if (options.IncludeDisassembly) colIndex++;
				if (options.IncludeNotes) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;
				if (options.IncludePreOrder) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;
				if (options.IncludeScope) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;
				if (options.IncludeActualAssemblyHours) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;
				if (options.IncludeReworkHours) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;
				if (options.IncludeNamePlate) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;

				workSheet.Cells.AutoFitColumns();
				colIndex = 1;
				if (options.IncludeSalesOrderNumber) workSheet.Column(colIndex++).Width = 15; else colIndex++;
				if (options.IncludeSalesOrderDate) workSheet.Column(colIndex++).Width = 12; else colIndex++;
				if (options.IncludeCustomerName) workSheet.Column(colIndex++).Width = 25; else colIndex++;
				if (options.IncludeMachineDescriptions) workSheet.Column(colIndex++).Width = 20; else colIndex++;
				if (options.IncludeSerialNumbers) workSheet.Column(colIndex++).Width = 15; else colIndex++;
				if (options.IncludeProductionOrderNumbers) workSheet.Column(colIndex++).Width = 15; else colIndex++;
				if (options.IncludePackageReleaseDateE) workSheet.Column(colIndex++).Width = 15; else colIndex++;
				if (options.IncludePackageReleaseDateA) workSheet.Column(colIndex++).Width = 15; else colIndex++;
				if (options.IncludeVendorNames) workSheet.Column(colIndex++).Width = 25; else colIndex++;
				if (options.IncludePoNumbers) workSheet.Column(colIndex++).Width = 15; else colIndex++;
				if (options.IncludePoDueDates) workSheet.Column(colIndex++).Width = 15; else colIndex++;
				if (options.IncludeMedia) workSheet.Column(colIndex++).Width = 10; else colIndex++;
				if (options.IncludeSpareParts) workSheet.Column(colIndex++).Width = 10; else colIndex++;
				if (options.IncludeBase) workSheet.Column(colIndex++).Width = 10; else colIndex++;
				if (options.IncludeAirSeal) workSheet.Column(colIndex++).Width = 10; else colIndex++;
				if (options.IncludeCoatingLining) workSheet.Column(colIndex++).Width = 10; else colIndex++;
				if (options.IncludeDisassembly) workSheet.Column(colIndex++).Width = 10; else colIndex++;
				if (options.IncludeNotes) workSheet.Column(colIndex++).Width = 30; else colIndex++;
				if (options.IncludePreOrder) workSheet.Column(colIndex++).Width = 20; else colIndex++;
				if (options.IncludeScope) workSheet.Column(colIndex++).Width = 20; else colIndex++;
				if (options.IncludeActualAssemblyHours) workSheet.Column(colIndex++).Width = 12; else colIndex++;
				if (options.IncludeReworkHours) workSheet.Column(colIndex++).Width = 12; else colIndex++;
				if (options.IncludeNamePlate) workSheet.Column(colIndex++).Width = 15; else colIndex++;
=======
			// Dynamic Headers
			var headers = new List<(string Name, bool Include, Color Color)>
	{
		("Order #", options.IncludeOrderNumber, Color.FromArgb(91, 155, 213)),
		("ENG.", options.IncludeEngineer, Color.FromArgb(91, 155, 213)),
		("Customer", options.IncludeCustomerName, Color.FromArgb(91, 155, 213)),
		("Machine Model", options.IncludeMachineModel, Color.FromArgb(91, 155, 213)),
		("QTY", options.IncludeQuantity, Color.FromArgb(91, 155, 213)),
		("Media", options.IncludeMedia, Color.FromArgb(91, 155, 213)),
		("Spare Parts", options.IncludeSpareParts, Color.FromArgb(91, 155, 213)),
		("App Dwg Rec'd", options.IncludeApprovedDrawingReceived, Color.FromArgb(91, 155, 213))
	};

			int colIndex = 1;
			foreach (var header in headers.Where(h => h.Include))
			{
				workSheet.Cells[2, colIndex].Value = header.Name;
				workSheet.Cells[2, colIndex].Style.Font.Name = "Calibri";
				workSheet.Cells[2, colIndex].Style.Font.Size = 11;
				workSheet.Cells[2, colIndex].Style.Font.Bold = true;
				workSheet.Cells[2, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
				workSheet.Cells[2, colIndex].Style.Fill.BackgroundColor.SetColor(header.Color);
				workSheet.Cells[2, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				colIndex++;
			}

			int staticCols = headers.Count(h => h.Include);

			// Add week numbers and dates for Gantt data
			int totalWeeks = 0;
			if (options.IncludeGanttData)
			{
				// Dynamic Start Date: Start from current month
				DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
				totalWeeks = GetWeeksRemainingUntilEndOfYear(startDate);

				// Generate week columns dynamically from current month to end of year
				for (int week = 1; week <= totalWeeks; week++)
				{
					int weekNumber = GetWeekOfYear(startDate, WeekStartOption.Monday);

					// Week number header
					workSheet.Cells[2, staticCols + week].Value = $"W{weekNumber}";
					workSheet.Cells[2, staticCols + week].Style.Font.Name = "Calibri";
					workSheet.Cells[2, staticCols + week].Style.Font.Size = 10;
					workSheet.Cells[2, staticCols + week].Style.Font.Bold = true;
					workSheet.Cells[2, staticCols + week].Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Cells[2, staticCols + week].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
					workSheet.Cells[2, staticCols + week].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

					// Date header
					workSheet.Cells[3, staticCols + week].Value = startDate.ToString("MMM d");
					workSheet.Cells[3, staticCols + week].Style.Font.Name = "Calibri";
					workSheet.Cells[3, staticCols + week].Style.Font.Size = 8;
					workSheet.Cells[3, staticCols + week].Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Cells[3, staticCols + week].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
					workSheet.Cells[3, staticCols + week].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

					startDate = startDate.AddDays(7); // Move to next week
				}
			}

			// Add Special Notes header if needed
			int specialNotesCol = 0;
			if (options.IncludeSpecialNotes)
			{
				specialNotesCol = staticCols + totalWeeks + 1;
				workSheet.Cells[2, specialNotesCol].Value = "Notes";
				workSheet.Cells[2, specialNotesCol].Style.Font.Name = "Calibri";
				workSheet.Cells[2, specialNotesCol].Style.Font.Size = 11;
				workSheet.Cells[2, specialNotesCol].Style.Font.Bold = true;
				workSheet.Cells[2, specialNotesCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
				workSheet.Cells[2, specialNotesCol].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(91, 155, 213));
				workSheet.Cells[2, specialNotesCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
>>>>>>> b998c9d23189efe44dc7bb325dd11c8609ac17c9

				workSheet.View.FreezePanes(5, 1);
			}

<<<<<<< HEAD
			private void SetupGanttSchedulesWorksheet(ExcelWorksheet workSheet, List<GanttScheduleViewModel> schedules, ScheduleExportOptionsViewModel options)
			{
				if (workSheet == null) throw new ArgumentNullException(nameof(workSheet));
				if (schedules == null) throw new ArgumentNullException(nameof(schedules));
				if (options == null) throw new ArgumentNullException(nameof(options));

				// Title
				workSheet.Cells[1, 1].Value = "Machinery Gantt Schedule";
				using (ExcelRange title = workSheet.Cells[1, 1, 1, 52])
				{
					title.Merge = true;
					title.Style.Font.Name = "Calibri";
					title.Style.Font.Size = 20;
					title.Style.Font.Bold = true;
					title.Style.Fill.PatternType = ExcelFillStyle.Solid;
					title.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 32, 96));
					title.Style.Font.Color.SetColor(Color.White);
					title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					title.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
				}
				workSheet.Row(1).Height = 30;

				// Dynamic Headers
				var headers = new List<(string Name, bool Include, Color Color)>();
				int colIndex = 1;
				if (options.IncludeOrderNumber) headers.Add(("OR #", true, Color.FromArgb(91, 155, 213)));
				if (options.IncludeEngineer) headers.Add(("ENG.", true, Color.FromArgb(91, 155, 213)));
				if (options.IncludeGanttCustomerName) headers.Add(("Customer", true, Color.FromArgb(91, 155, 213)));
				if (options.IncludeMachineModel) headers.Add(("Machine Model", true, Color.FromArgb(91, 155, 213)));
				if (options.IncludeQuantity) headers.Add(("QTY", true, Color.FromArgb(91, 155, 213)));
				if (options.IncludeGanttMedia) headers.Add(("Media", true, Color.FromArgb(91, 155, 213)));
				if (options.IncludeGanttSpareParts) headers.Add(("Spare Parts", true, Color.FromArgb(91, 155, 213)));
				if (options.IncludeApprovedDrawingReceived) headers.Add(("App Dwg Rec'd", true, Color.FromArgb(91, 155, 213)));

				int staticCols = headers.Count;

				// Add week numbers and dates for Gantt data
				if (options.IncludeGanttData)
				{
					// Dynamic Start Date: Start from current month
					DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
					int totalWeeks = GetWeeksRemainingUntilEndOfYear(startDate);

					// Generate week columns dynamically from current month to end of year
					for (int week = 1; week <= totalWeeks; week++)
					{
						int weekNumber = GetWeekOfYear(startDate, WeekStartOption.Monday);

						// Week number header
						workSheet.Cells[2, staticCols + week].Value = $"W{weekNumber}";
						workSheet.Cells[2, staticCols + week].Style.Font.Name = "Calibri";
						workSheet.Cells[2, staticCols + week].Style.Font.Size = 10;
						workSheet.Cells[2, staticCols + week].Style.Font.Bold = true;
						workSheet.Cells[2, staticCols + week].Style.Fill.PatternType = ExcelFillStyle.Solid;
						workSheet.Cells[2, staticCols + week].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
						workSheet.Cells[2, staticCols + week].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

						// Date header
						workSheet.Cells[3, staticCols + week].Value = startDate.ToString("MMM d");
						workSheet.Cells[3, staticCols + week].Style.Font.Name = "Calibri";
						workSheet.Cells[3, staticCols + week].Style.Font.Size = 8;
						workSheet.Cells[3, staticCols + week].Style.Fill.PatternType = ExcelFillStyle.Solid;
						workSheet.Cells[3, staticCols + week].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
						workSheet.Cells[3, staticCols + week].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

						startDate = startDate.AddDays(7); // Move to next week
					}
				}

				// Add Special Notes header if needed
				if (options.IncludeSpecialNotes)
				{
					int specialNotesCol = staticCols + 44 ;
					workSheet.Cells[2, specialNotesCol].Value = "Notes";
					workSheet.Cells[2, specialNotesCol].Style.Font.Name = "Calibri";
					workSheet.Cells[2, specialNotesCol].Style.Font.Size = 11;
					workSheet.Cells[2, specialNotesCol].Style.Font.Bold = true;
					workSheet.Cells[2, specialNotesCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Cells[2, specialNotesCol].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(91, 155, 213));
					workSheet.Cells[2, specialNotesCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

					// Empty cell for alignment
					workSheet.Cells[3, specialNotesCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Cells[3, specialNotesCol].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(91, 155, 213));
				}

				// Add static headers in row 2
				colIndex = 1;
				foreach (var header in headers)
				{
					workSheet.Cells[2, colIndex].Value = header.Name;
					workSheet.Cells[2, colIndex].Style.Font.Name = "Calibri";
					workSheet.Cells[2, colIndex].Style.Font.Size = 11;
					workSheet.Cells[2, colIndex].Style.Font.Bold = true;
					workSheet.Cells[2, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Cells[2, colIndex].Style.Fill.BackgroundColor.SetColor(header.Color);
					workSheet.Cells[2, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					colIndex++;
				}

				// Empty cells in row 3 for static columns
				for (int i = 1; i <= staticCols; i++)
				{
					workSheet.Cells[3, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Cells[3, i].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(91, 155, 213));
				}

				// Data rows
				int row = 4;
				foreach (var schedule in schedules)
				{
					if (schedule?.GanttData == null || !schedule.GanttData.Any()) continue;

                

						colIndex = 1;
						if (options.IncludeOrderNumber) workSheet.Cells[row, colIndex++].Value = schedule.OrderNumber;
						if (options.IncludeEngineer) workSheet.Cells[row, colIndex++].Value = schedule.Engineer;
						if (options.IncludeGanttCustomerName) workSheet.Cells[row, colIndex++].Value = schedule.CustomerName;
						if (options.IncludeMachineModel) workSheet.Cells[row, colIndex++].Value = schedule.MachineModel;
						if (options.IncludeQuantity) workSheet.Cells[row, colIndex++].Value = schedule.Quantity;
						if (options.IncludeGanttMedia) workSheet.Cells[row, colIndex++].Value = schedule.Media;
						if (options.IncludeGanttSpareParts) workSheet.Cells[row, colIndex++].Value = schedule.SpareParts;
						if (options.IncludeApprovedDrawingReceived)
							workSheet.Cells[row, colIndex++].Value = schedule.ApprovedDrawingReceived.ToString("MMM-dd-yyyy");

						if (options.IncludeGanttData && schedule.GanttData != null)
						{

							ApplyAllMilestonesToGantt(workSheet, row, staticCols, schedule.GanttData, options);
						}

						// Special Notes
						if (options.IncludeSpecialNotes)
						{
							workSheet.Cells[row, staticCols + 44].Value = schedule.SpecialNotes;
							workSheet.Cells[row, staticCols + 44].Style.WrapText = true;
							workSheet.Cells[row, staticCols + 44].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
						}

						row++;
					}
			

				// Styling for all data cells
				int lastCol = staticCols + (options.IncludeGanttData ? 44 : 0) + (options.IncludeSpecialNotes ? 1 : 0);
				if (row > 4)
=======
			// Empty cells in row 3 for static columns
			for (int i = 1; i <= staticCols; i++)
			{
				workSheet.Cells[3, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
				workSheet.Cells[3, i].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(91, 155, 213));
			}

			// Data rows
			int row = 4;
			foreach (var schedule in schedules)
			{
				if (schedule?.GanttData == null || !schedule.GanttData.Any()) continue;

				colIndex = 1;
				if (options.IncludeOrderNumber) workSheet.Cells[row, colIndex++].Value = schedule.OrderNumber;
				if (options.IncludeEngineer) workSheet.Cells[row, colIndex++].Value = schedule.Engineer;
				if (options.IncludeCustomerName) workSheet.Cells[row, colIndex++].Value = schedule.CustomerName;
				if (options.IncludeMachineModel) workSheet.Cells[row, colIndex++].Value = schedule.MachineModel;
				if (options.IncludeQuantity) workSheet.Cells[row, colIndex++].Value = schedule.Quantity;
				if (options.IncludeMedia) workSheet.Cells[row, colIndex++].Value = schedule.Media;
				if (options.IncludeSpareParts) workSheet.Cells[row, colIndex++].Value = schedule.SpareParts;
				if (options.IncludeApprovedDrawingReceived)
				{
					workSheet.Cells[row, colIndex++].Value = schedule.ApprovedDrawingReceived != DateTime.MinValue
						? schedule.ApprovedDrawingReceived.ToString("MMM-dd-yyyy")
						: "";
				}

				if (options.IncludeGanttData && schedule.GanttData != null)
				{
					ApplyAllMilestonesToGantt(workSheet, row, staticCols, schedule.GanttData, options);
				}

				// Special Notes
				if (options.IncludeSpecialNotes && specialNotesCol > 0)
				{
					workSheet.Cells[row, specialNotesCol].Value = schedule.SpecialNotes;
					workSheet.Cells[row, specialNotesCol].Style.WrapText = true;
					workSheet.Cells[row, specialNotesCol].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
				}

				row++;
			}

			// Styling for all data cells
			int lastCol = staticCols + (options.IncludeGanttData ? totalWeeks : 0) + (options.IncludeSpecialNotes ? 1 : 0);
			if (row > 4)
			{
				using (var dataRange = workSheet.Cells[4, 1, row - 1, lastCol])
>>>>>>> b998c9d23189efe44dc7bb325dd11c8609ac17c9
				{
					using (var dataRange = workSheet.Cells[4, 1, row - 1, lastCol])
					{
						dataRange.Style.Font.Name = "Calibri";
						dataRange.Style.Font.Size = 10;
						dataRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
						dataRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					}

					// Alternating row colors for static columns
					for (int i = 4; i < row; i++)
					{
						using (var rowRange = workSheet.Cells[i, 1, i, staticCols])
						{
							rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
							rowRange.Style.Fill.BackgroundColor.SetColor(i % 2 == 0 ? Color.FromArgb(242, 242, 242) : Color.White);
						}
					}

					for (int i = 4; i < row; i++)
					{
						workSheet.Row(i).Height = 25;
					}
				}

<<<<<<< HEAD
				// Column Widths
				colIndex = 1;
				if (options.IncludeOrderNumber) workSheet.Column(colIndex++).Width = 10;
				if (options.IncludeEngineer) workSheet.Column(colIndex++).Width = 6;
				if (options.IncludeGanttCustomerName) workSheet.Column(colIndex++).Width = 20;
				if (options.IncludeMachineModel) workSheet.Column(colIndex++).Width = 24;
				if (options.IncludeQuantity) workSheet.Column(colIndex++).Width = 5;
				if (options.IncludeGanttMedia) workSheet.Column(colIndex++).Width = 8;
				if (options.IncludeGanttSpareParts) workSheet.Column(colIndex++).Width = 12;
				if (options.IncludeApprovedDrawingReceived) workSheet.Column(colIndex++).Width = 19;

				if (options.IncludeGanttData)
				{
					for (int i = staticCols + 1; i <= staticCols + 33; i++)
						workSheet.Column(i).Width = 5.5; // Narrower columns for cleaner Gantt chart

               
				}


				if (options.IncludeSpecialNotes)
=======
				for (int i = 4; i < row; i++)
				{
					workSheet.Row(i).Height = 25;
				}
			}

			// Column Widths
			colIndex = 1;
			if (options.IncludeOrderNumber) workSheet.Column(colIndex++).Width = 10;
			if (options.IncludeEngineer) workSheet.Column(colIndex++).Width = 6;
			if (options.IncludeCustomerName) workSheet.Column(colIndex++).Width = 20;
			if (options.IncludeMachineModel) workSheet.Column(colIndex++).Width = 24;
			if (options.IncludeQuantity) workSheet.Column(colIndex++).Width = 5;
			if (options.IncludeMedia) workSheet.Column(colIndex++).Width = 8;
			if (options.IncludeSpareParts) workSheet.Column(colIndex++).Width = 12;
			if (options.IncludeApprovedDrawingReceived) workSheet.Column(colIndex++).Width = 19;

			if (options.IncludeGanttData)
			{
				for (int i = staticCols + 1; i <= staticCols + totalWeeks; i++)
					workSheet.Column(i).Width = 5.5; // Narrower columns for cleaner Gantt chart
			}

			if (options.IncludeSpecialNotes && specialNotesCol > 0)
			{
				workSheet.Column(specialNotesCol).Width = 40;
				workSheet.Column(specialNotesCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
				workSheet.Column(specialNotesCol).Style.WrapText = true;
			}

			// Freeze headers and static columns
			workSheet.View.FreezePanes(4, staticCols + 1);

			// Add thin borders to all cells for better readability
			if (row > 4)
			{
				using (var allCells = workSheet.Cells[1, 1, row - 1, lastCol])
>>>>>>> b998c9d23189efe44dc7bb325dd11c8609ac17c9
				{
					workSheet.Column(staticCols + 44).Width = 40;
					workSheet.Column(staticCols + 44).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
					workSheet.Column(staticCols + 44).Style.WrapText = true;
				}

				// Freeze headers and static columns
				workSheet.View.FreezePanes(4, staticCols + 1);

				// Add thin borders to all cells for better readability
				if (row > 1)
				{
					using (var allCells = workSheet.Cells[1, 1, row - 1, lastCol])
					{
						allCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
						allCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
						allCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
						allCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
					}
				}
			}
<<<<<<< HEAD

			private Color GetMilestoneColor(string hexCode)
			{
				if (string.IsNullOrWhiteSpace(hexCode))
					return Color.FromArgb(200, 200, 200); // Default gray

				try
				{
					hexCode = hexCode.Trim().TrimStart('#');

					if (hexCode.Length == 3)
					{
						// Expand shorthand (e.g., "03F" to "0033FF")
						hexCode = $"{hexCode[0]}{hexCode[0]}{hexCode[1]}{hexCode[1]}{hexCode[2]}{hexCode[2]}";
					}

					if (hexCode.Length == 6)
					{
						int r = Convert.ToInt32(hexCode.Substring(0, 2), 16);
						int g = Convert.ToInt32(hexCode.Substring(2, 2), 16);
						int b = Convert.ToInt32(hexCode.Substring(4, 2), 16);
						return Color.FromArgb(r, g, b);
					}
				}
				catch (Exception)
				{
					// Log exception in production code if needed
				}

=======
		}
		private Color GetMilestoneColor(string hexCode)
		{
			if (string.IsNullOrWhiteSpace(hexCode))
>>>>>>> b998c9d23189efe44dc7bb325dd11c8609ac17c9
				return Color.FromArgb(200, 200, 200); // Default gray
			}

			private Color GetContrastColor(Color backgroundColor)
			{
				// Calculate luminance to determine if background is dark
				double luminance = (0.299 * backgroundColor.R + 0.587 * backgroundColor.G + 0.114 * backgroundColor.B) / 255;
				return luminance > 0.5 ? Color.Black : Color.White;
			}
			private void SetupHeader(ExcelWorksheet workSheet, int row, int col, string value, Color bgColor)
			{
				workSheet.Cells[row, col].Value = value;
				workSheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
				workSheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(bgColor);
				workSheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
			}
			private string GetGanttWeekValue(GanttScheduleViewModel gantt, DateTime weekStart)
			{
				if (gantt?.GanttData == null) return "";
				// Logic to determine if this week has a milestone (simplified here)
				return gantt.GanttData.Any(g => g.StartDate <= weekStart && g.EndDate >= weekStart) ? "X" : "";
			}

			// Placeholder for GetWeeksRemainingUntilEndOfYear and GetWeekOfYear (assumed to exist)
			private int GetWeeksRemainingUntilEndOfYear(DateTime startDate) => 52 - GetWeekOfYear(startDate, WeekStartOption.Monday) + 1;
			private int GetWeekOfYear(DateTime date, WeekStartOption option) => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
			private enum WeekStartOption { Monday }

			private static int GetColumnFromWeek(int week)
			{
				if (week >= 1 && week <= 33) // Weeks 1-33 of 2025
					return 11 + (week - 1);  // Columns 11-43
				return -1;
			}



			private void ApplyMilestoneToGantt(ExcelWorksheet workSheet, int row, int staticCols, GanttViewModel milestone, ScheduleExportOptionsViewModel options)
			{
				if (milestone == null || milestone.StartDate == null || milestone.EndDate == null)
					return; // Skip if milestone data is incomplete

				// Get the starting and ending columns for the milestone based on dates
				int startCol = GetColumnFromWeekWithDate((DateTime)milestone.StartDate);
				int endCol = GetColumnFromWeekWithDate((DateTime)milestone.EndDate);

				// Validate columns to ensure they stay within the valid week range
				startCol = startCol > 0 && startCol <= 33 ? staticCols + startCol : staticCols + 1;
				endCol = endCol > 0 && endCol <= 33 ? staticCols + endCol : staticCols + 33;

				// Get the milestone color based on defined mapping
				Color milestoneColor = GetMilestoneColor(milestone.MilestoneClass);

				// Apply milestone color across the range of weeks
				for (int col = startCol; col <= endCol; col++)
				{
					var cell = workSheet.Cells[row, col];
					cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
					cell.Style.Fill.BackgroundColor.SetColor(milestoneColor);

					// Add comments with milestone name for reference
					if (col == startCol)
					{
						cell.AddComment(milestone.MachineName, "Milestone");
					}
				}
			}

			private void ApplyAllMilestonesToGantt(ExcelWorksheet workSheet, int row, int staticCols, List<GanttViewModel> milestones, ScheduleExportOptionsViewModel options)
			{
				if (milestones == null || !milestones.Any())
					return;

				// Loop through all milestones and apply them in the same row
				foreach (var milestone in milestones)
				{
					if (milestone == null || milestone.StartDate == null || milestone.EndDate == null)
						continue;

					// Get start and end columns for the milestone
					int startCol = GetColumnFromWeekWithDate((DateTime)milestone.StartDate);
					int endCol = GetColumnFromWeekWithDate((DateTime)milestone.EndDate);

					// Validate column boundaries
					startCol = startCol > 0 && startCol <= 33 ? staticCols + startCol : staticCols + 1;
					endCol = endCol > 0 && endCol <= 33 ? staticCols + endCol : staticCols + 33;

					// Get milestone color
					Color milestoneColor = GetMilestoneColor(milestone.MilestoneClass);

					// Apply color and add milestone name if it's the start of a milestone
					for (int col = startCol; col <= endCol; col++)
					{
						var cell = workSheet.Cells[row, col];
						cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
						cell.Style.Fill.BackgroundColor.SetColor(milestoneColor);

						// Add milestone label at start column for better visibility
						if (col == startCol)
						{
							//cell.Value = milestone.MachineName;
							cell.Style.Font.Bold = true;
							cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
						}
					}
				}
			}

			private int GetColumnFromWeekWithDate(DateTime date)
			{
				// Dynamic start date based on current month (fix)
				DateTime baseDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Start from the current month
				int weekNum = GetWeekOfYear(date, WeekStartOption.Monday);

				// Calculate the difference in weeks between baseDate and milestone date
				int weekDiff = (int)((date - baseDate).TotalDays / 7);

				// Add 1 to shift to the correct week column
				int column = weekDiff + 1;

				// Ensure column stays within the 33-week limit (for year-end)
				return column >= 1 && column <= 33 ? column : -1;
			}





			private int AdjustWeekNumber(int week, int year, int baseYear)
			{
				if (year < baseYear)
				{
					return week + 52; // Move previous year weeks to end
				}
				else if (year > baseYear)
				{
					return week; // Weeks 1-7 of next year stay as-is
				}
				else
				{
					return week >= 27 ? week : week + 52; // Current year: weeks < 27 wrap to next year
				}
			}




        private List<GanttViewModel> GetMilestoneTasks(GanttData g)
        {
            var tasks = new List<GanttViewModel>();
            if (g.Machine == null) return tasks;

            var milestones = new List<(DateTime? Start, DateTime? End, string Name, string Color)>
    {
        (g.EngExpected, g.EngReleased, "Engineering Released to Customer", "#FABF8F"),
        (g.AppDExp, g.AppDRcd, "Customer Approval Received", "#E26B0A"),
        (g.EngReleased, g.EngReleased, "Package Released to PIC/Spare Parts to Customer Service", "#ADF802"),
        (g.PurchaseOrdersIssued, g.PurchaseOrdersCompleted, "Purchase Orders Issued", "#8DB4E2"),
        (g.SupplierPODue, g.SupplierPODue, "Supplier Purchase Orders Due", "#FF99CC"),
        (g.AssemblyStart, g.AssemblyComplete, "Machine Assembly and Testing", "#00B050")
    };

            foreach (var (start, end, name, color) in milestones)
            {
                if (start.HasValue && end.HasValue)
                {
                    tasks.Add(new GanttViewModel
                    {
                        ID = g.ID,
                        UniqueID = $"{g.ID}-{g.Machine.ID}-{name}",
                        MachineName = $"{g.Machine.Description} - {name}",
                        SalesOrder = g.SalesOrder.OrderNumber,
                        Notes = g.Notes,
                        StartDate = start.Value,
                        EndDate = end.Value,
                        Progress = 100,
                        MilestoneClass = color


                    });
                    Console.WriteLine($"Milestone: {name}, Start: {start.Value}, End: {end.Value}, Color: {color}");
                }
            }

            return tasks;
        }

        //[HttpPost]
        //public async Task<IActionResult> UpdateTask([FromBody] GanttViewModel updatedTask)
        //{
        //    if (updatedTask == null || string.IsNullOrEmpty(updatedTask.UniqueID))
        //    {
        //        return Json(new { success = false, message = "Invalid task data." });
        //    }

        //    var existingTask = await _context.GanttDatas
        //        .FirstOrDefaultAsync(t => t.ID == updatedTask.UniqueID);

        //    if (existingTask == null)
        //    {
        //        return Json(new { success = false, message = "Task not found." });
        //    }

        //    // Update task properties
        //    existingTask.StartDate = updatedTask.StartDate;
        //    existingTask.EndDate = updatedTask.EndDate;
        //    existingTask.Progress = updatedTask.Progress;

        //    _context.GanttTasks.Update(existingTask);
        //    await _context.SaveChangesAsync();

        //    return Json(new { success = true, message = "Task updated successfully." });
        //}





        //private string GetMilestoneClass(GanttData g)
        //{
        //    if (g.EngReleased.HasValue) return "eng-released";
        //    if (g.PackageReleased.HasValue) return "package-released";
        //    if (g.ShipExpected.HasValue) return "shipping";
        //    if (g.DeliveryExpected.HasValue) return "delivery";
        //    return "default-task";
        //}

        [Authorize(Roles = "Admin,Engineering,Production,PIC")]
        public async Task<IActionResult> FinalizeGantt(int id)
        {
            var gantt = _context.GanttDatas.Find(id);
            if (gantt == null)
            {
                return NotFound();
            }

            gantt.IsFinalized = true;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Gantt chart finalized successfully!";
            return RedirectToAction("Index", new { isFinalized = true });
        }
        public async Task<IActionResult> UnfinalizeGantt(int id)
        {
            var gantt = _context.GanttDatas.Find(id);
            if (gantt == null)
            {
                return NotFound();
            }

            gantt.IsFinalized = false;
            _context.SaveChanges();

            return RedirectToAction("Index", new { isFinalized = false });
        }


        private bool GanttDataExists(int id)
        {
            return _context.GanttDatas.Any(e => e.ID == id);
        }
    }
}
