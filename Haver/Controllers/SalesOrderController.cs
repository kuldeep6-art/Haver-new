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
using System.Reflection.PortableExecutable;

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
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string? SearchString, int? CustomerID,
            string? actionButton, string sortDirection = "asc",  string sortField="OrderNumber")
        {
            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "OrderNumber", "Customer" };

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied


            PopulateDropDownLists();

            var salesOrders = from s in _context.SalesOrders
                              .Include(s => s.Customer)
                              .AsNoTracking()
                              select s;

            //Add as many filters as needed
            if (CustomerID.HasValue)
            {
               salesOrders = salesOrders.Where(p => p.CustomerID == CustomerID);
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                salesOrders = salesOrders.Where(p => p.OrderNumber.Contains(SearchString));
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
                        .OrderByDescending(p => p.Customer.CompanyName);
                }
                else
                {
                    salesOrders = salesOrders
                        .OrderBy(p => p.Customer.CompanyName);
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
                .Include(s => s.PackageRelease)
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
        public async Task<IActionResult> Create([Bind("ID,OrderNumber,SoDate,Price,ShippingTerms,AppDwgRcd,DwgIsDt,CustomerID,Comments")] SalesOrder salesOrder)
        {
            try
            {
				if (ModelState.IsValid)
				{
					_context.Add(salesOrder);
					await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "SalesOrderProcurement", new { SalesOrderID = salesOrder.ID });
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
			var salesOrderToUpdate = await _context.SalesOrders.FirstOrDefaultAsync(p => p.ID == id);

			if (salesOrderToUpdate == null)
			{
				return NotFound();
			}

			if (await TryUpdateModelAsync<SalesOrder>(salesOrderToUpdate, "",
			   p => p.OrderNumber, p => p.SoDate, p => p.Price, p => p.ShippingTerms,
			   p => p.AppDwgRcd, p => p.DwgIsDt, p => p.CustomerID,p => p.Comments))
			{
				try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "SalesOrderProcurement", new { SalesOrderID = salesOrderToUpdate.ID });
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
                .AsNoTracking()
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

        private bool SalesOrderExists(int id)
        {
            return _context.SalesOrders.Any(e => e.ID == id);
        }

		private void PopulateDropDownLists(SalesOrder? salesOrder = null)
		{
			ViewData["CustomerID"] = new SelectList(_context.Customers, "ID", "CompanyName");
		}
	}
}
