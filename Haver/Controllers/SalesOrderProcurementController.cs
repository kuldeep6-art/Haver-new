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
using GymManagement.Utilities;
using haver.Utilities;

namespace haver.Controllers
{
    public class SalesOrderProcurementController : ElephantController
    {
        private readonly HaverContext _context;

        public SalesOrderProcurementController(HaverContext context)
        {
            _context = context;
        }

        // GET: SalesOrderProcurement
        public async Task<IActionResult> Index(int? SalesOrderID, int? VendorID, int? page, int? pageSizeID, string actionButton,
            string SearchString, string sortDirection = "desc", string sortField = "PoNumber")
        {

            //Get the URL with the last filter, sort and page parameters from THE PATIENTS Index View
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "SalesOrder");

            if (!SalesOrderID.HasValue)
            {
                //Go back to the proper return URL for the Patients controller
                return Redirect(ViewData["returnURL"].ToString());
            }

            PopulateDropDownLists();

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "PoNumber", "Vendor"};

            var appts = from a in _context.Procurements
                        .Include(a => a.Vendor)
                        .Include(a => a.SalesOrder)
                        where a.SalesOrderID == SalesOrderID.GetValueOrDefault()
                        select a;

            if (VendorID.HasValue)
            {
                appts = appts.Where(p => p.VendorID == VendorID);
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                appts = appts.Where(p => p.PONumber.ToUpper().Contains(SearchString.ToUpper()));
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
                //@ViewData["ShowFilter"] = " show";
            }

            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted so lets sort!
            {
                page = 1;//Reset back to first page when sorting or filtering

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            //Now we know which field and direction to sort by.
            if (sortField == "Vendor")
            {
                if (sortDirection == "asc")
                {
                    appts = appts
                        .OrderBy(p => p.Vendor.Name);
                }
                else
                {
                    appts = appts
                        .OrderByDescending(p => p.Vendor.Name);
                }
            }

            else //Appointment Date
            {
                if (sortDirection == "asc")
                {
                    appts = appts
                        .OrderByDescending(p => p.PONumber);
                }
                else
                {
                    appts = appts
                        .OrderBy(p => p.PONumber);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Now get the MASTER record, the patient, so it can be displayed at the top of the screen
            SalesOrder? salesOrder = await _context.SalesOrders
                .Include(s => s.Customer)
                .Include(s => s.PackageRelease)
                .Where(p => p.ID == SalesOrderID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefaultAsync();

            ViewBag.SalesOrder = salesOrder;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Procurement>.CreateAsync(appts.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: SalesOrderProcurement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procurement = await _context.Procurements
                .Include(p => p.SalesOrder)
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (procurement == null)
            {
                return NotFound();
            }

            return View(procurement);
        }

        // GET: SalesOrderProcurement/Create
        public IActionResult Add(int? SalesOrderID, string OrderNumber)
        {
            if (!SalesOrderID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }

            ViewData["OrderNumber"] = OrderNumber;
            Procurement a = new Procurement()
            {
                SalesOrderID = SalesOrderID.GetValueOrDefault()
            };
            PopulateDropDownLists();
            return View(a);
        }

        // POST: SalesOrderProcurement/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("ID,VendorID,SalesOrderID,PONumber,ExpDueDate,DeliveryDate")] Procurement procurement, string OrderNumber)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    _context.Add(procurement);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                    "persists see your system administrator.");
            }

            PopulateDropDownLists(procurement);
            ViewData["OrderNumber"] = OrderNumber;
            return View(procurement);
        }

        // GET: SalesOrderProcurement/Edit/5
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || _context.Procurements == null)
            {
                return NotFound();
            }

            var procurement = await _context.Procurements
               .Include(a => a.Vendor)
               .Include(a => a.SalesOrder)
               .AsNoTracking()
               .FirstOrDefaultAsync(m => m.ID == id);
            if (procurement == null)
            {
                return NotFound();
            }

            PopulateDropDownLists(procurement);
            return View(procurement);
        }

        // POST: SalesOrderProcurement/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id)
        {
            var procurementToUpdate = await _context.Procurements
                .Include(a => a.Vendor)
                .Include(a => a.SalesOrder)
                .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (procurementToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Procurement>(procurementToUpdate, "",
                a => a.VendorID, a => a.SalesOrderID, a => a.PONumber, a => a.ExpDueDate,
                a => a.DeliveryDate))
            {
                try
                {
                    _context.Update(procurementToUpdate);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProcurementExists(procurementToUpdate.ID))
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
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                        "persists see your system administrator.");
                }

            }
            PopulateDropDownLists(procurementToUpdate);
            return View(procurementToUpdate);
        }

        // GET: SalesOrderProcurement/Delete/5
        public async Task<IActionResult> Remove(int? id)
        {
            if (id == null || _context.Procurements == null)
            {
                return NotFound();
            }

            var procurement = await _context.Procurements
                .Include(p => p.SalesOrder)
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (procurement == null)
            {
                return NotFound();
            }

            return View(procurement);
        }

        // POST: SalesOrderProcurement/Delete/5
        [HttpPost, ActionName("Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveConfirmed(int id)
        {
            var procurement = await _context.Procurements
                 .Include(p => p.SalesOrder)
                 .Include(p => p.Vendor)
                 .FirstOrDefaultAsync(m => m.ID == id);

            try
            {
                _context.Procurements.Remove(procurement);
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                    "persists see your system administrator.");
            }
            return View(procurement);
        }

        private SelectList VendorSelectList(int? id)
        {
            var dQuery = from d in _context.Vendors
                         orderby d.Name
                         select d;
            return new SelectList(dQuery, "ID", "Name", id);
        }

        private void PopulateDropDownLists(Procurement? procurement = null)
        {
            ViewData["VendorID"] = VendorSelectList(procurement?.VendorID);
        }

        private bool ProcurementExists(int id)
        {
            return _context.Procurements.Any(e => e.ID == id);
        }
    }
}
