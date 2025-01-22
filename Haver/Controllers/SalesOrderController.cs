using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using haver.Data;
using haver.Models;

namespace haver.Controllers
{
    public class SalesOrderController : Controller
    {
        private readonly HaverContext _context;

        public SalesOrderController(HaverContext context)
        {
            _context = context;
        }

        // GET: SalesOrder
        public async Task<IActionResult> Index()
        {
            var haverContext = _context.SalesOrders.Include(s => s.Customer).Include(s => s.MachineSchedule).Include(s => s.Vendor);
            return View(await haverContext.ToListAsync());
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
                    return RedirectToAction(nameof(Index));
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
                    return RedirectToAction(nameof(Index));
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
                return RedirectToAction(nameof(Index));
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
            ViewData["CustomerID"] = new SelectList(_context.Customers, "ID", "CompanyName");
            ViewData["MachineScheduleID"] = new SelectList(_context.MachineSchedules, "ID", "ID");
            ViewData["VendorID"] = new SelectList(_context.Vendors, "ID", "Email");
        }
        private bool SalesOrderExists(int id)
        {
            return _context.SalesOrders.Any(e => e.ID == id);
        }
    }
}
