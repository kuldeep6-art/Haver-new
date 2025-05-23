﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using haver.Data;
using haver.Models;
using haver.Utilities;
using haver.CustomControllers;
using System.Reflection.PortableExecutable;
using haver.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;

namespace haver.Controllers
{
    [Authorize]
    public class MachineTypeController : ElephantController
    {
        private readonly HaverContext _context;

        public MachineTypeController(HaverContext context)
        {
            _context = context;
        }

        // GET: MachineType
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string? actionButton,
            string sortDirection = "asc",string sortField = "Serial Number")
        {
            string[] sortOptions = new[] { "Description" };


            var machinetypes = from m in _context.MachineTypes
                               .Include(m => m.Machines)
                               .ThenInclude(m => m.SalesOrder)
                        .AsNoTracking()
                            select m;

            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
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
            if (sortField == "Description")
            {
                if (sortDirection == "asc")
                {
                    machinetypes = machinetypes
                        .OrderByDescending(p => p.Description);
                }
                else
                {
                    machinetypes = machinetypes
                        .OrderBy(p => p.Description);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<MachineType>.CreateAsync(machinetypes.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: MachineType/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machineType = await _context.MachineType
                .Include(m => m.Machines).ThenInclude(m => m.SalesOrder)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (machineType == null)
            {
                return NotFound();
            }

            return View(machineType);
        }

        // GET: MachineType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MachineType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Description")] MachineType machineType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(machineType);
                    await _context.SaveChangesAsync();

                    await LogActivity($"Machine Type '{machineType.Description}' was created");

                    await _context.SaveChangesAsync();

                    var returnUrl = ViewData["returnURL"]?.ToString();
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    return Redirect(returnUrl);
                }
            }
            catch (DbUpdateException dex)
            {
                ExceptionMessageVM msg = new();
                string baseMessage = dex.GetBaseException().Message;

                if (baseMessage.Contains("UNIQUE"))
                {
                    if (baseMessage.Contains("Description"))
                    {
                        msg.ErrProperty = "Description";
                        ModelState.AddModelError("Description", "Machine Model Combination should be Unique");
                    }
                }
                else
                {
                    msg.ErrProperty = string.Empty;
                }

                ModelState.AddModelError(msg.ErrProperty, msg.ErrMessage);
            }

            return View(machineType);
        }


        // GET: MachineType/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machineType = await _context.MachineType.FindAsync(id);
            if (machineType == null)
            {
                return NotFound();
            }
            return View(machineType);
        }

        // POST: MachineType/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var machinetypeToUpdate = await _context.MachineTypes.FirstOrDefaultAsync(c => c.ID == id);

            if (machinetypeToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<MachineType>(machinetypeToUpdate, "",
                        p => p.Description))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    await LogActivity($"Machine Type '{machinetypeToUpdate.Description}' was edited");
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MachineTypeExists(machinetypeToUpdate.ID))
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
                    var baseExceptionMessage = dex.GetBaseException().Message;
                    if (baseExceptionMessage.Contains("UNIQUE"))
                    {
                        ModelState.AddModelError("Description", "Machine Model Combination should be Unique");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    }
                }
            }

            return View(machinetypeToUpdate);
        }


        // GET: MachineType/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machineType = await _context.MachineType
                .FirstOrDefaultAsync(m => m.ID == id);
            if (machineType == null)
            {
                return NotFound();
            }

            return View(machineType);
        }

        // POST: MachineType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var machineType = await _context.MachineType.FindAsync(id);

            try
            {
                if (machineType != null)
                {
                    _context.MachineType.Remove(machineType);
                    await _context.SaveChangesAsync();

                    await LogActivity($"Machine Type '{machineType.Description}' was deleted");

                    await _context.SaveChangesAsync();
                }

                var returnUrl = ViewData["returnURL"]?.ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction(nameof(Index));
                }
                return Redirect(returnUrl);
            }
            catch (DbUpdateException dex)
            {
                ExceptionMessageVM msg = new();
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    msg.ErrProperty = "";
                    msg.ErrMessage = "Unable to Delete. This Machine Type is associated with existing records.";
                }

                ModelState.AddModelError(msg.ErrProperty, msg.ErrMessage);
            }
            return View(machineType);
        }

        private async Task LogActivity(string message)
        {
            string userName = User.Identity?.Name ?? "Unknown User";
            _context.ActivityLogs.Add(new ActivityLog
            {
                Message = $"{message} by {userName}.",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        private bool MachineTypeExists(int id)
        {
            return _context.MachineType.Any(e => e.ID == id);
        }
    }
}
