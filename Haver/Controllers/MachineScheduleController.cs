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

namespace haver.Controllers
{
    public class MachineScheduleController : Controller
    {
        private readonly HaverContext _context;

        public MachineScheduleController(HaverContext context)
        {
            _context = context;
        }

        // GET: MachineSchedule
        public async Task<IActionResult> Index()
        {
            var haverContext = _context.MachineSchedules.Include(m => m.Machine);
            return View(await haverContext.ToListAsync());
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
            ViewData["MachineID"] = new SelectList(_context.Machines, "ID", "Class");
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
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            ViewData["MachineID"] = new SelectList(_context.Machines, "ID", "Class", machineSchedule.MachineID);
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
                s => s.StartDate, s => s.DueDate, s => s.EndDate, s=>s.PackageRDate, s=>s.PODueDate, s=>s.DeliveryDate, s=>s.Media, s=>s.SpareParts, s=>s.SparePMedia, s=>s.Base, s => s.AirSeal, s => s.CoatingLining, s => s.Dissembly, s => s.MachineID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    var returnUrl = ViewData["returnURL"]?.ToString();
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    return Redirect(returnUrl);
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
                catch (DbUpdateException dex)
                {
                    string message = dex.GetBaseException().Message;
                    if (message.Contains("UNIQUE") && message.Contains("Email"))
                    {
                        ModelState.AddModelError("Email", "Unable to save changes. Remember, " +
                            "you cannot have duplicate Email addresses for Instructors.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MachineID"] = new SelectList(_context.Machines, "ID", "Class", scheduleToUpdate.MachineID);
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
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Instructor. Remember, you cannot delete a Instructor that teaches Group Classes.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(machineSchedule);

        }

        private bool MachineScheduleExists(int id)
        {
            return _context.MachineSchedules.Any(e => e.ID == id);
        }
    }
}
