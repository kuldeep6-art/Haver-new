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
    public class PackageReleaseController : Controller
    {
        private readonly HaverContext _context;

        public PackageReleaseController(HaverContext context)
        {
            _context = context;
        }

        // GET: PackageRelease
        public async Task<IActionResult> Index()
        {
            var packageReleases = _context.PackageReleases
                .Include(p => p.Schedule)
                .AsNoTracking();
            return View(await packageReleases.ToListAsync());
        }

        // GET: PackageRelease/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var packageRelease = await _context.PackageReleases
                .Include(p => p.Schedule)
                .AsNoTracking()
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
            ViewData["MachineScheduleID"] = new SelectList(_context.MachineSchedules, "ID", "ID");
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
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: PackageRelease.MachineScheduleID"))
                {
                    ModelState.AddModelError("MachineScheduleID", "Unable to save changes, Remenmber you can not have duplicate machine schedules");
                }
                else 
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again and if problem persists contact your system admistrator");
                }
            }         
            ViewData["MachineScheduleID"] = new SelectList(_context.MachineSchedules, "ID", "ID", packageRelease.MachineScheduleID);
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
            ViewData["MachineScheduleID"] = new SelectList(_context.MachineSchedules, "ID", "ID", packageRelease.MachineScheduleID);
            return View(packageRelease);
        }

        // POST: PackageRelease/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //Get the record to be updated and checks if it exists or it is null
            var packageReleaseToUpdate = await _context.PackageReleases.FirstOrDefaultAsync(p => p.ID == id);

            if (packageReleaseToUpdate == null)
            {
                return NotFound();
            }

            //updates with values posted
            if (await TryUpdateModelAsync<PackageRelease>(packageReleaseToUpdate, "", 
                p => p.Name, p => p.PReleaseDateA, p => p.PReleaseDateP, p => p.Notes, 
                p => p.MachineScheduleID))

            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PackageReleaseExists(packageReleaseToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch(DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: PackageRelease.MachineScheduleID"))
                    {
                        ModelState.AddModelError("MachineScheduleID", "Unable to save changes, Remenmber you can not have duplicate machine schedules");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again and if problem persists contact your system admistrator");
                    }
                }
            }
            ViewData["MachineScheduleID"] = new SelectList(_context.MachineSchedules, "ID", "ID", packageReleaseToUpdate.MachineScheduleID);
            return View(packageReleaseToUpdate);
        }

        // GET: PackageRelease/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var packageRelease = await _context.PackageReleases
                .Include(p => p.Schedule)
                .AsNoTracking()
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
            var packageRelease = await _context.PackageReleases
                .Include(p => p.Schedule)
                .FirstOrDefaultAsync(m => m.ID == id);
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
                ModelState.AddModelError("", "Unable to delete record. Try again and if the problem persists contact your sytems administrator.");
            }
            return View(packageRelease);
        }

        private bool PackageReleaseExists(int id)
        {
            return _context.PackageReleases.Any(e => e.ID == id);
        }
    }
}
