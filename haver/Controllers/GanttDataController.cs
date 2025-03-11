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
using System.Drawing;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text.RegularExpressions;

namespace haver.Controllers
{
    public class GanttDataController : ElephantController
    {
        private readonly HaverContext _context;

        public GanttDataController(HaverContext context)
        {
            _context = context;
        }

        // GET: GanttData
        public async Task<IActionResult> Index(int? page, int? pageSizeID, DateTime? DtString,
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
            if (DtString.HasValue)
            {
                DateTime searchDae = DtString.Value.Date;
                gData = gData.Where(s => s.AppDRcd.HasValue &&
                                         s.AppDRcd.Value.Year == searchDae.Year &&
                                         s.AppDRcd.Value.Month == searchDae.Month &&
                                         s.AppDRcd.Value.Day == searchDae.Day);
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
            var ganttData = _context.GanttDatas
                 .Include(g => g.SalesOrder)
                .ThenInclude(m => m.Machines)
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
                PurchaseOrdersIssued = m.SalesOrder?.SoDate.ToString("yyy-MM-dd"),
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SalesOrderID,AppDRcd,EngExpected,EngReleased,CustomerApproval,PackageReleased,PurchaseOrdersIssued,PurchaseOrdersCompleted,SupplierPODue,AssemblyStart,AssemblyComplete,ShipExpected,ShipActual,DeliveryExpected,DeliveryActual,Notes")] GanttData ganttData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get all machines linked to the selected Sales Order
                    var machines = _context.Machines
                                           .Where(m => m.SalesOrderID == ganttData.SalesOrderID)
                                           .ToList();

                    if (machines.Any())
                    {
                        foreach (var machine in machines)
                        {
                            var newGanttData = new GanttData
                            {
                                SalesOrderID = ganttData.SalesOrderID,
                                MachineID = machine.ID, // Assign MachineID
                                AppDRcd = ganttData.AppDRcd,
                                EngExpected = ganttData.EngExpected,
                                EngReleased = ganttData.EngReleased,
                                CustomerApproval = ganttData.CustomerApproval,
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
                                IsFinalized = false, // Default to false
                                StartOfWeek = WeekStartOption.Monday // Or dynamically set based on user input
                            };

                            _context.GanttDatas.Add(newGanttData);
                        }

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

            // Re-populate dropdown with Sales Orders
            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber");

            return View(ganttData);
        }




        // GET: GanttData/Edit/5
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var gDataToUpdate = await _context.GanttDatas
                .Include(g => g.SalesOrder)
                .Include(g => g.Machine) // Ensure related data is loaded
                .FirstOrDefaultAsync(e => e.ID == id);

            if (gDataToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<GanttData>(gDataToUpdate, "",
                 p => p.SalesOrderID, p => p.AppDRcd, p => p.AppDExp, p => p.EngExpected, p => p.EngReleased, p => p.CustomerApproval,
                 p => p.PackageReleased, p => p.PurchaseOrdersIssued, p => p.PurchaseOrdersCompleted, p => p.PurchaseOrdersReceived,
                 p => p.SupplierPODue, p => p.AssemblyStart, p => p.AssemblyComplete, p => p.ShipExpected, p => p.ShipActual, p => p.DeliveryExpected,
                 p => p.DeliveryActual, p => p.Notes))
            {
                try
                {
                    bool isUpdated = false;

                    // Ensure related SalesOrder and Machine reflect changes
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

                        if (gDataToUpdate.EngReleased != null && gDataToUpdate.SalesOrder.EngPRel != gDataToUpdate.EngReleased)
                        {
                            gDataToUpdate.SalesOrder.EngPRel = gDataToUpdate.EngReleased;
                            isUpdated = true;
                        }
                    }

                    if (gDataToUpdate.Machine != null)
                    {
                        if (gDataToUpdate.AssemblyStart != null && gDataToUpdate.Machine.AssemblyStart != gDataToUpdate.AssemblyStart)
                        {
                            gDataToUpdate.Machine.AssemblyStart = gDataToUpdate.AssemblyStart;
                            isUpdated = true;
                        }

                        if (gDataToUpdate.AssemblyComplete != null && gDataToUpdate.Machine.AssemblyComplete != gDataToUpdate.AssemblyComplete)
                        {
                            gDataToUpdate.Machine.AssemblyComplete = gDataToUpdate.AssemblyComplete;
                            isUpdated = true;
                        }

                        if (gDataToUpdate.ShipExpected != null && gDataToUpdate.Machine.RToShipExp != gDataToUpdate.ShipExpected)
                        {
                            gDataToUpdate.Machine.RToShipExp = gDataToUpdate.ShipExpected;
                            isUpdated = true;
                        }

                        if (gDataToUpdate.ShipActual != null && gDataToUpdate.Machine.RToShipA != gDataToUpdate.ShipActual)
                        {
                            gDataToUpdate.Machine.RToShipA = gDataToUpdate.ShipActual;
                            isUpdated = true;
                        }
                    }

                    // If there were any updates, save changes
                    if (isUpdated)
                    {
                        await _context.SaveChangesAsync();
                    }

                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Gantt Data has been successfully edited, Necessary Dates have been updated on the sales order and machine";
                    return RedirectToAction(nameof(Index));
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
                }

                await _context.SaveChangesAsync();
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

		public IActionResult DownloadGanttSchedules()
		{
			// Fetch GanttDatas with related SalesOrder and Machine data
			var ganttData = _context.GanttDatas
				.Include(g => g.SalesOrder)
				.Include(g => g.Machine).ThenInclude(m => m.MachineType)
				.AsNoTracking()
				.ToList();

			// Generate milestone tasks
			var milestoneTasks = ganttData
				.SelectMany(g => GetMilestoneTasks(g))
				.ToList();

			Console.WriteLine($"Total GanttDatas: {ganttData.Count}");
			Console.WriteLine($"Total Milestone Tasks: {milestoneTasks.Count}");

			// Create order number to ID mapping
			var orderNumberToIdMap = _context.SalesOrders
				.AsNoTracking()
				.GroupBy(so => so.OrderNumber ?? "")
				.ToDictionary(g => g.Key, g => g.First().ID);
			Console.WriteLine($"OrderNumber to ID Map Keys: {string.Join(", ", orderNumberToIdMap.Keys)}");

			// Group milestone tasks by SalesOrder
			var groupedTasks = milestoneTasks
				.GroupBy(g => g.SalesOrder)
				.ToList();

			// Create ganttDataLookup dictionary
			var ganttDataLookup = new Dictionary<int, List<GanttViewModel>>();
			foreach (var group in groupedTasks)
			{
				if (!orderNumberToIdMap.TryGetValue(group.Key, out var salesOrderId))
				{
					Console.WriteLine($"Warning: SalesOrder '{group.Key}' not found in orderNumberToIdMap. Skipping this group.");
					continue;
				}
				ganttDataLookup[salesOrderId] = group.ToList();
			}

			Console.WriteLine($"ganttDataLookup Keys: {string.Join(", ", ganttDataLookup.Keys)}");

			// Fetch and map schedules
			var schedules = _context.SalesOrders
				.Include(so => so.Machines).ThenInclude(m => m.MachineType)
				.Include(so => so.SalesOrderEngineers).ThenInclude(se => se.Engineer)
				.OrderByDescending(so => so.SoDate)
				.AsNoTracking()
				.ToList()
				.Select(so => new
				{
					OrderNumber = so.OrderNumber ?? "",
					// Fix Engineer: Check if SalesOrderEngineers exists and get the Engineer's name
					Engineer = $"{so.SalesOrderEngineers?.FirstOrDefault()?.Engineer.FirstName[0]} {so.SalesOrderEngineers?.FirstOrDefault()?.Engineer.LastName[0]}",
					CustomerName = so.CompanyName ?? "Unknown",
					Quantity = so.Machines?.Count() ?? 0,
					Size = so.Machines != null
						? string.Join(", ", so.Machines.Select(m => m.MachineType?.Size ?? "N/A"))
						: "N/A",
					Class = so.Machines != null
						? string.Join(", ", so.Machines.Select(m => m.MachineType?.Class ?? "N/A"))
						: "N/A",
					SizeDeck = so.Machines != null
						? string.Join(", ", so.Machines.Select(m => m.MachineType?.Deck ?? "N/A"))
						: "N/A",
					Media = so.Media ? "Yes" : "No",
					SpareParts = so.SpareParts ? "Yes" : "No",
					ApprovedDrawingReceived = so.AppDwgExp,
					GanttData = ganttDataLookup.ContainsKey(so.ID) ? ganttDataLookup[so.ID] : new List<GanttViewModel>(),
<<<<<<< Updated upstream
					// Get SpecialNotes from GanttData if available, otherwise fall back to Comments, and strip HTML
					SpecialNotes = ganttDataLookup.ContainsKey(so.ID) && ganttDataLookup[so.ID].Any()
						? string.Join("; ", ganttDataLookup[so.ID].Select(g => g.notes != null ? System.Net.WebUtility.HtmlDecode(g.notes.Replace("<p>", "").Replace("</p>", "")) : ""))
						: so.Comments != null ? System.Net.WebUtility.HtmlDecode(so.Comments.Replace("<p>", "").Replace("</p>", "")) : ""
				})
=======
					SpecialNotes = !string.IsNullOrEmpty(so.Comments)
                        ? Regex.Replace(so.Comments, "<.*?>", string.Empty)
                        : " "
                })
>>>>>>> Stashed changes
				.ToList();

			Console.WriteLine($"Total Schedules: {schedules.Count}");
			foreach (var schedule in schedules)
			{
				Console.WriteLine($"OrderNumber: {schedule.OrderNumber}, GanttData Count: {schedule.GanttData.Count}");
			}

			using (ExcelPackage excel = new ExcelPackage())
			{
				var workSheet = excel.Workbook.Worksheets.Add("Machine Schedules");

				// Set overall document properties
				excel.Workbook.Properties.Title = "Machinery Gantt Schedule";
				excel.Workbook.Properties.Author = "xAI Generated";
				excel.Workbook.Properties.Company = "Your Company";

				// Title styling
				workSheet.Cells[1, 1].Value = "Machinery Gantt Schedule";
				using (ExcelRange title = workSheet.Cells[1, 1, 1, 47])
				{
					title.Merge = true;
					title.Style.Font.Name = "Calibri";
					title.Style.Font.Size = 20;
					title.Style.Font.Bold = true;
					title.Style.Fill.PatternType = ExcelFillStyle.Solid;
					title.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(31, 78, 121)); // Darker blue
					title.Style.Font.Color.SetColor(Color.White);
					title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					title.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
					title.Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(23, 54, 93));
				}
				workSheet.Row(1).Height = 30; // Increase title row height

