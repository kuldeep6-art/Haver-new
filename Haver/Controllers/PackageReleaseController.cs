using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using haver.Data;
using haver.Models;
using haver.Utilities;
using static Azure.Core.HttpHeader;
using System.Reflection.PortableExecutable;

namespace haver.Controllers
{
    public class PackageReleaseController : CognizantController
    {
        private readonly HaverContext _context;

        public PackageReleaseController(HaverContext context)
        {
            _context = context;
        }

        // GET: PackageRelease
        public async Task<IActionResult> Index(int? page, int? pageSizeID,string? SearchName,string? SearchNotes,int? MachineScheduleID, string? actionButton, string sortDirection = "asc", string sortField = "PReleaseDateP")
        {
            string[] sortOptions = new[] { "Name", "PReleaseDateP", "PReleaseDateA", "Notes", "MachineSchedule" };
            var packageRelease = from m in _context.PackageReleases
                         .Include(m => m.MachineSchedule)
                         .AsNoTracking()
                          select m;
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            PopulateDropDownLists();

            if (MachineScheduleID.HasValue)
            {
                packageRelease = packageRelease.Where(p => p.MachineScheduleID == MachineScheduleID);
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchName))
            {
                packageRelease = packageRelease.Where(p => p.Name.ToUpper().Contains(SearchName.ToUpper()));
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchNotes))
            {
                packageRelease = packageRelease.Where(p => p.Notes.ToUpper().Contains(SearchNotes.ToUpper()));
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
            if (sortField == "PReleaseDateP")
            {
                if (sortDirection == "asc")
                {
                    packageRelease = packageRelease
                        .OrderByDescending(p => p.PReleaseDateP);
                }
                else
                {
                    packageRelease = packageRelease
                         .OrderBy(p => p.PReleaseDateP);
                }
            }
            else if(sortField=="Name")
            {
                if (sortDirection == "asc")
                {
                    packageRelease = packageRelease
                        .OrderByDescending(p => p.Name);
                }
                else
                {
                    packageRelease = packageRelease
                         .OrderBy(p => p.Name);
                }
            }
            else if (sortField == "Notes")
            {
                if (sortDirection == "asc")
                {
                    packageRelease = packageRelease
                        .OrderByDescending(p => p.Notes);
                }
                else
                {
                    packageRelease = packageRelease
                         .OrderBy(p => p.Notes);
                }
            }
            else if (sortField == "MachineSchedule")
            {
                if (sortDirection == "asc")
                {
                    packageRelease = packageRelease
                        .OrderByDescending(p => p.MachineSchedule);
                }
                else
                {
                    packageRelease = packageRelease
                         .OrderBy(p => p.MachineSchedule);
                }
            }
            else
            {
                if (sortDirection == "asc")
                {
                    packageRelease = packageRelease
                        .OrderByDescending(p => p.PReleaseDateA);
                }
                else
                {
                    packageRelease = packageRelease
                         .OrderBy(p => p.PReleaseDateA);
                }
            }
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<PackageRelease>.CreateAsync(packageRelease.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: PackageRelease/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var packageRelease = await _context.PackageReleases
                .Include(p => p.MachineSchedule)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (packageRelease == null)
            {
                return NotFound();
            }

            return View(packageRelease);
        }

        // GET: PackageRelease/Create
        public IActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        }

        // POST: PackageRelease/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,PReleaseDateP,PReleaseDateA,Notes,MachineScheduleID")] PackageRelease packageRelease)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(packageRelease);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            PopulateDropDownLists(packageRelease);
            return View(packageRelease);
        }

        // GET: PackageRelease/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var packageRelease = await _context.PackageReleases.FindAsync(id);
            if (packageRelease == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(packageRelease);
            return View(packageRelease);
        }

        // POST: PackageRelease/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //Go get the packae to update
            var packageToUpdate = await _context.PackageReleases.FirstOrDefaultAsync(c => c.ID == id);

            if (packageToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<PackageRelease>(packageToUpdate, "",
                p => p.Name, p => p.PReleaseDateP, p => p.PReleaseDateA, p => p.Notes, p => p.MachineScheduleID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PackageReleaseExists(packageToUpdate.ID))
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
            PopulateDropDownLists(packageToUpdate);
            return View(packageToUpdate);
        }

        // GET: PackageRelease/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var packageRelease = await _context.PackageReleases
                .Include(p => p.MachineSchedule)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (packageRelease == null)
            {
                return NotFound();
            }

            return View(packageRelease);
        }

        // POST: PackageRelease/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var packageRelease = await _context.PackageReleases.FindAsync(id);

            try
            {
                if (packageRelease != null)
                {
                    _context.PackageReleases.Remove(packageRelease);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");

            }
            return View(packageRelease);


        }


        private void PopulateDropDownLists(PackageRelease? packageRelease = null)
        {
            ViewData["MachineScheduleID"] = new SelectList(_context.MachineSchedules, "ID", "ID");
        }
        private bool PackageReleaseExists(int id)
        {
            return _context.PackageReleases.Any(e => e.ID == id);
        }
    }
}
