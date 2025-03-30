using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using haver.Data;
using haver.Models;
using haver.Utilities;
using haver.CustomControllers;

namespace haver.Controllers
{
    [Authorize]
    public class VendorController : ElephantController
    {
        private readonly HaverContext _context;

        public VendorController(HaverContext context)
        {
            _context = context;
        }

        // GET: Vendor
        [Authorize(Roles = "Admin,Procurement")]
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string? SearchCname,
            string? SearchString, string? actionButton, string sortDirection = "asc",
            string sortField = "Name", bool? isActive = null)
        {
            string[] sortOptions = { "Name", "Phone", "Email" };
            var vendors = _context.Vendors.AsNoTracking();

            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;

            isActive ??= true;
            vendors = vendors.Where(v => v.IsActive == isActive.Value);
            ViewBag.Status = isActive.Value ? "Active" : "Inactive";

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

            ViewData["Filtering"] = numberFilters > 0 ? "btn-danger" : "btn-outline-secondary";
            ViewData["numberFilters"] = numberFilters > 0 ? $"({numberFilters} Filter{(numberFilters > 1 ? "s" : "")} Applied)" : "";
            ViewData["ShowFilter"] = numberFilters > 0 ? "show" : "";

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
                "Phone" => sortDirection == "asc" ? vendors.OrderBy(v => v.Phone) : vendors.OrderByDescending(v => v.Phone),
                "Email" => sortDirection == "asc" ? vendors.OrderBy(v => v.Email) : vendors.OrderByDescending(v => v.Email),
                _ => sortDirection == "asc" ? vendors.OrderBy(v => v.Name) : vendors.OrderByDescending(v => v.Name),
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
            if (id == null) return NotFound();

            var vendor = await _context.Vendors.FirstOrDefaultAsync(m => m.ID == id);
            return vendor == null ? NotFound() : View(vendor);
        }

        // GET: Vendor/Create
        public IActionResult Create() => View();

        // POST: Vendor/Create
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
                    await LogActivity($"Vendor '{vendor.Name}' was created");

                    TempData["Message"] = "Vendor has been successfully created";
                    return RedirectToAction(nameof(Index));
                    //return RedirectToAction("Details", new { vendor.ID });
                }
            }
            catch (DbUpdateException dex)
            {
                HandleDbUpdateException(dex, "Name");
            }
            return View(vendor);
        }

        // GET: Vendor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vendor = await _context.Vendors.FindAsync(id);
            return vendor == null ? NotFound() : View(vendor);
        }

        // POST: Vendor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var vendorToUpdate = await _context.Vendors.FirstOrDefaultAsync(c => c.ID == id);
            if (vendorToUpdate == null) return NotFound();

            if (await TryUpdateModelAsync(vendorToUpdate, "", p => p.Name, p => p.Phone, p => p.Email))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    await LogActivity($"Vendor '{vendorToUpdate.Name}' was edited");

                    TempData["Message"] = "Vendor has been successfully edited";
                    return RedirectToAction(nameof(Index));
                    //return RedirectToAction("Details", new { vendorToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VendorExists(vendorToUpdate.ID)) return NotFound();
                    throw;
                }
                catch (DbUpdateException dex)
                {
                    HandleDbUpdateException(dex, "Name");
                }
            }
            return View(vendorToUpdate);
        }

        // GET: Vendor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var vendor = await _context.Vendors.FirstOrDefaultAsync(m => m.ID == id);
            return vendor == null ? NotFound() : View(vendor);
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
                    await _context.SaveChangesAsync();
                    await LogActivity($"Vendor '{vendor.Name}' was deleted");

                    TempData["Message"] = "Vendor has been successfully deleted";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to delete vendor. It is linked to a Sales Order.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(vendor);
        }

        public async Task<IActionResult> ToggleStatus(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null) return NotFound();

            //Toggle vendor status
            vendor.IsActive = !vendor.IsActive;
            _context.Update(vendor);
            await _context.SaveChangesAsync();

        
            string status = vendor.IsActive ? "activated" : "deactivated";
            await LogActivity($"Vendor '{vendor.Name}' was {status}");

         
            TempData["Message"] = vendor.IsActive
                ? $"Vendor '{vendor.Name}' has been successfully activated."
                : $"Vendor '{vendor.Name}' has been successfully deactivated.";

            return RedirectToAction(nameof(Index));
        }

        private bool VendorExists(int id) => _context.Vendors.Any(e => e.ID == id);

        private async Task LogActivity(string message)
        {
            string userName = User.Identity?.Name ?? "Unknown User";
            _context.ActivityLogs.Add(new ActivityLog
            {
                Message = $"{message} by {userName}.",
                Timestamp = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }


        private void HandleDbUpdateException(DbUpdateException dex, string property)
        {
            if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Vendors." + property))
            {
                ModelState.AddModelError(property, $"Unable to save changes. Remember, you cannot have duplicate {property}.");
            }
            else
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
        }
    }
}
