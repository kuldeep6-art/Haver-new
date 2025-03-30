using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using haver.Data;
using haver.Models;
using haver.CustomControllers;
using haver.Utilities;
using haver.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace haver.Controllers
{
	[Authorize]
	public class EngineerController : ElephantController
	{
		private readonly HaverContext _context;

		public EngineerController(HaverContext context)
		{
			_context = context;
		}

		// GET: Engineer

		[Authorize(Roles ="Admin,Engineering")]
		public async Task<IActionResult> Index(int? page, int? pageSizeID, string? SearchString,
			string? actionButton, string sortDirection = "asc", string sortField = "Last Name")
		{
			string[] sortOptions = new[] { "Last Name", "First Name", "Phone" };
			ViewData["Filtering"] = "btn-outline-secondary";
			int numberFilters = 0;

			var engineers = from e in _context.Engineers.AsNoTracking() select e;

			if (!String.IsNullOrEmpty(SearchString))
			{
				engineers = engineers.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
									   || p.FirstName.ToUpper().Contains(SearchString.ToUpper()));
				numberFilters++;
			}

			if (numberFilters != 0)
			{
				ViewData["Filtering"] = " btn-danger";
				ViewData["numberFilters"] = "(" + numberFilters.ToString()
					+ " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
				ViewData["ShowFilter"] = " show";
			}

			if (!String.IsNullOrEmpty(actionButton))
			{
				if (sortOptions.Contains(actionButton))
				{
					if (actionButton == sortField)
					{
						sortDirection = sortDirection == "asc" ? "desc" : "asc";
					}
					sortField = actionButton;
				}
			}

			if (sortField == "First Name")
			{
				if (sortDirection == "asc")
				{
					engineers = engineers.OrderByDescending(p => p.FirstName);
				}
				else
				{
					engineers = engineers.OrderBy(p => p.FirstName);
				}
			}
			else
			{
				if (sortDirection == "asc")
				{
					engineers = engineers.OrderBy(p => p.LastName).ThenBy(p => p.FirstName);
				}
				else
				{
					engineers = engineers.OrderByDescending(p => p.LastName).ThenByDescending(p => p.FirstName);
				}
			}

			ViewData["sortField"] = sortField;
			ViewData["sortDirection"] = sortDirection;

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,EngineerInitials,FirstName,LastName,Phone,Email")] Engineer engineer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(engineer);
                    await _context.SaveChangesAsync();

                    await LogActivity($"New engineer {engineer.FirstName} {engineer.LastName} was added");

                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Engineer has been successfully created";
                    //return RedirectToAction("Details", new { engineer.ID });
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {
                var baseExceptionMessage = dex.GetBaseException().Message;
                if (baseExceptionMessage.Contains("UNIQUE constraint failed: Engineers.FirstName, Engineers.LastName"))
                {
                    ModelState.AddModelError("LastName", "An engineer with the same First Name and Last Name already exists.");
                    ModelState.AddModelError("FirstName", "This combination must be unique.");
                }
                else if (baseExceptionMessage.Contains("UNIQUE constraint failed: Engineers.Email"))
                {
                    ModelState.AddModelError("Email", "An engineer with the same Email already exists.");
                }
                else if (baseExceptionMessage.Contains("UNIQUE constraint failed: Engineers.EngineerInitials"))
                {
                    ModelState.AddModelError("EngineerInitials", "An engineer with the same Initial already exists.");
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var engineerToUpdate = await _context.Engineers.FirstOrDefaultAsync(e => e.ID == id);
            if (engineerToUpdate == null)
            {
                return NotFound();
            }

            // Store original values for logging
            string previousEmail = engineerToUpdate.Email;
            string previousName = $"{engineerToUpdate.FirstName} {engineerToUpdate.LastName}";

            if (await TryUpdateModelAsync<Engineer>(engineerToUpdate, "",
                n => n.EngineerInitials, n => n.FirstName, n => n.LastName, n => n.Email))
            {
                try
                {
                    await _context.SaveChangesAsync();

                    string updatedName = $"{engineerToUpdate.FirstName} {engineerToUpdate.LastName}";
                    if (previousName != updatedName)
                    {
                        await LogActivity($"Engineer {previousName} changed their name to {updatedName}");
                    }

                    if (engineerToUpdate.Email != previousEmail)
                    {
                        await LogActivity($"Engineer {updatedName} updated their email from {previousEmail} to {engineerToUpdate.Email}");
                    }

                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Engineer has been successfully Edited";
                    return RedirectToAction(nameof(Index));
                    //return RedirectToAction("Details", new { engineerToUpdate.ID });
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
            if (engineer == null)
            {
                return NotFound();
            }

            try
            {
                _context.Engineers.Remove(engineer);
                await _context.SaveChangesAsync();

                await LogActivity($"Engineer {engineer.FirstName} {engineer.LastName} was deleted");

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
                ExceptionMessageVM msg = new();
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    msg.ErrProperty = "";
                    msg.ErrMessage = "Unable to Delete " + ViewData["ControllerFriendlyName"] +
                        ". Remember, you cannot delete a " + ViewData["ControllerFriendlyName"] +
                        " that has related records.";
                }

                ModelState.AddModelError(msg.ErrProperty, msg.ErrMessage);
            }
            return View(engineer);
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

        private bool EngineerExists(int id)
		{
			return _context.Engineers.Any(e => e.ID == id);
		}
	}
}