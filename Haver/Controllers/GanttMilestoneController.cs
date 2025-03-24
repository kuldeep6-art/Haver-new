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
    public class GanttMilestoneController : Controller
    {
        private readonly HaverContext _context;

        public GanttMilestoneController(HaverContext context)
        {
            _context = context;
        }

        // GET: GanttMilestone
        public async Task<IActionResult> Index()
        {
            var haverContext = _context.GanttMilestones.Include(g => g.GanttTask);
            return View(await haverContext.ToListAsync());
        }

        // GET: GanttMilestone/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganttMilestone = await _context.GanttMilestones
                .Include(g => g.GanttTask)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (ganttMilestone == null)
            {
                return NotFound();
            }

            return View(ganttMilestone);
        }

        // GET: GanttMilestone/Create
        public IActionResult Create()
        {
            ViewData["GanttTaskID"] = new SelectList(_context.GanttTasks, "ID", "ID");
            return View();
        }

        // POST: GanttMilestone/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,GanttTaskID,MilestoneName,Progress,DateCompleted")] GanttMilestone ganttMilestone)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ganttMilestone);
                await _context.SaveChangesAsync();

                _context.ActivityLogs.Add(new ActivityLog
                {
                    Message = $"Milestone '{ganttMilestone.MilestoneName}' created for Task ID {ganttMilestone.GanttTaskID}.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["GanttTaskID"] = new SelectList(_context.GanttTasks, "ID", "ID", ganttMilestone.GanttTaskID);
            return View(ganttMilestone);
        }


        // GET: GanttMilestone/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganttMilestone = await _context.GanttMilestones.FindAsync(id);
            if (ganttMilestone == null)
            {
                return NotFound();
            }
            ViewData["GanttTaskID"] = new SelectList(_context.GanttTasks, "ID", "ID", ganttMilestone.GanttTaskID);
            return View(ganttMilestone);
        }

        // POST: GanttMilestone/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,GanttTaskID,MilestoneName,Progress,DateCompleted")] GanttMilestone ganttMilestone)
        {
            if (id != ganttMilestone.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ganttMilestone);
                    await _context.SaveChangesAsync();

                    _context.ActivityLogs.Add(new ActivityLog
                    {
                        Message = $"Milestone '{ganttMilestone.MilestoneName}' (ID {ganttMilestone.ID}) updated.",
                        Timestamp = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GanttMilestoneExists(ganttMilestone.ID))
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

            ViewData["GanttTaskID"] = new SelectList(_context.GanttTasks, "ID", "ID", ganttMilestone.GanttTaskID);
            return View(ganttMilestone);
        }


        // GET: GanttMilestone/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganttMilestone = await _context.GanttMilestones
                .Include(g => g.GanttTask)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (ganttMilestone == null)
            {
                return NotFound();
            }

            return View(ganttMilestone);
        }

        // POST: GanttMilestone/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ganttMilestone = await _context.GanttMilestones.FindAsync(id);
            if (ganttMilestone != null)
            {
                _context.GanttMilestones.Remove(ganttMilestone);
                await _context.SaveChangesAsync();

                _context.ActivityLogs.Add(new ActivityLog
                {
                    Message = $"Milestone '{ganttMilestone.MilestoneName}' (ID {ganttMilestone.ID}) deleted.",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool GanttMilestoneExists(int id)
        {
            return _context.GanttMilestones.Any(e => e.ID == id);
        }
    }
}
