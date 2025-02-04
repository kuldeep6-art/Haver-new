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
using haver.CustomControllers;

namespace haver.Controllers
{
    public class VendorController : ElephantController
    {
        private readonly HaverContext _context;

        public VendorController(HaverContext context)
        {
            _context = context;
        }

        // GET: Vendor
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string? SearchCname, string? SearchString, string? actionButton, string sortDirection = "asc", string sortField = "Name")
        {
            string[] sortOptions = new[] { "Name", "Phone", "Email" };

            var vendors = from m in _context.Vendors.AsNoTracking() select m;

            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;

            if (!string.IsNullOrEmpty(SearchCname))
            {
                vendors = vendors.Where(v => v.Name.ToUpper().Contains(SearchCname.ToUpper()));
                numberFilters++;
            }

            if (!string.IsNullOrEmpty(SearchString))
            {
                vendors = vendors.Where(v => v.Phone.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
            }

            if (numberFilters > 0)
            {
                ViewData["Filtering"] = "btn-danger";
                ViewData["numberFilters"] = $"({numberFilters} Filter{(numberFilters > 1 ? "s" : "")} Applied)";
                ViewData["ShowFilter"] = "show";
            }
            else
            {
                ViewData["numberFilters"] = string.Empty;
                ViewData["ShowFilter"] = string.Empty;
            }

            if (!string.IsNullOrEmpty(actionButton) && sortOptions.Contains(actionButton))
            {
                if (actionButton == sortField)
                {
                    sortDirection = sortDirection == "asc" ? "desc" : "asc";
                }
                sortField = actionButton;
            }

            vendors = sortField switch
            {
                "Phone" => sortDirection == "asc"
                    ? vendors.OrderBy(v => v.Phone)
                    : vendors.OrderByDescending(v => v.Phone),
                "Email" => sortDirection == "asc"
                    ? vendors.OrderBy(v => v.Email)
                    : vendors.OrderByDescending(v => v.Email),
                _ => sortDirection == "asc"
                    ? vendors.OrderBy(v => v.Name)
                    : vendors.OrderByDescending(v => v.Name),
            };

            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Vendor>.CreateAsync(vendors, page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Vendor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }

        // GET: Vendor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vendor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Phone,Email")] Vendor vendor)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(vendor);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Vendor has been successfully Created";
                    return RedirectToAction("Details", new { vendor.ID });
                }
            }
			catch (DbUpdateException dex)
			{
				if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Vendors.Name"))
				{
					ModelState.AddModelError("Name", "Unable to save changes. Remember, you cannot have duplicate Vendor Names.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
				}
			}

			return View(vendor);
        }   

        // GET: Vendor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }
            return View(vendor);
        }

        // POST: Vendor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //Go get the vendor to update
            var vendorToUpdate = await _context.Vendors.FirstOrDefaultAsync(c => c.ID == id);

            if (vendorToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Vendor>(vendorToUpdate, "",
                  p => p.Name, p => p.Phone, p => p.Email))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Vendor has been successfully Edited";
                    return RedirectToAction("Details", new { vendorToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VendorExists(vendorToUpdate.ID))
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
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Vendors.Name"))
                    {
                        ModelState.AddModelError("Name", "Unable to save changes. Remember, you cannot have duplicate Vendor Names.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            return View(vendorToUpdate);
        }

        // GET: Vendor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }

        // POST: Vendor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);

            try
            {
                if (vendor != null)
                {
                    _context.Vendors.Remove(vendor);
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
                    ModelState.AddModelError("", "Unable to Delete Vendor.  Remember, you cannot delete a Vendor attached to a Sales Order");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(vendor);
        }

        private bool VendorExists(int id)
        {
            return _context.Vendors.Any(e => e.ID == id);
        }
    }
}
