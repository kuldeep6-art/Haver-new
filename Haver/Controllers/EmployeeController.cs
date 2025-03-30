using haver.CustomControllers;
using haver.Data;
using haver.Models;
using haver.Utilities;
using haver.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;

namespace haver.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeController : ElephantController
    {
        private readonly HaverContext _context;
        private readonly ApplicationDbContext _identityContext;
        private readonly IMyEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public EmployeeController(HaverContext context,
            ApplicationDbContext identityContext, IMyEmailSender emailSender,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _identityContext = identityContext;
            _emailSender = emailSender;
            _userManager = userManager;
        }

		public async Task<IActionResult> Index(int? page, int? pageSizeID, string? SUser, bool? Active = null)
		{
			ViewData["Filtering"] = "btn-block invisible";
			int numberFilters = 0;

			// Get all employees, but let's use pagination
			int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
			ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

			int pageIndex = page ?? 1; // Default to the first page if not provided

			var query = _context.Employees
								.Select(e => new EmployeeAdminVM
								{
									Email = e.Email,
									Active = e.Active,
									ID = e.ID,
									FirstName = e.FirstName,
									LastName = e.LastName,
									Phone = e.Phone
								});

			if (!Active.HasValue)
			{
				Active = true;
			}
			query = query.Where(v => v.Active == Active.Value);
			ViewBag.Status = Active.Value ? "Active" : "Inactive";

		
			if (!string.IsNullOrEmpty(SUser))
			{
				SUser = SUser.Trim().ToUpper();
				query = query.Where(u =>
					u.FirstName.ToUpper().Contains(SUser) ||
					u.LastName.ToUpper().Contains(SUser));

				numberFilters++;
			}

          

            // Count after filtering
            var count = await query.CountAsync();

			// Apply paging
			var employees = await query
								 .Skip((pageIndex - 1) * pageSize) // Skip items for previous pages
								 .Take(pageSize) // Take the number of records for current page
								 .ToListAsync();

			// Retrieve roles for each employee
			foreach (var e in employees)
			{
				var user = await _userManager.FindByEmailAsync(e.Email);
				if (user != null)
				{
					e.UserRoles = (List<string>)await _userManager.GetRolesAsync(user);
				}
			}

			// Track applied filters
			if (numberFilters != 0)
			{
				ViewData["Filtering"] = " btn-danger";
				ViewData["numberFilters"] = $"({numberFilters} Filter{(numberFilters > 1 ? "s" : "")} Applied)";
			}

			// Create PaginatedList and return the view
			var pageData = new PaginatedList<EmployeeAdminVM>(employees, count, pageIndex, pageSize);
			return View(pageData);
		}



		// GET: Employee/Create
		public IActionResult Create()
        {
            EmployeeAdminVM employee = new EmployeeAdminVM();
            PopulateAssignedRoleData(employee);
            return View(employee);
        }

        // POST: Employee/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Phone,Email")] Employee employee, string[] selectedRoles)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(employee);
                    await _context.SaveChangesAsync();

                    InsertIdentityUser(employee.Email, selectedRoles);

                    // 🌟 Activity Logging
                    await LogActivity($"New employee {employee.FirstName} {employee.LastName} was added");

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("Email", "Unable to save changes. Remember, you cannot have duplicate Email addresses.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            EmployeeAdminVM employeeAdminVM = new EmployeeAdminVM
            {
                Email = employee.Email,
                Active = employee.Active,
                ID = employee.ID,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Phone = employee.Phone
            };
            foreach (var role in selectedRoles)
            {
                employeeAdminVM.UserRoles.Add(role);
            }
            PopulateAssignedRoleData(employeeAdminVM);
            return View(employeeAdminVM);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Where(e => e.ID == id)
                .Select(e => new EmployeeAdminVM
                {
                    Email = e.Email,
                    Active = e.Active,
                    ID = e.ID,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Phone = e.Phone
                }).FirstOrDefaultAsync();

            if (employee == null)
            {
                return NotFound();
            }

            //Get the user from the Identity system
            var user = await _userManager.FindByEmailAsync(employee.Email);
            if (user != null)
            {
                //Add the current roles
                var r = await _userManager.GetRolesAsync(user);
                employee.UserRoles = (List<string>)r;
            }
            PopulateAssignedRoleData(employee);

            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, bool Active, string[] selectedRoles)
        {
            var employeeToUpdate = await _context.Employees
                .FirstOrDefaultAsync(m => m.ID == id);
            if (employeeToUpdate == null)
            {
                return NotFound();
            }

            // Note the current Email and Active Status
            bool ActiveStatus = employeeToUpdate.Active;
            string databaseEmail = employeeToUpdate.Email;

            // Get the currently logged-in admin
            var loggedInUser = User.Identity.Name;

            // Check if the user being edited is the logged-in admin
            bool isEditingSelf = employeeToUpdate.Email == loggedInUser;

            // Get the roles of the user being edited
            var user = await _userManager.FindByEmailAsync(employeeToUpdate.Email);
            var userRoles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();

            // Check if they are the only active admin
            bool isOnlyAdmin = userRoles.Contains("Admin") &&
                (await _userManager.GetUsersInRoleAsync("Admin")).Count == 1;

            // Create the EmployeeAdminVM instance for the view
            var employeeAdminVM = new EmployeeAdminVM
            {
                ID = employeeToUpdate.ID,
                Email = employeeToUpdate.Email,
                Active = employeeToUpdate.Active,
                FirstName = employeeToUpdate.FirstName,
                LastName = employeeToUpdate.LastName,
                Phone = employeeToUpdate.Phone,
                UserRoles = userRoles.ToList() // Ensure the roles are passed correctly
            };

            if (isEditingSelf)
            {
                // Prevent deactivation of self
                if (!Active)
                {
                    ModelState.AddModelError("", "You cannot deactivate your own account.");
                    PopulateAssignedRoleData(employeeAdminVM);
                    return View(employeeAdminVM);
                }

                // Prevent removing self from Admin role
                if (!selectedRoles.Contains("Admin"))
                {
                    ModelState.AddModelError("", "You cannot remove yourself from the Admin role.");
                    PopulateAssignedRoleData(employeeAdminVM);
                    return View(employeeAdminVM);
                }
            }

            if (isOnlyAdmin && isEditingSelf)
            {
                // Prevent deactivation or admin role removal
                if (!Active)
                {
                    ModelState.AddModelError("", "You cannot deactivate yourself because you are the only admin.");
                    PopulateAssignedRoleData(employeeAdminVM);
                    return View(employeeAdminVM);
                }

                if (!selectedRoles.Contains("Admin"))
                {
                    ModelState.AddModelError("", "You cannot remove yourself from the Admin role because you are the only admin.");
                    PopulateAssignedRoleData(employeeAdminVM);
                    return View(employeeAdminVM);
                }
            }

            if (await TryUpdateModelAsync(employeeToUpdate, "",
                e => e.FirstName, e => e.LastName, e => e.Phone, e => e.Email, e => e.Active))
            {
                try
                {
                    await _context.SaveChangesAsync();

                    // Handle Active status changes
                    if (employeeToUpdate.Active == false && ActiveStatus == true)
                    {
                        await DeleteIdentityUser(employeeToUpdate.Email);
                    }
                    else if (employeeToUpdate.Active == true && ActiveStatus == false)
                    {
                        InsertIdentityUser(employeeToUpdate.Email, selectedRoles);
                    }
                    else if (employeeToUpdate.Active == true && ActiveStatus == true)
                    {
                        if (employeeToUpdate.Email != databaseEmail)
                        {
                            InsertIdentityUser(employeeToUpdate.Email, selectedRoles);
                            await DeleteIdentityUser(databaseEmail);
                        }
                        else
                        {
                            await UpdateUserRoles(selectedRoles, employeeToUpdate.Email);
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employeeToUpdate.ID))
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
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("Email", "Unable to save changes. Remember, you cannot have duplicate Email addresses.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }

            // Ensure we populate roles again before returning the view
            PopulateAssignedRoleData(employeeAdminVM);
            return View(employeeAdminVM);
        }


        private void PopulateAssignedRoleData(EmployeeAdminVM employee)
        {//Prepare checkboxes for all Roles
            var allRoles = _identityContext.Roles;
            var currentRoles = employee.UserRoles;
            var viewModel = new List<RoleVM>();
            foreach (var r in allRoles)
            {
                viewModel.Add(new RoleVM
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    Assigned = currentRoles.Contains(r.Name)
                });
            }
            ViewBag.Roles = viewModel;
        }

        private async Task UpdateUserRoles(string[] selectedRoles, string Email)
        {
            var _user = await _userManager.FindByEmailAsync(Email);//IdentityUser
            if (_user != null)
            {
                var UserRoles = (List<string>)await _userManager.GetRolesAsync(_user);//Current roles user is in

                if (selectedRoles == null)
                {
                    //No roles selected so just remove any currently assigned
                    foreach (var r in UserRoles)
                    {
                        await _userManager.RemoveFromRoleAsync(_user, r);
                    }
                }
                else
                {
                    //At least one role checked so loop through all the roles
                    //and add or remove as required

                    //We need to do this next line because foreach loops don't always work well
                    //for data returned by EF when working async.  Pulling it into an IList<>
                    //first means we can safely loop over the colleciton making async calls and avoid
                    //the error 'New transaction is not allowed because there are other threads running in the session'
                    IList<IdentityRole> allRoles = _identityContext.Roles.ToList<IdentityRole>();

                    foreach (var r in allRoles)
                    {
                        if (selectedRoles.Contains(r.Name))
                        {
                            if (!UserRoles.Contains(r.Name))
                            {
                                await _userManager.AddToRoleAsync(_user, r.Name);
                            }
                        }
                        else
                        {
                            if (UserRoles.Contains(r.Name))
                            {
                                await _userManager.RemoveFromRoleAsync(_user, r.Name);
                            }
                        }
                    }
                }
            }
        }

        private void InsertIdentityUser(string Email, string[] selectedRoles)
        {
            //Create the IdentityUser in the IdentitySystem
            //Note: this is similar to what we did in ApplicationSeedData
            if (_userManager.FindByEmailAsync(Email).Result == null)
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = Email,
                    Email = Email,
                    EmailConfirmed = true //since we are creating it!
                };
                //Create a random password with a default 8 characters
                string password = MakePassword.Generate();
                password = "Pa55w@rd";
                IdentityResult result = _userManager.CreateAsync(user, password).Result;

                if (result.Succeeded)
                {
                    foreach (string role in selectedRoles)
                    {
                        _userManager.AddToRoleAsync(user, role).Wait();
                    }
                }
            }
            else
            {
                TempData["message"] = "The Login Account for " + Email + " was already in the system.";
            }
        }

        private async Task DeleteIdentityUser(string Email)
        {
            var userToDelete = await _identityContext.Users.Where(u => u.Email == Email).FirstOrDefaultAsync();
            if (userToDelete != null)
            {
                _identityContext.Users.Remove(userToDelete);
                await _identityContext.SaveChangesAsync();
            }
        }

        private async Task InviteUserToResetPassword(Employee employee, string message)
        {
            message ??= "Hello " + employee.FirstName + "<br /><p>Please navigate to:<br />" +
                        "<a href='https://theapp.azurewebsites.net/' title='https://theapp.azurewebsites.net/' target='_blank' rel='noopener'>" +
                        "https://theapp.azurewebsites.net</a><br />" +
                        " and create a new password for " + employee.Email + " using Forgot Password.</p>";
            try
            {
                await _emailSender.SendOneAsync(employee.Summary, employee.Email,
                "Account Registration", message);
                TempData["message"] = "Invitation email sent to " + employee.Summary + " at " + employee.Email;
            }
            catch (Exception)
            {
                TempData["message"] = "Could not send Invitation email to " + employee.Summary + " at " + employee.Email;
            }


        }


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

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.ID == id);
        }
    }
}

