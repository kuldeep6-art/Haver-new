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

namespace haver.Controllers
{
    public class PackageReleaseController : ElephantController
    {
        private readonly HaverContext _context;

        public PackageReleaseController(HaverContext context)
        {
            _context = context;
        }

        // GET: PackageRelease
        public async Task<IActionResult> Index()
        {
            var haverContext = _context.PackageReleases
                .Include(p => p.MachineSchedule);
            return View(await haverContext.ToListAsync());
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
                    return RedirectToAction("Details", new { packageRelease.ID });
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
                    return RedirectToAction("Details", new { packageToUpdate.ID });
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
