using GymManagement.Utilities;
using haver.CustomControllers;
using haver.Data;
using haver.Models;
using haver.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace haver.Controllers
{
    [Authorize]
    public class EmployeeAccountController : CognizantController
    {
        private readonly HaverContext _context;

        public EmployeeAccountController(HaverContext context)
        {
            _context = context;
        }

        // GET: EmployeeAccount
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Details));
        }

        // GET: EmployeeAccount/Details
        public async Task<IActionResult> Details()
        {
            var employee = await _context.Employees
                .Where(e => e.Email == User.Identity.Name)
                .Select(e => new EmployeeVM
                {
                    ID = e.ID,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Phone = e.Phone
                })
                .FirstOrDefaultAsync();

            return employee == null ? NotFound() : View(employee);
        }

        // GET: EmployeeAccount/Edit
        public async Task<IActionResult> Edit()
        {
            var employee = await _context.Employees
                .Where(e => e.Email == User.Identity.Name)
                .Select(e => new EmployeeVM
                {
                    ID = e.ID,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Phone = e.Phone,
                })
                .FirstOrDefaultAsync();

            return employee == null ? NotFound() : View(employee);
        }

        // POST: EmployeeAccount/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var employeeToUpdate = await _context.Employees.FirstOrDefaultAsync(m => m.ID == id);
            if (employeeToUpdate == null) return NotFound();

            if (await TryUpdateModelAsync(employeeToUpdate, "", c => c.FirstName, c => c.LastName, c => c.Phone))
            {
                try
                {
                    _context.Update(employeeToUpdate);
                    await _context.SaveChangesAsync();
                    await LogActivity($"Employee {employeeToUpdate.FirstName} {employeeToUpdate.LastName} updated their profile");

                    UpdateUserNameCookie(employeeToUpdate.Summary);
                    return RedirectToAction(nameof(Details));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employeeToUpdate.ID)) return NotFound();
                    throw;
                }
                catch (DbUpdateException dex)
                {
                    HandleDbUpdateException(dex);
                }
            }
            return View(employeeToUpdate);
        }

        private void UpdateUserNameCookie(string userName)
        {
            CookieHelper.CookieSet(HttpContext, "userName", userName, 960);
        }

        private bool EmployeeExists(int id) => _context.Employees.Any(e => e.ID == id);

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

        private void HandleDbUpdateException(DbUpdateException dex)
        {
            ModelState.AddModelError("", "Something went wrong in the database.");
        }
    }
}
