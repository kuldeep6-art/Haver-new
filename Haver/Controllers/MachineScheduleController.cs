using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using haver.Data;
using haver.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Numerics;
using Microsoft.VisualBasic;
using System.Reflection.PortableExecutable;
using System.Reflection;
using haver.Utilities;
using haver.CustomControllers;

namespace haver.Controllers
{
    public class MachineScheduleController : ElephantController
    {
        private readonly HaverContext _context;

        public MachineScheduleController(HaverContext context)
        {
            _context = context;
        }

        // GET: MachineSchedule
        public async Task<IActionResult> Index(int? page, int? pageSizeID, int? MachineID, string? actionButton, string sortDirection = "asc", string sortField = "Machine")
        {
            //List of sort options.
            string[] sortOptions = new[] { "Machine", "StartDate", "DueDate", "EndDate","PackageRDate","PODueDate","DeliveryDate" };

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;

            PopulateDropDownLists();

            var machineSchedules = from m in _context.MachineSchedules
                .Include(m => m.Machine)
                .Include(n => n.Note)
                .Include(e => e.MachineScheduleEngineers).ThenInclude(e => e.Engineer)
                .AsNoTracking()
                select m;

            //Add as many filters as needed
            if (MachineID.HasValue)
            {
                machineSchedules = machineSchedules.Where(p => p.MachineID == MachineID);
                numberFilters++;
            }

            if (numberFilters != 0)
            {
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
                page = 1;
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
            if (sortField == "StartDate")
            {
                if (sortDirection == "asc")
                {
                    machineSchedules = machineSchedules
                        .OrderByDescending(p => p.StartDate);
                }
                else
                {
                   machineSchedules = machineSchedules
                        .OrderBy(p => p.StartDate);
                }
            }
            else if (sortField == "DueDate")
            {
                if (sortDirection == "asc")
                {
                    machineSchedules = machineSchedules
                        .OrderByDescending(p => p.DueDate);
                }
                else
                {
                    machineSchedules = machineSchedules
                         .OrderBy(p => p.DueDate);
                }
            }
            else if (sortField == "EndDate")
            {
                if (sortDirection == "asc")
                {
                    machineSchedules = machineSchedules
                        .OrderByDescending(p => p.EndDate);
                }
                else
                {
                    machineSchedules = machineSchedules
                         .OrderBy(p => p.EndDate);
                }
            }
            else if (sortField == "PackageRDate")
            {
                if (sortDirection == "asc")
                {
                    machineSchedules = machineSchedules
                        .OrderByDescending(p => p.PackageRDate);
                }
                else
                {
                    machineSchedules = machineSchedules
                         .OrderBy(p => p.PackageRDate);
                }
            }
            else if (sortField == "PODueDate")
            {
                if (sortDirection == "asc")
                {
                    machineSchedules = machineSchedules
                        .OrderByDescending(p => p.PODueDate);
                }
                else
                {
                    machineSchedules = machineSchedules
                         .OrderBy(p => p.PODueDate);
                }
            }
            else if (sortField == "DeliveryDate")
            {
                if (sortDirection == "asc")
                {
                    machineSchedules = machineSchedules
                        .OrderByDescending(p => p.DeliveryDate);
                }
                else
                {
                    machineSchedules = machineSchedules
                         .OrderBy(p => p.DeliveryDate);
                }
            }
            else 
            {
                if (sortDirection == "asc")
                {
                    machineSchedules = machineSchedules
                         .OrderBy(p => p.Machine.Class);
                }
                else
                {
                    machineSchedules = machineSchedules
                        .OrderByDescending(p => p.Machine.Class);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<MachineSchedule>.CreateAsync(machineSchedules.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: MachineSchedule/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machineSchedule = await _context.MachineSchedules
                .Include(m => m.Machine)
                .Include(n => n.Note)
                .Include(p => p.PackageRelease)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (machineSchedule == null)
            {
                return NotFound();
            }

            return View(machineSchedule);
        }

        // GET: MachineSchedule/Create
        public IActionResult Create()
        {
           PopulateDropDownLists();
            return View();
        }

        // POST: MachineSchedule/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,StartDate,DueDate,EndDate,PackageRDate,PODueDate,DeliveryDate,Media,SpareParts,SparePMedia,Base,AirSeal,CoatingLining,Dissembly,MachineID")] MachineSchedule machineSchedule)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(machineSchedule);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { machineSchedule.ID });
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            PopulateDropDownLists(machineSchedule);
                return View(machineSchedule);
            
        }

        // GET: MachineSchedule/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machineSchedule = await _context.MachineSchedules.FindAsync(id);
            if (machineSchedule == null)
            {
                return NotFound();
            }
            ViewData["MachineID"] = new SelectList(_context.Machines, "ID", "Class", machineSchedule.MachineID);
            return View(machineSchedule);
        }

        // POST: MachineSchedule/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var scheduleToUpdate = await _context.MachineSchedules
                .FirstOrDefaultAsync(p => p.ID == id);
            if (scheduleToUpdate == null)
            {
                return NotFound();
            }
            if (await TryUpdateModelAsync<MachineSchedule>(scheduleToUpdate, "",
                s => s.StartDate, s => s.DueDate, s => s.EndDate, s=>s.PackageRDate,
                s=>s.PODueDate, s=>s.DeliveryDate, s=>s.Media, s=>s.SpareParts,
                s=>s.SparePMedia, s=>s.Base, s => s.AirSeal, s => s.CoatingLining, s => s.Dissembly, s => s.MachineID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { scheduleToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MachineScheduleExists(scheduleToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException )
                {
                    
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");

                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDropDownLists(scheduleToUpdate);
            return View(scheduleToUpdate);
        }

        // GET: MachineSchedule/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machineSchedule = await _context.MachineSchedules
                .Include(m => m.Machine)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (machineSchedule == null)
            {
                return NotFound();
            }

            return View(machineSchedule);
        }

        // POST: MachineSchedule/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var machineSchedule = await _context.MachineSchedules.FindAsync(id);
            try
            {
                if (machineSchedule != null)
                {
                    _context.MachineSchedules.Remove(machineSchedule);
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
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");  
            }
            return View(machineSchedule);

        }

        private void PopulateDropDownLists(MachineSchedule? machineSchedule = null)
        {
            ViewData["MachineID"] = new SelectList(_context.Machines, "ID", "Class", machineSchedule?.MachineID);
        }

        private bool MachineScheduleExists(int id)
        {
            return _context.MachineSchedules.Any(e => e.ID == id);
        }
    }
}
