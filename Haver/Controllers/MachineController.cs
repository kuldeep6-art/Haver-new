using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using haver.Data;
using haver.Models;
using System.Reflection.PortableExecutable;
using Machine = haver.Models.Machine;
using haver.Utilities;
using haver.CustomControllers;

namespace haver.Controllers
{
    public class MachineController : ElephantController
    {
        private readonly HaverContext _context;

        public MachineController(HaverContext context)
        {
            _context = context;
        }

        // GET: Machine
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string? DesString, string? PoString, string? SerString, string? ClString, 
            string? actionButton, string sortDirection = "asc", string sortField = "Description")
        {
            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Description", "Production Order Number" };

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied

            PopulateDropDownLists();

            var machines = from s in _context.Machines
                               .Include(s => s.SalesOrder)
                               .AsNoTracking()
                           select s;

            //Add as many filters as needed

            if (!String.IsNullOrEmpty(DesString))
            {
                machines = machines.Where(p => p.Description.ToUpper().Contains(DesString.ToUpper()));
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(PoString))
            {
                machines = machines.Where(p => p.ProductionOrderNumber.ToUpper().Contains(PoString.ToUpper()));
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SerString))
            {
                machines = machines.Where(p => p.SerialNumber.Contains(SerString));
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(ClString))
            {
                machines = machines.Where(p => p.Class.ToUpper().Contains(ClString.ToUpper()));
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
            if (sortField == "ProductionOrderNumber")
            {
                if (sortDirection == "asc")
                {
                    machines = machines
                        .OrderByDescending(p => p.ProductionOrderNumber);
                }
                else
                {
                    machines = machines
                        .OrderBy(p => p.ProductionOrderNumber);
                }
            }

            else
            {
                if (sortDirection == "asc")
                {
                    machines = machines
                        .OrderBy(p => p.Description);
                }
                else
                {
                    machines = machines
                        .OrderByDescending(p => p.Description);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;


            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Machine>.CreateAsync(machines.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);

        }

        // GET: Machine/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines
                .Include(m => m.SalesOrder)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (machine == null)
            {
                return NotFound();
            }

            return View(machine);
        }

        // GET: Machine/Create
        public IActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        }

        // POST: Machine/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Description,ProductionOrderNumber,SerialNumber,Quantity,Size,Class,SizeDeck,Media,SpareParts,SparePMedia,Base,AirSeal,CoatingLining,Disassembly,BudgetedHours,ActualAssemblyHours,ReworkHours,Nameplate,PreOrder,Scope,SalesOrderID")] Machine machine)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(machine);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { machine.ID });
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Machines.SerialNumber"))
                {
                    ModelState.AddModelError("SerialNumber", "Unable to save changes. Remember, you cannot have duplicate Serial Number.");
                }
                else if(dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Machines.ProductionOrderNumber"))
                {
                    ModelState.AddModelError("ProductionOrderNumber", "Unable to save changes. Remember, you cannot have duplicate Production Order Number.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }


          PopulateDropDownLists(machine);
            return View(machine);
        }

        // GET: Machine/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines.FindAsync(id);
            if (machine == null)
            {
                return NotFound();
            }
           PopulateDropDownLists(machine);
            return View(machine);
        }

        // POST: Machine/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var machinesToUpdate = await _context.Machines.FirstOrDefaultAsync(p => p.ID == id);

            if (machinesToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Machine>(machinesToUpdate, "",
               p => p.Description, p => p.ProductionOrderNumber, p => p.SerialNumber, p => p.SerialNumber,
               p => p.Quantity, p => p.Size, p => p.Class, p => p.SizeDeck, p => p.Media, p => p.SpareParts,
               p => p.SparePMedia, p => p.Base, p => p.AirSeal, p => p.CoatingLining, p => p.Disassembly,
               p => p.BudgetedHours, p => p.ActualAssemblyHours,p => p.ReworkHours, p => p.Nameplate,
               p => p.PreOrder, p => p.Scope, p => p.SalesOrderID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { machinesToUpdate.ID });

                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Machines.SerialNumber"))
                    {
                        ModelState.AddModelError("SerialNumber", "Unable to save changes. Remember, you cannot have duplicate Serial Number.");
                    }
                    else if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Machines.ProductionOrderNumber"))
                    {
                        ModelState.AddModelError("ProductionOrderNumber", "Unable to save changes. Remember, you cannot have duplicate Production Order Number.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }


            }
            PopulateDropDownLists(machinesToUpdate); 
            return View(machinesToUpdate);
        }

        // GET: Machine/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines
                .Include(m => m.SalesOrder)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (machine == null)
            {
                return NotFound();
            }

            return View(machine);
        }

        // POST: Machine/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var machine = await _context.Machines
                 .Include(s => s.SalesOrder)
                  .FirstOrDefaultAsync(m => m.ID == id);

            try
            {
                if (machine != null)
                {
                    _context.Machines.Remove(machine);
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
                //Note: there is really no reason a delete should fail if you can "talk" to the database.
                ModelState.AddModelError("", "Unable to delete record. Try again, and if the problem persists see your system administrator.");
            }
            return View(machine);


        }

  

        private bool MachineExists(int id)
        {
            return _context.Machines.Any(e => e.ID == id);
        }

        private void PopulateDropDownLists(Machine? machine = null)
        {
            var dQuery = from d in _context.SalesOrders
                         select d;
            ViewData["SalesOrderID"] = new SelectList(dQuery, "ID", "OrderNumber", machine?.SalesOrderID);
        }
    }
}
