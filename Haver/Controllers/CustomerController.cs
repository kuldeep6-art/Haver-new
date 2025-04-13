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
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace haver.Controllers
{
    public class CustomerController : ElephantController
    {
        private readonly HaverContext _context;

        public CustomerController(HaverContext context)
        {
            _context = context;
        }

        // GET: Customer
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string? SearchCname,
            string? actionButton, string sortDirection = "asc", string sortField = "Customer")
        {
            string[] sortOptions = new[] { "Company Name", "Company Phone Number" };

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;

            var customers = from m in _context.Customers
                        .AsNoTracking()
                            select m;

            if (!String.IsNullOrEmpty(SearchCname))
            {
                customers = customers.Where(p => p.CompanyName.ToUpper().Contains(SearchCname.ToUpper()));
                numberFilters++;
            }

            //Give feedback about the state of the filters
            if (numberFilters != 0)
            {
                //Toggle the Open/Closed state of the collapse depending on if we are filtering
                ViewData["Filtering"] = " btn-danger";
                //Show how many filters have been applied
                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
                //Keep the Bootstrap collapse open
                @ViewData["ShowFilter"] = " show";
            }

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

            if (sortField == "Company Name")
            {
                if (sortDirection == "asc")
                {
                    customers = customers
                        .OrderByDescending(p => p.CompanyName);
                }
                else
                {
                    customers = customers
                        .OrderBy(p => p.CompanyName);
                }
            } else if (sortField == "Company Phone Number")
            {
                if (sortDirection == "asc")
                {
                    customers = customers
                        .OrderByDescending(p => p.Phone);
                }
                else
                {
                    customers = customers
                        .OrderBy(p => p.Phone);
                }
            }

                //Set sort for next time
                ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Customer>.CreateAsync(customers.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.ID == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,CompanyName,Phone")] Customer customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(customer);
                    await _context.SaveChangesAsync();

                    string userName = User.Identity?.Name ?? "Unknown User";
                    _context.ActivityLogs.Add(new ActivityLog
                    {
                        Message = $"Customer '{customer.CompanyName}' was created by {userName}.",
                        Timestamp = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
					TempData["Message"] = "Customer has been successfully Created";
					return RedirectToAction(nameof(Index));

				}
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Customers.Phone"))
                {
                    ModelState.AddModelError("Phone", "Unable to save changes. Remember, you cannot have duplicate Phone numbers.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            return View(customer);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customer/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //Go get the customer to update
            var customerToUpdate = await _context.Customers.FirstOrDefaultAsync(c => c.ID == id);


            if (customerToUpdate == null)
            {
                return NotFound();
            }


            if (await TryUpdateModelAsync<Customer>(customerToUpdate, "",
                  p => p.CompanyName, p => p.Phone))
            {
                try
                {
                    await _context.SaveChangesAsync();

                    string userName = User.Identity?.Name ?? "Unknown User";
                    _context.ActivityLogs.Add(new ActivityLog
                    {
                        Message = $"Customer '{customerToUpdate.CompanyName}' was edited by {userName}.",
                        Timestamp = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Customer has been successfully Edited";
					return RedirectToAction(nameof(Index));

				}
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customerToUpdate.ID))
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
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Customers.Phone"))
                    {
                        ModelState.AddModelError("Phone", "Unable to save changes. Remember, you cannot have duplicate Phone numbers.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            return View(customerToUpdate);
        }

        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (customer == null)
            {   
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            try
            {
                if (customer != null)
                {
                    _context.Customers.Remove(customer);
                }

                await _context.SaveChangesAsync();

                string userName = User.Identity?.Name ?? "Unknown User";
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Message = $"Customer '{customer.CompanyName}' was deleted by {userName}.",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
				TempData["Message"] = "Customer has been successfully Deleted";

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
                    ModelState.AddModelError("", "Unable to Delete Customer. Remember, you cannot delete a Customer that has a Sales Order");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(customer);
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.ID == id);
        }
    }
}
