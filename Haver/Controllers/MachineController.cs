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
using haver.Utilities;

namespace haver.Controllers
{
    public class MachineController : ElephantController
    {
        private readonly HaverContext _context;

        public MachineController(HaverContext context)
        {
            _context = context;
        }

        // GET: Machine
        public async Task<IActionResult> Index(int? page,int? pageSizeID,int? machineID,string? actionButton,string sortDirection = "asc",string sortField = "SerialNumber")
        {
            var machines = _context.Machines.AsNoTracking();

            string[] sortOptions = new[] { "SerialNumber", "Description", "Quantity", "Size", "Class", "SizeDeck", "Description" };

            ViewData["MachineID"] = new SelectList(
                await _context.Machines
                    .OrderBy(m => m.SerialNumber)
                    .Select(m => new { m.ID, m.SerialNumber })
                    .ToListAsync(),
                "ID",
                "SerialNumber"
            );
            if (machineID.HasValue)
            {
                machines = machines.Where(m => m.ID == machineID.Value);
                ViewData["Filtering"] = "btn-danger";
                ViewData["numberFilters"] = "(1 Filter Applied)";
                ViewData["ShowFilter"] = "show";
            }
            else
            {
                ViewData["Filtering"] = "btn-outline-secondary";
                ViewData["numberFilters"] = "";
                ViewData["ShowFilter"] = "";
            }
            if (!string.IsNullOrEmpty(actionButton) && sortOptions.Contains(actionButton))
            {
                if (actionButton == sortField)
                {
                    sortDirection = sortDirection == "asc" ? "desc" : "asc";
                }
                sortField = actionButton;
            }

            machines = sortField switch
            {
                "SerialNumber" => sortDirection == "asc"
                    ? machines.OrderBy(m => m.SerialNumber)
                    : machines.OrderByDescending(m => m.SerialNumber),
                "Description" => sortDirection == "asc"
                    ? machines.OrderBy(m => m.Description)
                    : machines.OrderByDescending(m => m.Description),
                "Quantity" => sortDirection == "asc"
                    ? machines.OrderBy(m => m.Quantity)
                    : machines.OrderByDescending(m => m.Quantity),
                "Size" => sortDirection == "asc"
                    ? machines.OrderBy(m => m.Size)
                    : machines.OrderByDescending(m => m.Size),
                "Class" => sortDirection == "asc"
                    ? machines.OrderBy(m => m.Class)
                    : machines.OrderByDescending(m => m.Class),
                "SizeDeck" => sortDirection == "asc"
                    ? machines.OrderBy(m => m.SizeDeck)
                    : machines.OrderByDescending(m => m.SizeDeck),
                _ => machines.OrderBy(m => m.SerialNumber)
            };
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, nameof(Index));
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Machine>.CreateAsync(machines, page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Machine/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Machine/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Description,SerialNumber,Quantity,Size,Class,SizeDeck")] Machine machine)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(machine);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { machine.ID });
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Machines.SerialNumber"))
                {
                    ModelState.AddModelError("SerialNumber", "Unable to save changes. Remember, you cannot have duplicate serial numbers.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }


            return View(machine);
        }

        // GET: Machine/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines.FindAsync(id);
            if (machine == null)
            {
                return NotFound();
            }
            return View(machine);
        }

        // POST: Machine/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //Go get the machine to update
            var machineToUpdate = await _context.Machines.FirstOrDefaultAsync(c => c.ID == id);


            if (machineToUpdate == null)
            {
                return NotFound();
            }


            if (await TryUpdateModelAsync<Machine>(machineToUpdate, "",
                  p => p.Description, p => p.SerialNumber, p => p.Quantity, p => p.Size, p => p.Class, p => p.SizeDeck))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { machineToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MachineExists(machineToUpdate.ID))
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
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Machines.SerialNumber"))
                    {
                        ModelState.AddModelError("SerialNumber", "Unable to save changes. Remember, you cannot have duplicate serial numbers.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            return View(machineToUpdate);
        }

        // GET: Machine/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines
                .FirstOrDefaultAsync(m => m.ID == id);
            if (machine == null)
            {
                return NotFound();
            }

            return View(machine);
        }


        // POST: Machine/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var machine = await _context.Machines.FindAsync(id);

            try
            {
                if (machine != null)
                {
                    _context.Machines.Remove(machine);
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
                    ModelState.AddModelError("", "Unable to Delete Machine. Remember, you cannot delete a machine attached to a Machine Schedule");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(machine);
        }

        private void PopulateDropDownLists(PackageRelease? packageRelease = null)
        {
            ViewData["MachineScheduleID"] = new SelectList(_context.MachineSchedules, "ID", "Summary");
        }

        private bool MachineExists(int id)
        {
            return _context.Machines.Any(e => e.ID == id);
        }

    }
}
