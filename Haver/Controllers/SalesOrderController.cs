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
        public async Task<IActionResult> Index(int? page, int? pageSizeID, 
            string? SearchOrder, string? SearchPO, string? actionButton, string sortDirection = "asc", string sortField = "OrderNumber")
        {
            string[] sortOptions = new[] { "OrderNumber", "SoDate", "PoNumber" };


            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;

            var salesOrders =  from s in _context.SalesOrders
                         .Include(s => s.Customer)
                .Include(s => s.MachineSchedule)
                .Include(s => s.Vendor)
                select s;

            if (!String.IsNullOrEmpty(SearchOrder))
            {
                salesOrders = salesOrders.Where(p => p.OrderNumber.Contains(SearchOrder));
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchPO))
            {
                salesOrders = salesOrders.Where(p => p.PoNumber.Contains(SearchPO));
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
            if (sortField == "SoDate")
            {
                if (sortDirection == "asc")
                {
                    salesOrders = salesOrders
                        .OrderByDescending(p => p.SoDate);
                }
                else
                {
                    salesOrders = salesOrders
                        .OrderBy(p => p.SoDate);
                }
            }
            else   //Now we know which field and direction to sort by
            if (sortField == "PoNumber")
            {
                if (sortDirection == "asc")
                {
                    salesOrders = salesOrders
                        .OrderByDescending(p => p.PoNumber);
                }
                else
                {
                    salesOrders = salesOrders
                        .OrderBy(p => p.PoNumber);
                }
            }


            else 
            {
                if (sortDirection == "asc")
                {
                    salesOrders = salesOrders
                        .OrderByDescending(p => p.OrderNumber);
                }
                else
                {
                    salesOrders = salesOrders
                        .OrderBy(p => p.OrderNumber);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<SalesOrder>.CreateAsync(salesOrders.AsNoTracking(), page ?? 1, pageSize);

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
                .Include(s => s.Customer)
                .Include(s => s.MachineSchedule)
                .Include(s => s.Vendor)
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
           
            PopulateDropDownLists();
            return View();
        }

        // POST: SalesOrder/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,OrderNumber,SoDate,Price,ShippingTerms,AppDwgRcd,DwgIsDt,PoNumber,CustomerID,VendorID,MachineScheduleID")] SalesOrder salesOrder)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(salesOrder);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { salesOrder.ID });
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
            PopulateDropDownLists(salesOrder);
            return View(salesOrder);
        }

        // GET: SalesOrder/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrder = await _context.SalesOrders.FindAsync(id);
            if (salesOrder == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(salesOrder);
            return View(salesOrder);
        }

        // POST: SalesOrder/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //Go get the sales order to update
            var salesOrderToUpdate = await _context.SalesOrders.FirstOrDefaultAsync(p => p.ID == id);

            if (salesOrderToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<SalesOrder>(salesOrderToUpdate, "",
               p => p.OrderNumber, p => p.SoDate, p => p.Price, p => p.ShippingTerms, p => p.ShippingTerms,
               p => p.AppDwgRcd, p => p.DwgIsDt, p => p.PoNumber, p => p.CustomerID, p => p.VendorID,p => p.MachineScheduleID))
            {
                try
                {
                    await _context.SaveChangesAsync();
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
            PopulateDropDownLists(salesOrderToUpdate);
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
                .Include(s => s.Customer)
                .Include(s => s.MachineSchedule)
                .Include(s => s.Vendor)
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
            var salesOrder = await _context.SalesOrders
                .Include(s => s.Customer)
                .Include(s => s.MachineSchedule)
                .Include(s => s.Vendor)
                .FirstOrDefaultAsync(m => m.ID == id);

            try
            {
                if (salesOrder != null)
                {
                    _context.SalesOrders.Remove(salesOrder);
                }
                await _context.SaveChangesAsync();
                var returnUrl = ViewData["returnURL"]?.ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction(nameof(Index));
                }
                return Redirect(returnUrl);
            }
            catch (DbUpdateException)
            {
                //Note: there is really no reason a delete should fail if you can "talk" to the database.
                ModelState.AddModelError("", "Unable to delete record. Try again, and if the problem persists see your system administrator.");
            }
            return View(salesOrder);


        }


        private void PopulateDropDownLists(SalesOrder? salesOrder = null)
        {
            ViewData["CustomerID"] = new SelectList(_context.Customers, "ID", "Summary");
            ViewData["MachineScheduleID"] = new SelectList(_context.MachineSchedules, "ID", "Summary");
            ViewData["VendorID"] = new SelectList(_context.Vendors, "ID", "Name");
        }
        private bool SalesOrderExists(int id)
        {
            return _context.SalesOrders.Any(e => e.ID == id);
        }
    }
}
