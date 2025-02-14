using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using haver.Data;
using haver.Models;
using System.Globalization;

namespace haver.Controllers
{
    public class GanttTaskController : Controller
    {
        private readonly HaverContext _context;

        public GanttTaskController(HaverContext context)
        {
            _context = context;
        }

        // GET: GanttTask
        public async Task<IActionResult> Index()
        {
            var haverContext = _context.GanttTasks.Include(g => g.SalesOrder);
            return View(await haverContext.ToListAsync());
        }

        // GET: GanttTask/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganttTask = await _context.GanttTasks
                .Include(g => g.SalesOrder)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (ganttTask == null)
            {
                return NotFound();
            }

            return View(ganttTask);
        }

        // GET: GanttTask/Create
        public IActionResult Create()
        {
            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber");
            return View();
        }

        // POST: GanttTask/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,SalesOrderID,StartDate,EndDate,Notes")] GanttTask ganttTask)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ganttTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber", ganttTask.SalesOrderID);
            return View(ganttTask);
        }

        // GET: GanttTask/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganttTask = await _context.GanttTasks.FindAsync(id);
            if (ganttTask == null)
            {
                return NotFound();
            }
            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber", ganttTask.SalesOrderID);
            return View(ganttTask);
        }

        // POST: GanttTask/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,SalesOrderID,StartDate,EndDate,Notes")] GanttTask ganttTask)
        {
            if (id != ganttTask.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ganttTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GanttTaskExists(ganttTask.ID))
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
            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber", ganttTask.SalesOrderID);
            return View(ganttTask);
        }

        // GET: GanttTask/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganttTask = await _context.GanttTasks
                .Include(g => g.SalesOrder)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (ganttTask == null)
            {
                return NotFound();
            }

            return View(ganttTask);
        }

        // POST: GanttTask/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ganttTask = await _context.GanttTasks.FindAsync(id);
            if (ganttTask != null)
            {
                _context.GanttTasks.Remove(ganttTask);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> GetGanttData()
        {
            var tasks = await _context.GanttTasks
                .Include(g => g.GanttMilestones)
                .Select(g => new
                {
                    id = g.ID,
                    name = g.SalesOrderID.ToString(), // Sales Order as task name
                    start = g.StartDate.ToString("yyyy-MM-dd"),
                    end = g.EndDate.ToString("yyyy-MM-dd"),
                    progress = g.OverallProgress,
                    dependencies = ""  // Add dependencies if needed
                })
                .ToListAsync();

            return Json(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTask([FromBody] GanttTaskUpdateModel model)
        {
            if (model == null) return BadRequest("Invalid data.");

            var task = await _context.GanttTasks
                .Include(a => a.SalesOrder)
                .Include(g => g.GanttMilestones) // Include milestones
                .FirstOrDefaultAsync(g => g.ID == model.Id);

            if (task == null) return NotFound("Task not found.");

            try
            {
                task.StartDate = DateTime.ParseExact(model.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                task.EndDate = DateTime.ParseExact(model.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                // Update milestones' progress
                foreach (var milestone in task.GanttMilestones)
                {
                    milestone.Progress = model.Progress;
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Task updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

        private bool GanttTaskExists(int id)
        {
            return _context.GanttTasks.Any(e => e.ID == id);
        }
    }
}