				// Headers for static fields
				string[] headers = {
			"OR #", "ENG.", "Customer Name", "QTY", "Size", "Class", "SizeDeck", "Media", "Spare Parts", "App Dwg Rec'd"
		};

				for (int i = 0; i < headers.Length; i++)
				{
					SetupHeader(workSheet, 2, i + 1, headers[i], Color.FromArgb(91, 155, 213)); // Light blue
				}

				// Gantt chart headers (weeks 1-33 of 2025)
				int colIndex = 11;
				for (int week = 1; week <= 33; week++)
				{
					SetupHeader(workSheet, 2, colIndex++, week.ToString(), Color.FromArgb(155, 194, 230)); // Lighter blue gradient
				}

				// Special Notes header
				SetupHeader(workSheet, 2, colIndex, "Special Notes", Color.FromArgb(91, 155, 213));

				// Style header row
				using (var headerRange = workSheet.Cells[2, 1, 2, 47])
				{
					headerRange.Style.Font.Name = "Calibri";
					headerRange.Style.Font.Size = 11;
					headerRange.Style.Font.Bold = true;
					headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
					headerRange.Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(23, 54, 93));
					headerRange.Style.WrapText = true;
				}
				workSheet.Row(2).Height = 25;

				// Load data
				int row = 3;
				foreach (var schedule in schedules)
				{
					workSheet.Cells[row, 1].Value = schedule.OrderNumber;
					workSheet.Cells[row, 2].Value = schedule.Engineer;
					workSheet.Cells[row, 3].Value = schedule.CustomerName;
					workSheet.Cells[row, 4].Value = schedule.Quantity;
					workSheet.Cells[row, 5].Value = schedule.Size;
					workSheet.Cells[row, 6].Value = schedule.Class;
					workSheet.Cells[row, 7].Value = schedule.SizeDeck;
					workSheet.Cells[row, 8].Value = schedule.Media;
					workSheet.Cells[row, 9].Value = schedule.SpareParts;
					workSheet.Cells[row, 10].Value = schedule.ApprovedDrawingReceived.ToString("d-MMM-yy");

					Console.WriteLine($"OrderNumber: {schedule.OrderNumber}, GanttData Count: {schedule.GanttData.Count}");
					foreach (var milestone in schedule.GanttData)
					{
						Console.WriteLine($"Milestone: {milestone.MachineName}, Start: {milestone.StartDate}, End: {milestone.EndDate}");

						int startWeek = GetWeekOfYear((DateTime)milestone.StartDate, WeekStartOption.Monday);
						int endWeek = GetWeekOfYear((DateTime)milestone.EndDate, WeekStartOption.Monday);

						Console.WriteLine($"Raw Start Week: {startWeek}, Raw End Week: {endWeek}");

						int startCol = GetColumnFromWeek(startWeek);
						int endCol = GetColumnFromWeek(endWeek);

						Console.WriteLine($"Start Col: {startCol}, End Col: {endCol}");

						if (startCol == -1 || endCol == -1 || startCol > 43 || endCol > 43)
						{
							Console.WriteLine($"Skipping milestone {milestone.MachineName} - out of range");
							continue;
						}

						try
						{
							string colorHex = milestone.MilestoneClass.TrimStart('#');
							Color milestoneColor;
							try
							{
								milestoneColor = ColorTranslator.FromHtml($"#{colorHex}");
							}
							catch (Exception)
							{
								Console.WriteLine($"Invalid color format for {milestone.MilestoneClass}. Using default gray.");
								milestoneColor = Color.Gray;
							}

							for (int col = startCol; col <= endCol && col <= 43; col++)
							{
								var cell = workSheet.Cells[row, col];
								cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
								cell.Style.Fill.BackgroundColor.SetColor(milestoneColor);
								cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(127, 127, 127)); // Gray border
								cell.Style.Font.Size = 9;
								cell.Style.Font.Name = "Calibri";

								if (col == startCol)
								{
									cell.Value = milestone.MachineName.Split(" - ")[1];
									cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
								}

								Console.WriteLine($"Row: {row}, Col: {col}, Color: {milestone.MilestoneClass}, Value: {cell.Value}");
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error applying color for milestone {milestone.MachineName}: {ex.Message}");
							continue;
						}
					}

					// Assign Special Notes to column 47 with HTML stripped and ensure it fits
					workSheet.Cells[row, 47].Value = schedule.SpecialNotes;
					workSheet.Cells[row, 47].Style.WrapText = true; // Ensure text wraps
					workSheet.Cells[row, 47].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

					row++;
				}

