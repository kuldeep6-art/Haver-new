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
using GymManagement.Utilities;
using haver.Utilities;

namespace haver.Controllers
{
    public class MachineProcurementController : ElephantController
    {
        private readonly HaverContext _context;

        public MachineProcurementController(HaverContext context)
        {
            _context = context;
        }

        // GET: MachineProcurement
        public async Task<IActionResult> Index(int? MachineID, int? VendorID, int? page, int? pageSizeID, string actionButton,
            string SearchString, string sortDirection = "desc", string sortField = "PoNumber")
        {
            //Get the URL with the last filter, sort and page parameters from THE PATIENTS Index View
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Machine");

            if (!MachineID.HasValue)
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
            string[] sortOptions = new[] { "PoNumber", "Vendor" };

            var appts = from a in _context.Procurements
                        .Include(a => a.Vendor)
                        .Include(a => a.Machine)
                        where a.MachineID == MachineID.GetValueOrDefault()
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
            Machine? machine = await _context.Machines
                .Include(s => s.MachineType)
                .Include(s => s.SalesOrder)
                .Where(p => p.ID == MachineID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefaultAsync();

            ViewBag.Machine = machine;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Procurement>.CreateAsync(appts.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: MachineProcurement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procurement = await _context.Procurements
                .Include(p => p.Machine)
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (procurement == null)
            {
                return NotFound();
            }

            return View(procurement);
        }

        // GET: MachineProcurement/Create
        public IActionResult Create()
        {
           PopulateDropDownLists();
            return View();
        }

        // POST: MachineProcurement/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,VendorID,MachineID,PONumber,ExpDueDate,PODueDate,PORcd,QualityICom,NcrRaised")] Procurement procurement)
        {
            if (ModelState.IsValid)
            {
                _context.Add(procurement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
           PopulateDropDownLists(procurement);
            return View(procurement);
        }

        // GET: MachineProcurement/Edit/5
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

        // POST: MachineProcurement/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,VendorID,MachineID,PONumber,ExpDueDate,PODueDate,PORcd,QualityICom,NcrRaised")] Procurement procurement)
        {
            if (id != procurement.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(procurement);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            PopulateDropDownLists(procurement);
            return View(procurement);
        }

        // GET: MachineProcurement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procurement = await _context.Procurements
                .Include(p => p.Machine)
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (procurement == null)
            {
                return NotFound();
            }

            return View(procurement);
        }

        // POST: MachineProcurement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var procurement = await _context.Procurements.FindAsync(id);
            if (procurement != null)
            {
                _context.Procurements.Remove(procurement);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
