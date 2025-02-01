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
            var haverContext = _context.PackageReleases.Include(p => p.SalesOrder);
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
                .Include(p => p.SalesOrder)
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
            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber");
            return View();
        }

        // POST: PackageRelease/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,PReleaseDateP,PReleaseDateA,Notes,SalesOrderID")] PackageRelease packageRelease)
        {
            if (ModelState.IsValid)
            {
                _context.Add(packageRelease);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber", packageRelease.SalesOrderID);
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
            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber", packageRelease.SalesOrderID);
            return View(packageRelease);
        }

        // POST: PackageRelease/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,PReleaseDateP,PReleaseDateA,Notes,SalesOrderID")] PackageRelease packageRelease)
        {
            if (id != packageRelease.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(packageRelease);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PackageReleaseExists(packageRelease.ID))
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
            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber", packageRelease.SalesOrderID);
            return View(packageRelease);
        }

        // GET: PackageRelease/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var packageRelease = await _context.PackageReleases
                .Include(p => p.SalesOrder)
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
            if (packageRelease != null)
            {
                _context.PackageReleases.Remove(packageRelease);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PackageReleaseExists(int id)
        {
            return _context.PackageReleases.Any(e => e.ID == id);
        }
    }
}
