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
                                StartOfWeek = WeekStartOption.Monday
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
                    TempData["Message"] = "Gantt Data successfully updated.";
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

        [HttpGet]
		public IActionResult ExportSchedules()
		{
			var options = LoadOptionsFromSession() ?? new ScheduleExportOptionsViewModel();
			return View("ExportSchedules", options);
		}
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

        [HttpPost]
		public IActionResult DownloadSchedules(ScheduleExportOptionsViewModel options)
		{
			// Load previously saved options from session if this is not a form submission (i.e., options is default/empty)
			if (!HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) || options == null)
			{
				options = LoadOptionsFromSession() ?? new ScheduleExportOptionsViewModel();
			}

			// Fetch sales orders with related data (unchanged)
			var salesOrders = _context.SalesOrders
				.Include(so => so.Machines).ThenInclude(m => m.MachineType)
				.Include(so => so.Machines).ThenInclude(m => m.Procurements).ThenInclude(p => p.Vendor)
				.Include(so => so.SalesOrderEngineers).ThenInclude(se => se.Engineer)
				.OrderByDescending(so => so.SoDate)
				.AsNoTracking()
				.AsEnumerable();

			// Machine Schedules Data (unchanged)
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
						Media = options.IncludeMedia ? (m?.Media ?? false ? "✔" : "") : null,
						SpareParts = options.IncludeSpareParts ? (m?.SpareParts ?? false ? "✔" : "") : null,
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
				.Select(so => new GanttScheduleViewModel
				{
					OrderNumber = options.IncludeOrderNumber ? (so?.OrderNumber ?? "") : null,
					Engineer = options.IncludeEngineer && so?.SalesOrderEngineers?.FirstOrDefault()?.Engineer != null
						? $"{so.SalesOrderEngineers.FirstOrDefault().Engineer.FirstName?[0]} {so.SalesOrderEngineers.FirstOrDefault().Engineer.LastName?[0]}"
						: null,
					CustomerName = options.IncludeGanttCustomerName ? (so?.CompanyName ?? "Unknown") : null,
					Quantity = options.IncludeQuantity ? (so?.Machines?.Count() ?? 0) : 0,
					MachineModel = options.IncludeMachineModel && so?.Machines != null
						? string.Join("\n", so.Machines.Select(m => m?.MachineModel ?? "Unknown"))
						: null,
					Media = options.IncludeGanttMedia ? (so.Machines.Any(m => m.Media) ? "Yes" : "No") : null,
					SpareParts = options.IncludeGanttSpareParts ? (so.Machines.Any(m => m.SpareParts) ? "Yes" : "No") : null,
					ApprovedDrawingReceived = options.IncludeApprovedDrawingReceived ? (so?.AppDwgExp ?? DateTime.MinValue) : DateTime.MinValue,
					GanttData = options.IncludeGanttData && ganttDataLookup.ContainsKey(so?.ID ?? 0) ? ganttDataLookup[so.ID] : null,
					SpecialNotes = options.IncludeSpecialNotes && ganttDataLookup.ContainsKey(so?.ID ?? 0) && ganttDataLookup[so.ID].Any()
						? string.Join("; ", ganttDataLookup[so.ID].Where(g => !string.IsNullOrEmpty(g?.Notes)).Select(g => Regex.Replace(g.Notes, "<.*?>", string.Empty)).Distinct())
						: null
				})
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
				SaveOptionsToSession(options);
			}

			using (ExcelPackage excel = new ExcelPackage())
			{
				if (options.ReportType == ReportType.MachineSchedules || options.ReportType == ReportType.Both)
				{
					var workSheet = excel.Workbook.Worksheets.Add("Machine Schedules");
					SetupMachineSchedulesWorksheet(workSheet, machineSchedules, options);
				}

				if (options.ReportType == ReportType.GanttSchedules || options.ReportType == ReportType.Both)
				{
					var workSheet = excel.Workbook.Worksheets.Add("Gantt Schedules");
					SetupGanttSchedulesWorksheet(workSheet, ganttSchedules, options);
				}

				try
				{
					byte[] theData = excel.GetAsByteArray();
					string fileName = options.ReportType == ReportType.MachineSchedules ? "Machine Schedule.xlsx" :
									  options.ReportType == ReportType.GanttSchedules ? "Gantt Schedule.xlsx" :
									  "Combined Schedules.xlsx";
					return File(theData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
				}
				catch (Exception ex)
				{
					return BadRequest($"Could not build and download the file: {ex.Message}");
				}
			}
		}

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
			if (options.IncludeMedia) workSheet.Column(colIndex++).Width = 8; else colIndex++;
			if (options.IncludeSpareParts) workSheet.Column(colIndex++).Width = 8; else colIndex++;
			if (options.IncludeBase) workSheet.Column(colIndex++).Width = 8; else colIndex++;
			if (options.IncludeAirSeal) workSheet.Column(colIndex++).Width = 8; else colIndex++;
			if (options.IncludeCoatingLining) workSheet.Column(colIndex++).Width = 8; else colIndex++;
			if (options.IncludeDisassembly) workSheet.Column(colIndex++).Width = 8; else colIndex++;
			if (options.IncludeNotes) workSheet.Column(colIndex++).Width = 30; else colIndex++;
			if (options.IncludePreOrder) workSheet.Column(colIndex++).Width = 20; else colIndex++;
			if (options.IncludeScope) workSheet.Column(colIndex++).Width = 20; else colIndex++;
			if (options.IncludeActualAssemblyHours) workSheet.Column(colIndex++).Width = 12; else colIndex++;
			if (options.IncludeReworkHours) workSheet.Column(colIndex++).Width = 12; else colIndex++;
			if (options.IncludeNamePlate) workSheet.Column(colIndex++).Width = 15; else colIndex++;

			workSheet.View.FreezePanes(5, 1);
		}

		private void SetupGanttSchedulesWorksheet(ExcelWorksheet workSheet, List<GanttScheduleViewModel> schedules, ScheduleExportOptionsViewModel options)
		{
			// Title
			workSheet.Cells[1, 1].Value = "Machinery Gantt Schedule";
			using (ExcelRange title = workSheet.Cells[1, 1, 1, 47])
			{
				title.Merge = true;
				title.Style.Font.Name = "Arial";
				title.Style.Font.Size = 20;
				title.Style.Font.Bold = true;
				title.Style.Fill.PatternType = ExcelFillStyle.Solid;
				title.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(34, 45, 67)); // Deep slate blue
				title.Style.Font.Color.SetColor(Color.White);
				title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				title.Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(20, 25, 40));
			}
			workSheet.Row(1).Height = 30;

			// Headers Setup
			var headers = new List<(string Name, bool Include, Color Color)>();
			int colIndex = 1;
			if (options.IncludeOrderNumber) headers.Add(("OR #", true, Color.FromArgb(120, 180, 220))); // Soft sky blue
			if (options.IncludeEngineer) headers.Add(("ENG", true, Color.FromArgb(120, 180, 220)));
			if (options.IncludeGanttCustomerName) headers.Add(("Customer", true, Color.FromArgb(120, 180, 220)));
			if (options.IncludeMachineModel) headers.Add(("Model", true, Color.FromArgb(120, 180, 220)));
			if (options.IncludeQuantity) headers.Add(("QTY", true, Color.FromArgb(120, 180, 220)));
			if (options.IncludeGanttMedia) headers.Add(("Media", true, Color.FromArgb(120, 180, 220)));
			if (options.IncludeGanttSpareParts) headers.Add(("Spares", true, Color.FromArgb(120, 180, 220)));
			if (options.IncludeApprovedDrawingReceived) headers.Add(("App Dwg Rec'd", true, Color.FromArgb(120, 180, 220)));

			int staticCols = headers.Count;

			// Gantt Data Headers with Corrected Dates
			if (options.IncludeGanttData)
			{
				DateTime startDate = new DateTime(DateTime.Now.Year, 7, 1);
				for (int week = 1; week <= 33; week++)
				{
					int weekNumber = GetWeekOfYear(startDate, WeekStartOption.Monday);
					headers.Add((weekNumber.ToString(), true, Color.FromArgb(180, 230, 255))); // Very light blue

					// Row 2: Week Number
					workSheet.Cells[2, staticCols + week].Value = $"W{weekNumber}";
					workSheet.Cells[2, staticCols + week].Style.Font.Name = "Arial";
					workSheet.Cells[2, staticCols + week].Style.Font.Size = 10;
					workSheet.Cells[2, staticCols + week].Style.Font.Bold = true;
					workSheet.Cells[2, staticCols + week].Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Cells[2, staticCols + week].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(180, 230, 255));
					workSheet.Cells[2, staticCols + week].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(80, 120, 160));

					// Row 3: Date (MMM-dd)
					workSheet.Cells[3, staticCols + week].Value = startDate.ToString("MMM-dd");
					workSheet.Cells[3, staticCols + week].Style.Font.Name = "Arial";
					workSheet.Cells[3, staticCols + week].Style.Font.Size = 9;
					workSheet.Cells[3, staticCols + week].Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Cells[3, staticCols + week].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 240, 255));
					workSheet.Cells[3, staticCols + week].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(80, 120, 160));
					workSheet.Cells[3, staticCols + week].Style.TextRotation = 45;

					startDate = startDate.AddDays(7);
				}
			}

			// Special Notes Header
			if (options.IncludeSpecialNotes)
			{
				headers.Add(("Special Notes", true, Color.FromArgb(120, 180, 220)));
				int specialNotesCol = staticCols + 33 + 1;
				workSheet.Cells[2, specialNotesCol].Value = "Special Notes";
				workSheet.Cells[2, specialNotesCol].Style.Font.Name = "Arial";
				workSheet.Cells[2, specialNotesCol].Style.Font.Size = 11;
				workSheet.Cells[2, specialNotesCol].Style.Font.Bold = true;
				workSheet.Cells[2, specialNotesCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
				workSheet.Cells[2, specialNotesCol].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(120, 180, 220));
				workSheet.Cells[2, specialNotesCol].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(80, 120, 160));

				workSheet.Cells[3, specialNotesCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
				workSheet.Cells[3, specialNotesCol].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(140, 200, 240));
				workSheet.Cells[3, specialNotesCol].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(80, 120, 160));
			}

			// Static Headers Styling
			colIndex = 1;
			for (int i = 0; i < staticCols; i++)
			{
				var cell = workSheet.Cells[2, colIndex];
				cell.Value = headers[i].Name;
				cell.Style.Font.Name = "Arial";
				cell.Style.Font.Size = 11;
				cell.Style.Font.Bold = true;
				cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
				cell.Style.Fill.BackgroundColor.SetColor(headers[i].Color);
				cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(80, 120, 160));
				cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				colIndex++;
			}

			for (int i = 1; i <= staticCols; i++)
			{
				workSheet.Cells[3, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
				workSheet.Cells[3, i].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(140, 200, 240));
				workSheet.Cells[3, i].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(80, 120, 160));
			}

			// Premium Gantt Colors and Design
			var milestoneColors = new List<(Color Base, Color Highlight)>
	{
		(Color.FromArgb(46, 204, 113), Color.FromArgb(88, 230, 147)),  // Emerald Green
        (Color.FromArgb(52, 152, 219), Color.FromArgb(94, 175, 240)),  // Sky Blue
        (Color.FromArgb(155, 89, 182), Color.FromArgb(187, 143, 206)), // Amethyst Purple
        (Color.FromArgb(230, 126, 34), Color.FromArgb(245, 166, 87)),  // Tangerine Orange
        (Color.FromArgb(231, 76, 60), Color.FromArgb(245, 120, 110)),  // Coral Red
        (Color.FromArgb(26, 188, 156), Color.FromArgb(72, 219, 191))   // Turquoise
    };
			int colorIndex = 0;

			int row = 4;
			foreach (var schedule in schedules)
			{
				colIndex = 1;
				if (options.IncludeOrderNumber) workSheet.Cells[row, colIndex++].Value = schedule.OrderNumber;
				if (options.IncludeEngineer) workSheet.Cells[row, colIndex++].Value = schedule.Engineer;
				if (options.IncludeGanttCustomerName) workSheet.Cells[row, colIndex++].Value = schedule.CustomerName;
				if (options.IncludeMachineModel) workSheet.Cells[row, colIndex++].Value = schedule.MachineModel;
				if (options.IncludeQuantity) workSheet.Cells[row, colIndex++].Value = schedule.Quantity;
				if (options.IncludeGanttMedia) workSheet.Cells[row, colIndex++].Value = schedule.Media;
				if (options.IncludeGanttSpareParts) workSheet.Cells[row, colIndex++].Value = schedule.SpareParts;
				if (options.IncludeApprovedDrawingReceived) workSheet.Cells[row, colIndex++].Value = schedule.ApprovedDrawingReceived.ToString("d-MMM-yy");

				if (options.IncludeGanttData && schedule.GanttData != null)
				{
					foreach (var milestone in schedule.GanttData)
					{
						int startWeek = GetWeekOfYear((DateTime)milestone.StartDate, WeekStartOption.Monday);
						int endWeek = GetWeekOfYear((DateTime)milestone.EndDate, WeekStartOption.Monday);
						int startCol = staticCols + startWeek;
						int endCol = staticCols + endWeek;

						if (startCol >= colIndex && endCol <= staticCols + 33)
						{
							(Color Base, Color Highlight) colorPair;
							try
							{
								string hexCode = milestone.MilestoneClass?.Trim();
								if (!string.IsNullOrEmpty(hexCode))
								{
									hexCode = hexCode.TrimStart('#');
									if (System.Text.RegularExpressions.Regex.IsMatch(hexCode, @"^[0-9A-Fa-f]{6}$"))
									{
										int r = Convert.ToInt32(hexCode.Substring(0, 2), 16);
										int g = Convert.ToInt32(hexCode.Substring(2, 2), 16);
										int b = Convert.ToInt32(hexCode.Substring(4, 2), 16);
										colorPair = (Color.FromArgb(r, g, b), Color.FromArgb(
											Math.Min(255, r + 40),
											Math.Min(255, g + 40),
											Math.Min(255, b + 40)));
									}
									else if (System.Text.RegularExpressions.Regex.IsMatch(hexCode, @"^[0-9A-Fa-f]{3}$"))
									{
										int r = Convert.ToInt32(hexCode.Substring(0, 1) + hexCode.Substring(0, 1), 16);
										int g = Convert.ToInt32(hexCode.Substring(1, 1) + hexCode.Substring(1, 1), 16);
										int b = Convert.ToInt32(hexCode.Substring(2, 1) + hexCode.Substring(2, 1), 16);
										colorPair = (Color.FromArgb(r, g, b), Color.FromArgb(
											Math.Min(255, r + 40),
											Math.Min(255, g + 40),
											Math.Min(255, b + 40)));
									}
									else
									{
										throw new FormatException("Invalid hex code format.");
									}
								}
								else
								{
									colorPair = milestoneColors[colorIndex % milestoneColors.Count];
									colorIndex++;
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine($"Error parsing hex code '{milestone.MilestoneClass}': {ex.Message}");
								colorPair = milestoneColors[colorIndex % milestoneColors.Count];
								colorIndex++;
							}

							int rangeLength = endCol - startCol + 1;
							for (int col = startCol; col <= endCol; col++)
							{
								var cell = workSheet.Cells[row, col];
								double gradientFactor = rangeLength > 1 ? (double)(col - startCol) / (rangeLength - 1) : 0.5;
								int r = (int)(colorPair.Base.R + (colorPair.Highlight.R - colorPair.Base.R) * gradientFactor);
								int g = (int)(colorPair.Base.G + (colorPair.Highlight.G - colorPair.Base.G) * gradientFactor);
								int b = (int)(colorPair.Base.B + (colorPair.Highlight.B - colorPair.Base.B) * gradientFactor);
								Color cellColor = Color.FromArgb(r, g, b);

								// Premium styling
								cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
								cell.Style.Fill.BackgroundColor.SetColor(cellColor);
								// Subtle diagonal pattern for texture
								if (rangeLength > 1 && col == startCol + (rangeLength / 2))
								{
									cell.Style.Fill.PatternType = ExcelFillStyle.LightUp;
									cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 255, 20)); // Very faint white overlay
								}

								// Enhanced borders
								cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
								cell.Style.Border.Top.Color.SetColor(Color.FromArgb(60, 60, 60));
								cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
								cell.Style.Border.Bottom.Color.SetColor(Color.FromArgb(60, 60, 60));
								cell.Style.Border.Left.Style = col == startCol ? ExcelBorderStyle.Medium : ExcelBorderStyle.Thin;
								cell.Style.Border.Left.Color.SetColor(col == startCol ? Color.FromArgb(20, 20, 20) : Color.FromArgb(120, 120, 120));
								cell.Style.Border.Right.Style = col == endCol ? ExcelBorderStyle.Medium : ExcelBorderStyle.Thin;
								cell.Style.Border.Right.Color.SetColor(col == endCol ? Color.FromArgb(20, 20, 20) : Color.FromArgb(120, 120, 120));
							}
						}
					}
					colIndex = staticCols + 34;
				}

				if (options.IncludeSpecialNotes)
				{
					workSheet.Cells[row, colIndex].Value = schedule.SpecialNotes;
					colIndex++;
				}

				row++;
			}

			// Data Range Styling
			if (row > 4)
			{
				using (var dataRange = workSheet.Cells[4, 1, row - 1, headers.Count])
				{
					dataRange.Style.Font.Name = "Arial";
					dataRange.Style.Font.Size = 10;
					dataRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
					dataRange.Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(34, 45, 67));
					dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
					dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
					dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
					dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
				}

				for (int i = 4; i < row; i++)
				{
					using (var rowRange = workSheet.Cells[i, 1, i, staticCols])
					{
						rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
						rowRange.Style.Fill.BackgroundColor.SetColor(i % 2 == 0 ? Color.FromArgb(240, 245, 250) : Color.FromArgb(255, 255, 255));
					}
				}
			}

			// Text Wrapping
			colIndex = 1;
			if (options.IncludeOrderNumber) colIndex++;
			if (options.IncludeEngineer) colIndex++;
			if (options.IncludeGanttCustomerName) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;
			if (options.IncludeMachineModel) workSheet.Column(colIndex++).Style.WrapText = true; else colIndex++;
			if (options.IncludeQuantity) colIndex++;
			if (options.IncludeGanttMedia) colIndex++;
			if (options.IncludeGanttSpareParts) colIndex++;
			if (options.IncludeApprovedDrawingReceived) colIndex++;
			if (options.IncludeSpecialNotes)
			{
				int specialNotesCol = staticCols + 33 + 1;
				workSheet.Column(specialNotesCol).Style.WrapText = true;
			}

			// Column Widths
			workSheet.Cells.AutoFitColumns();
			colIndex = 1;
			if (options.IncludeOrderNumber) workSheet.Column(colIndex++).Width = 12; else colIndex++;
			if (options.IncludeEngineer) workSheet.Column(colIndex++).Width = 8; else colIndex++;
			if (options.IncludeGanttCustomerName) workSheet.Column(colIndex++).Width = 25; else colIndex++;
			if (options.IncludeMachineModel) workSheet.Column(colIndex++).Width = 20; else colIndex++;
			if (options.IncludeQuantity) workSheet.Column(colIndex++).Width = 6; else colIndex++;
			if (options.IncludeGanttMedia) workSheet.Column(colIndex++).Width = 10; else colIndex++;
			if (options.IncludeGanttSpareParts) workSheet.Column(colIndex++).Width = 10; else colIndex++;
			if (options.IncludeApprovedDrawingReceived) workSheet.Column(colIndex++).Width = 14; else colIndex++;
			if (options.IncludeGanttData)
			{
				for (int i = staticCols + 1; i <= staticCols + 33; i++)
					workSheet.Column(i).Width = 7; // Wider for better visibility
			}
			if (options.IncludeSpecialNotes)
			{
				int specialNotesCol = staticCols + 33 + 1;
				workSheet.Column(specialNotesCol).Width = 40;
			}

			workSheet.View.FreezePanes(4, staticCols + 1);
		}
		private void SetupHeader(ExcelWorksheet workSheet, int row, int col, string value, Color bgColor)
        {
            workSheet.Cells[row, col].Value = value;
            workSheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(bgColor);
            workSheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }


        private static int GetColumnFromWeek(int week)
        {
            if (week >= 1 && week <= 33) // Weeks 1-33 of 2025
                return 11 + (week - 1);  // Columns 11-43
            return -1;
        }


        private static int GetWeekOfYear(DateTime date, WeekStartOption startOfWeek)
        {
            var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
            DayOfWeek dayOfWeek = startOfWeek == WeekStartOption.Monday ? DayOfWeek.Monday : DayOfWeek.Sunday;
            return cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, dayOfWeek);
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
        (g.PackageReleased, g.PackageReleased, "Package Released to PIC/Spare Parts to Customer Service", "#ADF802"),
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

            TempData["SuccessMessage"] = "Gantt chart has been unfinalized.";
            return RedirectToAction("Index", new { isFinalized = false });
        }


        private bool GanttDataExists(int id)
        {
            return _context.GanttDatas.Any(e => e.ID == id);
        }
    }
}
