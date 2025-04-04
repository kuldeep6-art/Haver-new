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
using GymManagement.Utilities;
using haver.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace haver.Controllers
{
    [Authorize]
    public class MachineProcurementController : ElephantController
    {
        private readonly HaverContext _context;

        public MachineProcurementController(HaverContext context)
        {
            _context = context;
        }

        // GET: MachineProcurement
        public async Task<IActionResult> Index(int? MachineID, int? VendorID, int? page, int? pageSizeID, string actionButton,
            string SearchString, string sortDirection = "desc", string sortField = "PoNumber")
        {
            //Get the URL with the last filter, sort and page parameters from THE PATIENTS Index View
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Machine");

            if (!MachineID.HasValue)
            {
                //Go back to the proper return URL for the Patients controller
                return Redirect(ViewData["returnURL"].ToString());
            }

            PopulateDropDownLists();

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "PoNumber", "Vendor" };

            var appts = from a in _context.Procurements
                        .Include(a => a.Vendor)
                        .Include(a => a.Machine)
                        where a.MachineID == MachineID.GetValueOrDefault()
                        select a;

            if (VendorID.HasValue)
            {
                appts = appts.Where(p => p.VendorID == VendorID);
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                appts = appts.Where(p => p.PONumber.ToUpper().Contains(SearchString.ToUpper()));
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
                //@ViewData["ShowFilter"] = " show";
            }

            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted so lets sort!
            {
                page = 1;//Reset back to first page when sorting or filtering

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            //Now we know which field and direction to sort by.
            if (sortField == "Vendor")
            {
                if (sortDirection == "asc")
                {
                    appts = appts
                        .OrderBy(p => p.Vendor.Name);
                }
                else
                {
                    appts = appts
                        .OrderByDescending(p => p.Vendor.Name);
                }
            }

            else //Appointment Date
            {
                if (sortDirection == "asc")
                {
                    appts = appts
                        .OrderByDescending(p => p.PONumber);
                }
                else
                {
                    appts = appts
                        .OrderBy(p => p.PONumber);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Now get the MASTER record, the patient, so it can be displayed at the top of the screen
            Machine? machine = await _context.Machines
                .Include(s => s.MachineType)
                .Include(s => s.SalesOrder)
                .Where(p => p.ID == MachineID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefaultAsync();

            ViewBag.Machine = machine;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Procurement>.CreateAsync(appts.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: MachineProcurement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procurement = await _context.Procurements
                .Include(p => p.Machine)
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (procurement == null)
            {
                return NotFound();
            }

            return View(procurement);
        }

        // GET: MachineProcurement/Create
        public IActionResult Add(int? MachineID, string SerialNumber)
        {

            if (!MachineID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }

            ViewData["SerialNumber"] = SerialNumber;
            Procurement a = new Procurement()
            {
                MachineID = MachineID.GetValueOrDefault()
            };
            PopulateDropDownLists();
            return View(a);
        }

        // POST: MachineProcurement/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("ID,VendorID,MachineID,PONumber,PODueDate,PORcd,QualityICom,NcrRaised")] Procurement procurement, string SerialNumber)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(procurement);
                    await _context.SaveChangesAsync();
                    await LogActivity($"Procurement {procurement.PONumber} added for Machine {procurement.MachineID}");

                    await _context.SaveChangesAsync();

                    //update procurement dates for gantt
                    await UpdateGanttProcurementDates(procurement.MachineID);

                    var salesOrderUrl = Url.Action("Details", "SalesOrder", new { id = procurement.Machine.SalesOrderID });
                    TempData["SuccessMessage"] = $"Procurement {procurement.PONumber} successfully added for Machine (Serial Number: {procurement.Machine.SerialNumber}). " +
                         $"You can <a href=\"{salesOrderUrl}\">click here</a> to view the Sales Order details, add another machine, or update the order.";



                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            PopulateDropDownLists(procurement);
            ViewData["SerialNumber"] = SerialNumber;
            return View(procurement);
        }


        // GET: MachineProcurement/Edit/5
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procurement = await _context.Procurements
              .Include(a => a.Vendor)
              .Include(a => a.Machine)
              .AsNoTracking()
              .FirstOrDefaultAsync(m => m.ID == id);

            if (procurement == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(procurement);
            return View(procurement);
        }

        // POST: MachineProcurement/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id)
        {
            var procurementToUpdate = await _context.Procurements
                .Include(a => a.Vendor)
                .Include(a => a.Machine)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (procurementToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Procurement>(procurementToUpdate, "",
                a => a.VendorID, a => a.MachineID, a => a.PONumber,
                a => a.PODueDate, a => a.PORcd, a => a.QualityICom, a => a.NcrRaised))
            {
                try
                {
                    _context.Update(procurementToUpdate);
                    await _context.SaveChangesAsync();

                    await LogActivity($"Procurement {procurementToUpdate.PONumber} updated");

                    await _context.SaveChangesAsync();

                    await UpdateGanttProcurementDates(procurementToUpdate.MachineID);

                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProcurementExists(procurementToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            PopulateDropDownLists(procurementToUpdate);
            return View(procurementToUpdate);
        }

        public async Task UpdateGanttProcurementDates(int? machineId)
        {
            // Handle null machineId safely
            if (machineId == null || machineId == 0)
            {
                Console.WriteLine("Invalid MachineID. Cannot update Gantt.");
                return;
            }

            // Get the machine and include Procurements to load related records
            var machine = await _context.Machines
                .Include(m => m.Procurements)
                .FirstOrDefaultAsync(m => m.ID == machineId);

            // Check if machine or procurements are null or empty
            if (machine != null && machine.Procurements != null && machine.Procurements.Any())
            {
                // Get the min and max dates for procurement safely
                var procurementStart = machine.Procurements.Min(po => po.PODueDate);
                var procurementEnd = machine.Procurements.Max(po => po.PORcd);

                // Get the corresponding Gantt record for the machine
                var ganttRecord = await _context.GanttDatas
                    .FirstOrDefaultAsync(g => g.MachineID == machineId);

                if (ganttRecord != null)
                {
                    // Update the procurement dates in the Gantt
                    ganttRecord.PurchaseOrdersIssued = procurementStart;
                    ganttRecord.PurchaseOrdersCompleted = procurementEnd;

                    // Save the updated Gantt record
                    _context.GanttDatas.Update(ganttRecord);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"Gantt updated for MachineID {machineId}. Start: {procurementStart}, End: {procurementEnd}");
                }
                else
                {
                    Console.WriteLine($"No Gantt record found for MachineID {machineId}");
                }
            }
            else
            {
                Console.WriteLine("No procurement records found or procurement is null for this machine.");
            }
        }


        // GET: MachineProcurement/Delete/5
        public async Task<IActionResult> Remove(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procurement = await _context.Procurements
                .Include(p => p.Machine)
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (procurement == null)
            {
                return NotFound();
            }

            return View(procurement);
        }

        // POST: MachineProcurement/Delete/5
        [HttpPost, ActionName("Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveConfirmed(int id)
        {
            var procurement = await _context.Procurements
                  .Include(p => p.Machine)
                  .Include(p => p.Vendor)
                  .FirstOrDefaultAsync(m => m.ID == id);

            try
            {
                if (procurement != null)
                {
                    _context.Procurements.Remove(procurement);
                    await _context.SaveChangesAsync();

                    await LogActivity($"Procurement {procurement.PONumber} deleted");

                    await _context.SaveChangesAsync();
                }

                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(procurement);
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

        private SelectList VendorSelectList(int? id)
		{
			var dQuery = from d in _context.Vendors
						 where d.IsActive // Only include active vendors
						 orderby d.Name
						 select d;

			return new SelectList(dQuery, "ID", "Name", id);
		}

		private void PopulateDropDownLists(Procurement? procurement = null)
        {
            ViewData["VendorID"] = VendorSelectList(procurement?.VendorID);
        }
        private bool ProcurementExists(int id)
        {
            return _context.Procurements.Any(e => e.ID == id);
        }
    }
}
