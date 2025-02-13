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
using System.Reflection.PortableExecutable;
using haver.Utilities;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace haver.Controllers
{
    
    public class EngineerController : ElephantController
    {
        private readonly HaverContext _context;

        public EngineerController(HaverContext context)
        {
            _context = context;
        }


        // adding in Sorting and filtering for the engineer section based on ??(Dont know which variables to sort by yet)
        // GET: Engineer
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string? SearchString, string? actionButton, string sortDirection = "asc", string sortField = "LastName")
        {

            string[] sortOptions = new[] { "LastName","FirstName", "Phone" };

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;

            var engineers = from e in _context.Engineers
                            .AsNoTracking()
                            select e;


            if (!String.IsNullOrEmpty(SearchString))
            {
                engineers = engineers.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || p.FirstName.ToUpper().Contains(SearchString.ToUpper()));
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
            if (sortField == "FirstName")
            {
                if (sortDirection == "asc")
                {
                    engineers = engineers
                        .OrderByDescending(p => p.FirstName);
                }
                else
                {
                    engineers = engineers
                        .OrderBy(p => p.FirstName);
                }
            }
            else 
            {
                if (sortDirection == "asc")
                {
                    engineers = engineers
                        .OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                }
                else
                {
                    engineers = engineers
                        .OrderByDescending(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Engineer>.CreateAsync(engineers.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Engineer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var engineer = await _context.Engineers
                .FirstOrDefaultAsync(m => m.ID == id);
            if (engineer == null)
            {
                return NotFound();
            }

            return View(engineer);
        }

        // GET: Engineer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Engineer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,Phone,Email")] Engineer engineer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(engineer);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Engineer has been successfully created";
                    return RedirectToAction("Details", new { engineer.ID });
                }
            }
            catch (DbUpdateException dex)
            {
                var baseExceptionMessage = dex.GetBaseException().Message;
                // Check for the composite unique constraint failure
                if (baseExceptionMessage.Contains("UNIQUE constraint failed: Engineers.FirstName, Engineers.LastName"))
                {
                    ModelState.AddModelError("LastName", "An engineer with the same First Name and Last Name already exists.");
                    ModelState.AddModelError("FirstName", "This combination must be unique.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            return View(engineer);
        }

        // GET: Engineer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var engineer = await _context.Engineers.FindAsync(id);
            if (engineer == null)
            {
                return NotFound();
            }
            return View(engineer);
        }

        // POST: Engineer/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var engineerToUpdate = await _context.Engineers.FirstOrDefaultAsync(e => e.ID == id);
            if (engineerToUpdate == null)
            {
                return NotFound();
            }

            if ((await TryUpdateModelAsync<Engineer>(engineerToUpdate, "", n=>n.FirstName,
                n => n.LastName, n=> n.Email )))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Engineer has been successfully deleted";
                    return RedirectToAction("Details", new { engineerToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EngineerExists(engineerToUpdate.ID))
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
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Engineers.LastName"))
                    {
                        ModelState.AddModelError("LastName", "Unable to save changes. Remember, you cannot have duplicate Names");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
                //Dont know if this line is needed

            }
            return View(engineerToUpdate);
        }

        // GET: Engineer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var engineer = await _context.Engineers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (engineer == null)
            {
                return NotFound();
            }

            return View(engineer);
        }

        // POST: Engineer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var engineer = await _context.Engineers.FindAsync(id);
            try
            {
                if (engineer != null)
                {
                    _context.Engineers.Remove(engineer);
                }
                await _context.SaveChangesAsync();
                var returnUrl = ViewData["returnURL"]?.ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction(nameof(Index));
                }
                return Redirect(returnUrl);
            }
            catch (DbUpdateException)
            {
               
                
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                
            }
            return View(engineer);

            
        }

       
        private bool EngineerExists(int id)
        {
            return _context.Engineers.Any(e => e.ID == id);
        }
    }
}
