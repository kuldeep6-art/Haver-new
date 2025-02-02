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
    public class ProcurementController : Controller
    {
        private readonly HaverContext _context;

        public ProcurementController(HaverContext context)
        {
            _context = context;
        }

        // GET: Procurement
        public async Task<IActionResult> Index()
        {
            var haverContext = _context.Procurements.Include(p => p.SalesOrder).Include(p => p.Vendor);
            return View(await haverContext.ToListAsync());
        }

        // GET: Procurement/Details/5
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

        // GET: Procurement/Create
        public IActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        }

        // POST: Procurement/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,VendorID,SalesOrderID,PONumber,ExpDueDate,DeliveryDate")] Procurement procurement)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(procurement);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {

                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
         
            }


            PopulateDropDownLists(procurement);
            return View(procurement);
        }

        // GET: Procurement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procurement = await _context.Procurements.FindAsync(id);
            if (procurement == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(procurement);
            return View(procurement);
        }

        // POST: Procurement/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,VendorID,SalesOrderID,PONumber,ExpDueDate,DeliveryDate")] Procurement procurement)
        {

            //Go get the customer to update
            var procurementToUpdate = await _context.Procurements.FirstOrDefaultAsync(c => c.ID == id);

            if (procurementToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Procurement>(procurementToUpdate, "",
                p => p.VendorID, p => p.SalesOrderID, p => p.PONumber, p => p.ExpDueDate,
               p => p.DeliveryDate))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { procurementToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProcurementExists(procurement.ID))
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
            PopulateDropDownLists(procurement);
            return View(procurement);
        }

        // GET: Procurement/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Procurement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {


            var procurement = await _context.Procurements.FindAsync(id);

            try
            {
                if (procurement != null)
                {
                    _context.Procurements.Remove(procurement);
                }

                await _context.SaveChangesAsync();
                var returnUrl = ViewData["returnURL"]?.ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction(nameof(Index));
                }
                return Redirect(returnUrl);
            }
            catch (DbUpdateException dex)
            {

       
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                
            }
            return View(procurement);
        }


       

        private SelectList SalesOrderSelectList(int? selectedId)
        {
            return new SelectList(_context.SalesOrders
                .OrderBy(d => d.OrderNumber), "ID", "OrderNumber", selectedId);
        }
        private SelectList VendorList(int? selectedId)
        {
            return new SelectList(_context
                .Vendors
                .OrderBy(m => m.Name), "ID", "Name", selectedId);
        }

        private void PopulateDropDownLists(Procurement? procurement = null)
        {
            ViewData["SalesOrderID"] = SalesOrderSelectList(procurement?.SalesOrderID);
            ViewData["VendorID"] = VendorList(procurement?.SalesOrderID);
        }
        private bool ProcurementExists(int id)
        {
            return _context.Procurements.Any(e => e.ID == id);
        }
    }
}