				// Style data rows
				using (var dataRange = workSheet.Cells[3, 1, row - 1, 47])
				{
					dataRange.Style.Font.Name = "Calibri";
					dataRange.Style.Font.Size = 10;
					dataRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
					dataRange.Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(23, 54, 93));
					dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
					dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
					dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
					dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
				}

				// Alternating row colors
				for (int i = 3; i < row; i++)
				{
					using (var rowRange = workSheet.Cells[i, 1, i, 10]) // Static columns only
					{
						rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
						rowRange.Style.Fill.BackgroundColor.SetColor(i % 2 == 0 ? Color.FromArgb(240, 248, 255) : Color.White); // AliceBlue for even rows
					}
				}

				// Text wrapping and alignment for specific columns
				int[] wrapTextColumns = { 3, 5, 6, 7, 47 };
				foreach (int col in wrapTextColumns)
				{
					workSheet.Column(col).Style.WrapText = true;
					workSheet.Column(col).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
				}
				workSheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // QTY
				workSheet.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Media
				workSheet.Column(9).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Spare Parts

				// Column widths
				workSheet.Cells.AutoFitColumns();
				workSheet.Column(1).Width = 12;  // OR #
				workSheet.Column(2).Width = 12;  // ENG.
				workSheet.Column(3).Width = 25;  // Customer Name
				workSheet.Column(4).Width = 6;   // QTY
				workSheet.Column(5).Width = 15;  // Size
				workSheet.Column(6).Width = 15;  // Class
				workSheet.Column(7).Width = 15;  // SizeDeck
				workSheet.Column(8).Width = 10;  // Media
				workSheet.Column(9).Width = 14;  // Spare Parts
				workSheet.Column(10).Width = 14; // App Dwg Rec'd
				for (int i = 11; i <= 43; i++)   // Gantt chart columns
				{
					workSheet.Column(i).Width = 4; // Slightly wider for readability
				}
				workSheet.Column(47).Width = 40; // Increased width for Special Notes to accommodate longer text

				// Freeze panes
				workSheet.View.FreezePanes(3, 11);

				// Add grid lines for better readability
				workSheet.Cells[3, 11, row - 1, 43].Style.Border.Top.Style = ExcelBorderStyle.Hair;
				workSheet.Cells[3, 11, row - 1, 43].Style.Border.Left.Style = ExcelBorderStyle.Hair;

				try
				{
					byte[] theData = excel.GetAsByteArray();
					return File(theData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Gantt Schedule.xlsx");
				}
				catch (Exception)
				{
					return BadRequest("Could not build and download the file.");
				}
			}
		}

		private static int GetColumnFromWeek(int week)
		{
			if (week >= 1 && week <= 33) // Weeks 1-33 of 2025
				return 11 + (week - 1);  // Columns 11-43
			return -1;
		}

		private void SetupHeader(ExcelWorksheet worksheet, int row, int col, string value, Color bgColor)
		{
			worksheet.Cells[row, col].Value = value;
			worksheet.Cells[row, col].Style.Font.Bold = true;
			worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
			worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(bgColor);
			worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(23, 54, 93));
			worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
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
		(g.CustomerApproval, g.PackageReleased, "Customer Approval Received", "#E26B0A"),
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
						StartDate = start.Value,
						EndDate = end.Value,
						Progress = 100,
						MilestoneClass = color,
                        notes=g.Notes
                        
                        
					});
					Console.WriteLine($"Milestone: {name}, Start: {start.Value}, End: {end.Value}, Color: {color}");
				}
			}

			return tasks;
		}

		//private string GetMilestoneClass(GanttData g)
		//{
		//    if (g.EngReleased.HasValue) return "eng-released";
		//    if (g.PackageReleased.HasValue) return "package-released";
		//    if (g.ShipExpected.HasValue) return "shipping";
		//    if (g.DeliveryExpected.HasValue) return "delivery";
		//    return "default-task";
		//}

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
