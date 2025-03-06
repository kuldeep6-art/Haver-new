using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using haver.Data;
using haver.Models;
using haver.CustomControllers;
using haver.Utilities;
using haver.ViewModels;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.VisualStudio.TextTemplating;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Text.RegularExpressions;

namespace haver.Controllers
{
    public class SalesOrderController : ElephantController
    {
        private readonly HaverContext _context;

        public SalesOrderController(HaverContext context)
        {
            _context = context;
        }

        // GET: SalesOrder
        public async Task<IActionResult> Index(int? page, int? pageSizeID,string status, DateTime? DtString, string? SearchString, string? CString,
            string? actionButton, string sortDirection = "asc", string sortField = "OrderNumber")
        {
            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Order Number", "Customer" };

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;

           // PopulateDropDownLists();

            var salesOrders = from s in _context.SalesOrders
                        //.Include(s => s.Customer)
                        .Include(d => d.SalesOrderEngineers).ThenInclude(d => d.Engineer)
                        .AsNoTracking()
                              select s;

            // Handle status filtering
            switch (status)
            {
                case "Archived":
                    salesOrders = salesOrders.Where(so => so.Status == Status.Archived);
                    break;
                case "Completed":
                    salesOrders = salesOrders.Where(so => so.Status == Status.Completed);
                    break;
                case "Draft":
                    salesOrders = salesOrders.Where(so => so.Status == Status.Draft);
                    break;
                default:
                    // Active tab shows non-archived and non-completed
                    salesOrders = salesOrders.Where(so => so.Status != Status.Archived && so.Status != Status.Completed && so.Status!= Status.Draft);
                    break;
            }


            ////Add as many filters as needed
            //if (CustomerID.HasValue)
            //{
            //    salesOrders = salesOrders.Where(p => p.CustomerID == CustomerID);
            //    numberFilters++;
            //}

            if (!String.IsNullOrEmpty(SearchString))
            {
                salesOrders = salesOrders.Where(p => p.OrderNumber.Contains(SearchString));
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(CString))
            {
                salesOrders = salesOrders.Where(p => p.CompanyName.Contains(CString));
                numberFilters++;
            }
            if(DtString.HasValue)
            {
                DateTime searchDae = DtString.Value.Date;
                salesOrders = salesOrders.Where(s => s.SoDate.Year == searchDae.Year &&
                                                s.SoDate.Month == searchDae.Month &&
                                                s.SoDate.Day == searchDae.Day);
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
            if (sortField == "Customer")
            {
                if (sortDirection == "asc")
                {
                    salesOrders = salesOrders
                        .OrderByDescending(p => p.CompanyName);
                }
                else
                {
                    salesOrders = salesOrders
                        .OrderBy(p => p.CompanyName);
                }
            }

            else //Sorting by Patient Name
            {
                if (sortDirection == "asc")
                {
                    salesOrders = salesOrders
                        .OrderBy(p => p.OrderNumber);
                }
                else
                {
                    salesOrders = salesOrders
                        .OrderByDescending(p => p.OrderNumber);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;


            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<SalesOrder>.CreateAsync(salesOrders.AsNoTracking(), page ?? 1, pageSize);

            ViewBag.Status = status;

            return View(pagedData);
        }

        // GET: SalesOrder/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrder = await _context.SalesOrders
                //.Include(s => s.Customer)
                .Include(s => s.PackageRelease)
                .Include(s => s.SalesOrderEngineers).ThenInclude(s => s.Engineer)
                .Include(s => s.Machines)
                    .ThenInclude(m => m.MachineType) // ✅ Include MachineType to prevent null issues
                .Include(s => s.Machines)
                    .ThenInclude(m => m.Procurements) // Include Procurements
                        .ThenInclude(p => p.Vendor) // Include Vendor details
                .FirstOrDefaultAsync(m => m.ID == id);

            if (salesOrder == null)
            {
                return NotFound();
            }

            return View(salesOrder);
        }


        // GET: SalesOrder/Create
        public IActionResult Create()
        {
            SalesOrder salesOrder = new SalesOrder();
            PopulateAssignedSpecialtyData(salesOrder);
           // PopulateDropDownLists();

            //Fetch available engineers from the database
            ViewBag.EngineersList = new MultiSelectList(_context.Engineers, "ID", "EngineerInitialsB");
            return View();
        }

        // POST: SalesOrder/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,OrderNumber,CompanyName,SoDate,Price,Currency,ShippingTerms,AppDwgExp,AppDwgRel,AppDwgRet,PreOExp,PreORel,EngPExp,EngPRel,Comments,Status")] SalesOrder salesOrder, string[] selectedOptions, int[] selectedEngineers, bool saveAsDraft)
        {
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CompanyName == salesOrder.CompanyName);
                if (customer == null)
                {
                    customer = new Customer { CompanyName = salesOrder.CompanyName };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }
               // salesOrder.CustomerID = customer.ID;
                UpdateSalesOrderEngineers(selectedOptions, salesOrder);
                if (ModelState.IsValid)
				{
                    salesOrder.Status = saveAsDraft ? Status.Draft : Status.InProgress;
                    _context.Add(salesOrder);
					await _context.SaveChangesAsync();
                    TempData["Message"] = saveAsDraft ? "Sales Order saved as draft" : "Sales Order created successfully";
                    // Redirect to Index if saved as Draft, otherwise go to Details
                    return saveAsDraft
                        ? RedirectToAction(nameof(Index))
                        : RedirectToAction("Details", new { id = salesOrder.ID });
                }
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException dex)
			{
				if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: SalesOrders.OrderNumber"))
				{
					ModelState.AddModelError("OrderNumber", "Unable to save changes. Remember, you cannot have duplicate Order Number.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
                
            }
            PopulateAssignedSpecialtyData(salesOrder);
           // PopulateDropDownLists(salesOrder);          
            return View(salesOrder);
        }

        // GET: SalesOrder/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


			var salesOrder = await _context.SalesOrders
				.Include(d => d.SalesOrderEngineers).ThenInclude(d => d.Engineer)
				.FirstOrDefaultAsync(p => p.ID == id);

			if (salesOrder == null)
            {
                return NotFound();
            }
            PopulateAssignedSpecialtyData(salesOrder);
            //PopulateDropDownLists(salesOrder);
			return View(salesOrder);
		}

        // POST: SalesOrder/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string[] selectedOptions)
        {
			var salesOrderToUpdate = await _context.SalesOrders
				.Include(d => d.SalesOrderEngineers).ThenInclude(d => d.Engineer)
				.FirstOrDefaultAsync(p => p.ID == id);

			if (salesOrderToUpdate == null)
			{
				return NotFound();
			}

            UpdateSalesOrderEngineers(selectedOptions, salesOrderToUpdate);

            if (await TryUpdateModelAsync<SalesOrder>(salesOrderToUpdate, "",
			p => p.OrderNumber, p => p.SoDate, p => p.Price, p => p.Currency, p => p.ShippingTerms,
			p => p.AppDwgExp, p => p.AppDwgRel, p => p.AppDwgRet, p => p.PreOExp, p => p.PreORel, p => p.EngPExp,
			p => p.EngPRel, p => p.CompanyName, p => p.Comments))
			{
				try
                {
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Sales Order has been successfully edited";
                    return RedirectToAction("Details", new { salesOrderToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalesOrderExists(salesOrderToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
				}
				catch (DbUpdateException dex)
				{
					if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: SalesOrders.OrderNumber"))
					{
						ModelState.AddModelError("OrderNumber", "Unable to save changes. Remember, you cannot have duplicate Order Number.");
					}
					else
					{
						ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
					}
				}

			}
            PopulateAssignedSpecialtyData(salesOrderToUpdate);
            //PopulateDropDownLists(salesOrderToUpdate);
			return View(salesOrderToUpdate);
		}

        // GET: SalesOrder/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrder = await _context.SalesOrders
                //.Include(s => s.Customer)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (salesOrder == null)
            {
                return NotFound();
            }

            return View(salesOrder);
        }

        // POST: SalesOrder/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salesOrder = await _context.SalesOrders.FindAsync(id);
            if (salesOrder != null)
            {
                _context.SalesOrders.Remove(salesOrder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        //private void PopulateDropDownLists(SalesOrder? salesOrder = null)
        //{
        //	ViewData["CustomerID"] = new SelectList(_context.Customers, "ID", "CompanyName");
        //}


        public async Task<IActionResult> Continue(int id)
        {
            var salesOrder = await _context.SalesOrders.FindAsync(id);
            if (salesOrder == null)
            {
                return NotFound();
            }

            // Change status from Draft to InProgress
            if (salesOrder.Status == Status.Draft)
            {
                salesOrder.Status = Status.InProgress;
                _context.Update(salesOrder);
                await _context.SaveChangesAsync();
            }

            // Redirect directly to Edit page
            return RedirectToAction("Edit", new { id = salesOrder.ID });
        }


        // GET: SalesOrder/Archive/5
        public async Task<IActionResult> Archive(int id)
        {
            var salesOrder = await _context.SalesOrders
                .FirstOrDefaultAsync(m => m.ID == id);

            if (salesOrder == null)
            {
                return NotFound();
            }

            salesOrder.Status = Status.Archived;
            _context.Update(salesOrder);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Message"] = "Sales Order has been successfully archived";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occured";
            }

            return RedirectToAction(nameof(Index));  // Redirect back to the Index page after archiving.
        }

        public async Task<IActionResult> Restore(int id)
        {
            var salesOrder = await _context.SalesOrders
                .FirstOrDefaultAsync(m => m.ID == id);

            if (salesOrder == null)
            {
                return NotFound();
            }

            salesOrder.Status = Status.InProgress;
            _context.Update(salesOrder);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Message"] = "Sales Order has been successfully restored and status set to active";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occured";
            }

            return RedirectToAction(nameof(Index));  // Redirect back to the Index page after restoring.
        }

		public IActionResult DownloadMachineSchedules()
		{
			var schedules = _context.SalesOrders
				.Include(so => so.Machines).ThenInclude(g => g.MachineType)
				.Include(so => so.Machines).ThenInclude(m => m.Procurements).ThenInclude(p => p.Vendor)
				.OrderByDescending(so => so.SoDate)
				.AsEnumerable()
				.Select(so => new
				{
					SalesOrderNumber = so.OrderNumber ?? "",
					SalesOrderDate = so.SoDate.ToShortDateString() ?? "N/A",
					CustomerName = so.CompanyName ?? "Unknown",
					MachineDescriptions = string.Join(Environment.NewLine, so.Machines.Select(m => m.MachineType.Description ?? "Unknown")),
					SerialNumbers = string.Join(Environment.NewLine, so.Machines.Select(m => m.SerialNumber ?? "N/A")),
					ProductionOrderNumbers = string.Join(Environment.NewLine, so.Machines.Select(m => m.ProductionOrderNumber ?? "N/A")),
					PackageReleaseDateE = "P - " + so.EngPExp?.ToShortDateString() ?? "N/A",
					PackageReleaseDateA = "A - " + so.EngPRel?.ToString() ?? "N/A",
					VendorNames = string.Join(Environment.NewLine, so.Machines.SelectMany(m => m.Procurements.Select(p => p.Vendor.Name ?? "N/A"))),
					PoNumbers = string.Join(Environment.NewLine, so.Machines.SelectMany(m => m.Procurements.Select(p => p.PONumber ?? "N/A"))),
					PoDueDates = string.Join(Environment.NewLine, so.Machines
						.SelectMany(m => m.Procurements
						.Select(p => p.PODueDate.HasValue ? p.PODueDate.Value.ToString("yyyy-MM-dd") : "N/A"))),
					DeliveryDates = string.Join(Environment.NewLine, so.Machines
						.SelectMany(m => m.Procurements
						.Select(p => p.ExpDueDate.HasValue ? p.ExpDueDate.Value.ToString("yyyy-MM-dd") : "N/A"))),
					Media = string.Join(Environment.NewLine, so.Machines.Select(m => m.Media ? "Yes" : "No")),
					SpareParts = string.Join(Environment.NewLine, so.Machines.Select(m => m.SpareParts ? "Yes" : "No")),
					Base = string.Join(Environment.NewLine, so.Machines.Select(m => m.Base ? "Yes" : "No")),
					AirSeal = string.Join(Environment.NewLine, so.Machines.Select(m => m.AirSeal ? "Yes" : "No")),
					CoatingLining = string.Join(Environment.NewLine, so.Machines.Select(m => m.CoatingLining ? "Yes" : "No")),
					Disassembly = string.Join(Environment.NewLine, so.Machines.Select(m => m.Disassembly ? "Yes" : "No")),
                    PreOrder = string.Join(Environment.NewLine,
    so.Machines.Select(m => !string.IsNullOrEmpty(m.PreOrder)
        ? Regex.Replace(m.PreOrder, "<.*?>", string.Empty)
        : "N/A")),

                    Scope = string.Join(Environment.NewLine,
    so.Machines.Select(m => !string.IsNullOrEmpty(m.Scope)
        ? Regex.Replace(m.Scope, "<.*?>", string.Empty)
        : "N/A")),

                    ActualAssemblyHours = string.Join(Environment.NewLine, so.Machines.Select(m => m.ActualAssemblyHours != null
						? $"{m.ActualAssemblyHours} hrs"
						: "N/A")),
					ReworkHours = string.Join(Environment.NewLine, so.Machines.Select(m => m.ReworkHours != null
						? $"{m.ReworkHours} hrs"
						: "N/A")),
					NamePlate = string.Join(Environment.NewLine, so.Machines.Select(m => m.Nameplate?.ToString() ?? "N/A")),
					Notes = !string.IsNullOrEmpty(so.Comments)
						? Regex.Replace(so.Comments, "<.*?>", string.Empty)
						: " "
				})
				.ToList();

			if (schedules.Count == 0)
			{
				return NotFound("No data available to export.");
			}

			using (ExcelPackage excel = new ExcelPackage())
			{
				var workSheet = excel.Workbook.Worksheets.Add("Machine Schedules");

				// Add header
				workSheet.Cells[1, 1].Value = "Machine Schedule Report";
				using (ExcelRange title = workSheet.Cells[1, 1, 1, 24])
				{
					title.Merge = true;
					title.Style.Font.Bold = true;
					title.Style.Font.Size = 18;
					title.Style.Fill.PatternType = ExcelFillStyle.Solid;
					title.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 102, 204)); // Dark blue background
					title.Style.Font.Color.SetColor(Color.White); // White text
					title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				}

				// Add timestamp
				DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
					TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
				using (ExcelRange timestamp = workSheet.Cells[2, 1, 2, 24])
				{
					timestamp.Merge = true;
					timestamp.Value = "Created: " + localDate.ToString("yyyy-MM-dd HH:mm");
					timestamp.Style.Font.Bold = true;
					timestamp.Style.Font.Size = 14;
					timestamp.Style.Fill.PatternType = ExcelFillStyle.Solid;
					timestamp.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
					timestamp.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				}

				// Column headers (all LightBlue)
				string[] headers = {
			"Sales Order", "Order Date", "Customer Name", "Machine Description", "Serial Number", "Production Order Number",
			"Package Release Expected", "Package Release Actual", "Vendors", "PO Number", "PO Due Date", "Delivery Date",
			"Media", "Spare Parts", "Base", "Air Seal", "Coating Lining", "Disassembly", "Notes/Comments"
		};

				for (int i = 0; i < headers.Length; i++)
				{
					workSheet.Cells[3, i + 1].Value = headers[i];
					workSheet.Cells[3, i + 1].Style.Font.Bold = true;
					workSheet.Cells[3, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Cells[3, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
					workSheet.Cells[3, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
				}

				// Merge and style the "Notes/Comments" header
				using (ExcelRange notesHeader = workSheet.Cells[3, 19, 3, 24])
				{
					notesHeader.Value = "Notes/Comments";
					notesHeader.Merge = true;
					notesHeader.Style.Font.Bold = true;
					notesHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
					notesHeader.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
					notesHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					notesHeader.Style.Border.BorderAround(ExcelBorderStyle.Thin);
				}

				// Add individual headers for the note-related columns with enhanced styling
				string[] noteHeaders = { "PreOrder", "Scope", "Actual Hours", "Budget Hours", "NamePlate", "Comment for Sales Orders" };
				Color[] noteColors = {
			Color.FromArgb(255, 204, 0),   // Yellow
            Color.FromArgb(255, 153, 51),  // Orange
            Color.FromArgb(153, 204, 255), // Light Blue
            Color.FromArgb(204, 255, 204), // Light Green
            Color.FromArgb(255, 204, 204), // Light Pink
            Color.FromArgb(204, 204, 255)  // Light Purple
        };

				for (int i = 0; i < noteHeaders.Length; i++)
				{
					workSheet.Cells[4, 19 + i].Value = noteHeaders[i];
					workSheet.Cells[4, 19 + i].Style.Font.Bold = true;
					workSheet.Cells[4, 19 + i].Style.Font.Size = 11;
					workSheet.Cells[4, 19 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Cells[4, 19 + i].Style.Fill.BackgroundColor.SetColor(noteColors[i]);
					workSheet.Cells[4, 19 + i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
					workSheet.Cells[4, 19 + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				}

				// Load data starting from row 5
				workSheet.Cells[5, 1].LoadFromCollection(schedules, false);

				// Add borders to all data cells
				using (var range = workSheet.Cells[5, 1, 5 + schedules.Count - 1, 24])
				{
					range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
					range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
					range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
					range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
				}

				// Enable text wrapping for columns with line breaks
				int[] wrapTextColumns = { 4, 5, 6, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };
				foreach (int col in wrapTextColumns)
				{
					workSheet.Column(col).Style.WrapText = true;
				}

				// AutoFit and manual column adjustments
				workSheet.Cells.AutoFitColumns();
				workSheet.Column(3).Width = 25;
				workSheet.Column(4).Width = 20;
				workSheet.Column(5).Width = 20;
				workSheet.Column(6).Width = 15;
				workSheet.Column(7).Width = 15;
				workSheet.Column(8).Width = 15;
				workSheet.Column(9).Width = 30;
				workSheet.Column(10).Width = 12;
				workSheet.Column(19).Width = 15; // PreOrder
				workSheet.Column(20).Width = 15; // Scope
				workSheet.Column(21).Width = 15; // Actual Hours
				workSheet.Column(22).Width = 15; // Budget Hours
				workSheet.Column(23).Width = 15; // NamePlate
				workSheet.Column(24).Width = 30; // Comment for Sales Orders

				// Freeze the top 4 rows for easier scrolling
				workSheet.View.FreezePanes(5, 1);

				try
				{
					Byte[] theData = excel.GetAsByteArray();
					return File(theData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Machine Schedule.xlsx");
				}
				catch (Exception)
				{
					return BadRequest("Could not build and download the file.");
				}
			}
		}

		//Return customer name suggestions
		public async Task<JsonResult> GetCompanyName(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<string>());
            }

            var companyNames = await _context.Customers
                .Where(c => c.CompanyName.Contains(term))
                .Select(c => c.CompanyName)
                .Take(10)
                .ToListAsync();

            return Json(companyNames);
        }

        public async Task<IActionResult> Complete(int id)
        {
            var salesOrder = await _context.SalesOrders
                .FirstOrDefaultAsync(m => m.ID == id);

            if (salesOrder == null)
            {
                return NotFound();
            }

            // Set the status to Completed
            salesOrder.Status = Status.Completed;
            _context.Update(salesOrder);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Message"] = "Sales Order has been marked as completed";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An Error Occured";
            }

            // Redirect back to the Index page after marking as completed
            return RedirectToAction(nameof(Index));
        }

        private void PopulateAssignedSpecialtyData(SalesOrder salesOrder)
        {
            //For this to work, you must have Included the child collection in the parent object
            var allOptions = _context.Engineers;
            var currentOptionsHS = new HashSet<int>(salesOrder.SalesOrderEngineers.Select(b => b.EngineerID));
            //Instead of one list with a boolean, we will make two lists
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            foreach (var s in allOptions)
            {
                if (currentOptionsHS.Contains(s.ID))
                {
                    selected.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.EngineerInitialsB
                    });
                }
                else
                {
                    available.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.EngineerInitialsB
                    });
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }
        private void UpdateSalesOrderEngineers(string[] selectedOptions, SalesOrder salesOrderToUpdate)
        {
            if (selectedOptions == null)
            {
                salesOrderToUpdate.SalesOrderEngineers = new List<SalesOrderEngineer>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var currentOptionsHS = new HashSet<int>(salesOrderToUpdate.SalesOrderEngineers.Select(b => b.EngineerID));
            foreach (var s in _context.Engineers)
            {
                if (selectedOptionsHS.Contains(s.ID.ToString()))//it is selected
                {
                    if (!currentOptionsHS.Contains(s.ID))//but not currently in the Doctor's collection - Add it!
                    {
                        salesOrderToUpdate.SalesOrderEngineers.Add(new SalesOrderEngineer
                        {
                            EngineerID = s.ID,
                            SalesOrderID = salesOrderToUpdate.ID
                        });
                    }
                }
                else //not selected
                {
                    if (currentOptionsHS.Contains(s.ID))//but is currently in the Doctor's collection - Remove it!
                    {
                        SalesOrderEngineer? specToRemove = salesOrderToUpdate.SalesOrderEngineers.FirstOrDefault(d => d.EngineerID == s.ID);
                        if (specToRemove != null)
                        {
                            _context.Remove(specToRemove);
                        }
                    }
                }
            }
        }
        private bool SalesOrderExists(int id)
        {
            return _context.SalesOrders.Any(e => e.ID == id);
        }
    }
}
