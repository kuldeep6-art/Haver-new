﻿using System;
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
using Microsoft.AspNetCore.Authorization;

namespace haver.Controllers
{
    [Authorize]
    public class SalesOrderController : ElephantController
    {
        private readonly HaverContext _context;

        public SalesOrderController(HaverContext context)
        {
            _context = context;
        }

        // GET: SalesOrder

        //[Authorize(Roles = "Admin,Sales")]
        public async Task<IActionResult> Index(int? page, int? pageSizeID,string status, DateTime? DtString, string? SearchString, string? CString,
            string? actionButton, string sortDirection = "asc", string sortField = "OrderNumber")
        {
            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Order Number", "Customer", "Order Date", "Engineers" };

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
                    salesOrders = salesOrders.Where(so => so.IsDraft == true);
                    break;
                default:
                    // Active tab shows non-archived and non-completed
                    salesOrders = salesOrders.Where(so => so.Status != Status.Archived && so.Status != Status.Completed && so.IsDraft == false);
                    break;
            }

            if (!String.IsNullOrEmpty(SearchString))
            {
                salesOrders = salesOrders.Where(p => p.OrderNumber.Contains(SearchString));
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(CString))
            {
                salesOrders = salesOrders.Where(p => p.CompanyName.ToUpper().Contains(CString.ToUpper()));
                numberFilters++;
            }
            if (DtString.HasValue)
            {
                DateTime searchDate = DtString.Value.Date;
                salesOrders = salesOrders.Where(s => s.SoDate.HasValue &&
                                                     s.SoDate.Value.Year == searchDate.Year &&
                                                     s.SoDate.Value.Month == searchDate.Month &&
                                                     s.SoDate.Value.Day == searchDate.Day);
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

            if (String.IsNullOrEmpty(actionButton))
            {
                salesOrders = salesOrders.OrderByDescending(p => p.CreatedOn);
            }
            else
            {
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

                else if (sortField == "Order Numer")
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
				else if (sortField == "Order Date")
				{
					if (sortDirection == "asc")
					{
						salesOrders = salesOrders
							.OrderBy(p => p.SoDate);
					}
					else
					{
						salesOrders = salesOrders
							.OrderByDescending(p => p.SoDate);
					}
				}
				else
                {
                    if (sortDirection == "asc")
                    {
                        salesOrders = salesOrders
                            .OrderBy(p => p.CreatedOn);
                    }
                    else
                    {
                        salesOrders = salesOrders
                            .OrderByDescending(p => p.CreatedOn);
                    }
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

		//[Authorize(Roles = "Admin,Sales")]

		// GET: SalesOrder/Details/5
		public async Task<IActionResult> Details(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var salesOrder = await _context.SalesOrders
                //.Include(s => s.Customer)
                .Include(s => s.SalesOrderEngineers).ThenInclude(s => s.Engineer)
                .Include(s => s.Machines)
                    .ThenInclude(m => m.MachineType) // 
                .Include(s => s.Machines)
                    .ThenInclude(m => m.Procurements) 
                        .ThenInclude(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (salesOrder == null)
            {
                return NotFound();
            }

            //ViewBag.SalesOrderID = salesOrder.ID;
            ViewBag.MachineTypeID = MachineTypeSelectList();
            ViewBag.SalesOrderID = SalesOrderSelectList();

            return View(salesOrder);
        }


		[Authorize(Roles = "Admin,Sales")]

		// GET: SalesOrder/Create
		public IActionResult Create()
        {
            var saleOrder = new SalesOrder();

            SalesOrder salesOrder = new SalesOrder();
            PopulateAssignedSpecialtyData(salesOrder);
           // PopulateDropDownLists();

            //Fetch available engineers from the database
            ViewBag.EngineersList = new MultiSelectList(_context.Engineers, "ID", "EngineerInitialsB");
            return View(saleOrder);
        }

        // POST: SalesOrder/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,Sales")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,OrderNumber,CompanyName,SoDate,Price,Currency,ShippingTerms,AppDwgExp,AppDwgRel,AppDwgRet," +
    "PreOExp,PreORel,EngPExp,EngPRel,DelDt,Comments,Status,IsDraft")]
    SalesOrder salesOrder, string[] selectedOptions, int[] selectedEngineers)
        {
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CompanyName.ToUpper() == salesOrder.CompanyName.ToUpper());
                if (customer == null)
                {
                    customer = new Customer { CompanyName = salesOrder.CompanyName };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }

                UpdateSalesOrderEngineers(selectedOptions, salesOrder);

                if (ModelState.IsValid)
                {
                    _context.Add(salesOrder);
                    await _context.SaveChangesAsync();

                    await LogActivity($"Sales Order '{salesOrder.OrderNumber}' created for customer '{salesOrder.CompanyName}'");

                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Sales Order created successfully";
                    return RedirectToAction("Details", new { id = salesOrder.ID });
                }
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again later.");
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: SalesOrders.OrderNumber"))
                {
                    ModelState.AddModelError("OrderNumber", "Unable to save changes. Duplicate Order Number is not allowed.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again later.");
                }
            }

            PopulateAssignedSpecialtyData(salesOrder);
            return View(salesOrder);
        }



        [Authorize(Roles = "Admin,Sales")]

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
        [Authorize(Roles = "Admin,Sales")]
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, string[] selectedOptions, Byte[] RowVersion)
        {
            var salesOrderToUpdate = await _context.SalesOrders
                .Include(d => d.SalesOrderEngineers).ThenInclude(d => d.Engineer)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (salesOrderToUpdate == null)
            {
                return NotFound();
            }

            UpdateSalesOrderEngineers(selectedOptions, salesOrderToUpdate);

            _context.Entry(salesOrderToUpdate).Property("RowVersion").OriginalValue = RowVersion;
            var debugState = _context.Entry(salesOrderToUpdate).State;
            Console.WriteLine($"Entity state before SaveChanges: {debugState}");


            if (await TryUpdateModelAsync<SalesOrder>(salesOrderToUpdate, "",
                p => p.OrderNumber, p => p.SoDate, p => p.Price, p => p.Currency, p => p.ShippingTerms,
                p => p.AppDwgExp, p => p.AppDwgRel, p => p.AppDwgRet, p => p.PreOExp, p => p.PreORel,
                p => p.EngPExp, p => p.EngPRel, p => p.DelDt, p => p.CompanyName, p => p.Comments, p => p.IsDraft))
            {
                try
                {
                    await _context.SaveChangesAsync();

                    await LogActivity($"Sales Order '{salesOrderToUpdate.OrderNumber}' for customer '{salesOrderToUpdate.CompanyName}' was updated");

                    var relatedGantts = await _context.GanttDatas
                        .Where(g => g.SalesOrderID == salesOrderToUpdate.ID)
                        .ToListAsync();

                    foreach (var gantt in relatedGantts)
                    {
                        gantt.AppDExp = salesOrderToUpdate.AppDwgExp;
                        gantt.AppDRcd = salesOrderToUpdate.AppDwgRel;
                        gantt.EngExpected = salesOrderToUpdate.EngPExp;
                        gantt.EngReleased = salesOrderToUpdate.EngPRel;
                    }

                    await _context.SaveChangesAsync();
                    await LogActivity($"Gantt milestones updated for Sales Order '{salesOrderToUpdate.OrderNumber}'");

                    TempData["Message"] = "Sales Order successfully updated";
                    return RedirectToAction("Details", new { id = salesOrderToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (SalesOrder)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();

                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("", "Unable to save changes. The Sales Order was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (SalesOrder)databaseEntry.ToObject();

                        if (databaseValues.OrderNumber != clientValues.OrderNumber)
                            ModelState.AddModelError("OrderNumber", $"Current value: {databaseValues.OrderNumber}");
                        if (databaseValues.CompanyName != clientValues.CompanyName)
                            ModelState.AddModelError("CompanyName", $"Current value: {databaseValues.CompanyName}");
                        if (databaseValues.SoDate != clientValues.SoDate)
                            ModelState.AddModelError("SoDate", $"Current value: {databaseValues.SoDate:d}");
                        if (databaseValues.Price != clientValues.Price)
                            ModelState.AddModelError("Price", $"Current value: {databaseValues.Price}");
                        if (databaseValues.Currency != clientValues.Currency)
                            ModelState.AddModelError("Currency", $"Current value: {databaseValues.Currency}");
                        if (databaseValues.ShippingTerms != clientValues.ShippingTerms)
                            ModelState.AddModelError("ShippingTerms", $"Current value: {databaseValues.ShippingTerms}");
                        if (databaseValues.AppDwgExp != clientValues.AppDwgExp)
                            ModelState.AddModelError("AppDwgExp", $"Current value: {databaseValues.AppDwgExp:d}");
                        if (databaseValues.AppDwgRel != clientValues.AppDwgRel)
                            ModelState.AddModelError("AppDwgRel", $"Current value: {databaseValues.AppDwgRel:d}");
                        if (databaseValues.AppDwgRet != clientValues.AppDwgRet)
                            ModelState.AddModelError("AppDwgRet", $"Current value: {databaseValues.AppDwgRet:d}");
                        if (databaseValues.PreOExp != clientValues.PreOExp)
                            ModelState.AddModelError("PreOExp", $"Current value: {databaseValues.PreOExp:d}");
                        if (databaseValues.PreORel != clientValues.PreORel)
                            ModelState.AddModelError("PreORel", $"Current value: {databaseValues.PreORel:d}");
                        if (databaseValues.EngPExp != clientValues.EngPExp)
                            ModelState.AddModelError("EngPExp", $"Current value: {databaseValues.EngPExp:d}");
                        if (databaseValues.EngPRel != clientValues.EngPRel)
                            ModelState.AddModelError("EngPRel", $"Current value: {databaseValues.EngPRel:d}");
						if (databaseValues.EngPRel != clientValues.DelDt)
							ModelState.AddModelError("DelDt", $"Current value: {databaseValues.DelDt:d}");
						if (databaseValues.Comments != clientValues.Comments)
                            ModelState.AddModelError("Comments", $"Current value: {databaseValues.Comments}");

                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                            + "was modified by another user after you received your values. The edit operation was canceled "
                            + "and the current values in the database have been displayed. If you still want to save your version "
                            + "of this record, click the Save button again. Otherwise, click the Back button.");

                        salesOrderToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
                        ModelState.Remove("RowVersion");
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: SalesOrders.OrderNumber"))
                    {
                        ModelState.AddModelError("OrderNumber", "Duplicate Order Number is not allowed.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again later.");
                    }
                }
            }

            PopulateAssignedSpecialtyData(salesOrderToUpdate);
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
        [Authorize(Roles = "Admin,Sales")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salesOrder = await _context.SalesOrders.FindAsync(id);

            try
            {
                if (salesOrder != null)
                {
                    _context.SalesOrders.Remove(salesOrder);
                    await _context.SaveChangesAsync();

                    await LogActivity($"Sales Order '{salesOrder.OrderNumber}' for customer '{salesOrder.CompanyName}' was deleted");

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete Sales Order. It may be linked to other records.");
                return View(salesOrder);
            }
        }



        //private void PopulateDropDownLists(SalesOrder? salesOrder = null)
        //{
        //	ViewData["CustomerID"] = new SelectList(_context.Customers, "ID", "CompanyName");
        //}





        // GET: SalesOrder/Archive/5
        [Authorize(Roles = "Admin,Sales")]
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
            catch (Exception )
            {
                TempData["Message"] = "An error occured";
            }

            return RedirectToAction(nameof(Index));  
        }

		[Authorize(Roles = "Admin,Sales")]
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

            return RedirectToAction(nameof(Index));  
        }


        //Return customer name suggestions
        public async Task<JsonResult> GetCompanyName(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<string>());
            }

            term = term.ToUpper(); // Convert input to uppercase

            var companyNames = await _context.Customers
                .Where(c => c.CompanyName.ToUpper().Contains(term))
                .Select(c => c.CompanyName) // Keep original casing
                .Take(10)
                .ToListAsync();

            return Json(companyNames);
        }

        ////Return machine model suggestions
        public async Task<JsonResult> GetMachineModel(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<string>());
            }

            term = term.ToUpper(); // Convert input to uppercase

            var machineModels = await _context.MachineTypes
                .Where(c => c.Description.ToUpper().Contains(term))
                .Select(c => c.Description) // Keep original casing
                .Take(10)
                .ToListAsync();

            return Json(machineModels);
        }


        [Authorize(Roles = "Admin,Sales")]
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

        [Authorize(Roles = "Admin,Sales")]
        public async Task<IActionResult> UnComplete(int id)
        {
            var salesOrder = await _context.SalesOrders
                .FirstOrDefaultAsync(m => m.ID == id);

            if (salesOrder == null)
            {
                return NotFound();
            }

            // Set the status to Completed
            salesOrder.Status = Status.InProgress;
            _context.Update(salesOrder);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Message"] = "Sales Order has been unfinalized";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An Error Occured";
            }

            // Redirect back to the Index page after marking as completed
            return RedirectToAction(nameof(Index));
        }

        private SelectList MachineTypeSelectList(int? selectedId = null)
        {
            //var machineTypes = from mt in _context.MachineTypes
            //                   orderby mt.Class
            //                   select new
            //                   {
            //                       ID = mt.ID,
            //                       DisplayText = $"{mt.Class} | {mt.Deck} | {mt.Size}"
            //                   };

            //return new SelectList(machineTypes, "ID", "DisplayText", selectedId);

            return new SelectList(_context
              .MachineTypes
              .AsEnumerable()
              .OrderBy(m => m.Description),
              "ID",
              "Description",
              selectedId);
        }


        private SelectList SalesOrderSelectList(int? selectedId = null)
        {
            //var salesOrders = from so in _context.SalesOrders
            //                  orderby so.OrderNumber
            //                  select new
            //                  {
            //                      ID = so.ID,
            //                      DisplayText = so.OrderNumber + " | " + so.CompanyName
            //                  };

            //return new SelectList(salesOrders, "ID", "DisplayText", selectedId);
            return new SelectList(_context
             .SalesOrders
             .AsEnumerable()
             .OrderBy(m => m.OrderNumber),
             "ID",
             "MachineOrderDetail",
             selectedId);
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
                    if (!currentOptionsHS.Contains(s.ID))
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
                    if (currentOptionsHS.Contains(s.ID))
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
        private async Task LogActivity(string message)
        {
            string userName = User.Identity?.Name ?? "Unknown User";
            _context.ActivityLogs.Add(new ActivityLog
            {
                Message = $"{message} by {userName}.",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        private bool SalesOrderExists(int id)
        {
            return _context.SalesOrders.Any(e => e.ID == id);
        }
    }
}
