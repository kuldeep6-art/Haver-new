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
    public class MachineController : ElephantController
    {
        private readonly HaverContext _context;

        public MachineController(HaverContext context)
        {
            _context = context;
        }

        // GET: Machine
        public async Task<IActionResult> Index(int? page, int? pageSizeID, int? MachineTypeID, string? PoString, string? SerString,
            string? actionButton, string sortDirection = "asc", string sortField = "Serial Number")
        {
            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Serial Number","Production Order Number","Sales Order","Machine Type" };

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied

            PopulateDropDownLists();

            var machines = from s in _context.Machines
                               .Include(m => m.MachineType)
							  .Include(s => s.SalesOrder)
							  .AsNoTracking()
						   select s;

            if (MachineTypeID.HasValue)
            {
                machines = machines.Where(p => p.MachineTypeID == MachineTypeID);
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
            if (sortField == "Production Order Number")
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
            else if (sortField == "Sales Order")
            {
                if (sortDirection == "asc")
                {
                    machines = machines
                        .OrderByDescending(p => p.SalesOrder.OrderNumber);
                }
                else
                {
                    machines = machines
                        .OrderBy(p => p.SalesOrder.OrderNumber);
                }
            }
            else if (sortField == "Machine Type")
            {
                if (sortDirection == "asc")
                {
                    machines = machines
                        .OrderByDescending(p => p.MachineType.Description);
                }
                else
                {
                    machines = machines
                        .OrderBy(p => p.MachineType.Description);
                }
            }
            else 
            {
                if (sortDirection == "asc")
                {
                    machines = machines
                        .OrderByDescending(p => p.SerialNumber);
                }
                else
                {
                    machines = machines
                        .OrderBy(p => p.SerialNumber);
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
                .Include(m => m.MachineType)
                .Include(m => m.SalesOrder)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (machine == null)
            {
                return NotFound();
            }

            return View(machine);
        }

        // GET: Machine/Create
        public IActionResult Create(int? salesOrderId)
        {
            var machine = new Machine();

            // If a SalesOrderID is provided, preselect it
            if (salesOrderId.HasValue)
            {
                machine.SalesOrderID = salesOrderId.Value;
            }

            PopulateDropDownLists();
            return View(machine);
        }

        // POST: Machine/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Machine/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,SerialNumber,ProductionOrderNumber,AssemblyExp,AssemblyStart,AssemblyComplete,RToShipExp,RToShipA,BudgetedHours,ActualAssemblyHours,ReworkHours,Nameplate,PreOrder,Scope,SalesOrderID,MachineTypeID")] Machine machine)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Step 1: Add the machine to the database
                    _context.Add(machine);
                    await _context.SaveChangesAsync();

                    // Step 2: Create the corresponding Gantt record
                    await CreateGanttForMachine(machine);

                    // Step 3: Set a message and redirect
                    TempData["Message"] = "Machine has been successfully created and Gantt record added.";
                    return RedirectToAction("Index", "MachineProcurement", new { MachineID = machine.ID });
                }
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

            PopulateDropDownLists(machine);
            return View(machine);
        }

        // Method to create Gantt record for the newly created machine
        public async Task CreateGanttForMachine(Machine machine)
        {
            // Step 1: Retrieve the Sales Order that the machine belongs to, including related machines and procurements
            var salesOrder = await _context.SalesOrders
                .Include(so => so.Machines)   // Include related Machines
                .ThenInclude(m => m.Procurements)  // Include related Procurements for each machine
                .FirstOrDefaultAsync(so => so.ID == machine.SalesOrderID);

            if (salesOrder != null)
            {
                // Step 2: Retrieve the Procurement related to this specific machine (if it exists)
                var procurement = machine.Procurements.FirstOrDefault();

                // Step 3: Create a Gantt record for the machine
                var gantt = new GanttData
                {
                    SalesOrderID = machine.SalesOrderID,
                    MachineID = machine.ID,
                    AppDExp = salesOrder.AppDwgExp,
                    AppDRcd = salesOrder.AppDwgRel,
                    //PreORel = salesOrder.PreORel,
                    //PreOExp = salesOrder.PreOExp,
                    EngExpected = salesOrder.EngPExp,
                    EngReleased = salesOrder.EngPRel,
                    PurchaseOrdersIssued = salesOrder.SoDate, 
                    PurchaseOrdersCompleted = salesOrder.SoDate,
                    PurchaseOrdersReceived = salesOrder.SoDate,
                    AssemblyStart = machine.AssemblyStart,
                    AssemblyComplete = machine.AssemblyComplete,
                    ShipExpected = machine.RToShipExp,
                    ShipActual = machine.RToShipA
                };

                // Step 4: Add the Gantt record to the database
                _context.GanttDatas.Add(gantt);
                await _context.SaveChangesAsync();
            }
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
            PopulateDropDownLists();
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
                  p => p.SerialNumber, p => p.ProductionOrderNumber,p => p.AssemblyExp,p => p.AssemblyStart,p => p.AssemblyComplete,
              p => p.RToShipExp, p => p.RToShipA,
              p => p.BudgetedHours, p => p.ActualAssemblyHours, p => p.ReworkHours, p => p.Nameplate,
              p => p.PreOrder, p => p.Scope, p => p.SalesOrderID, p => p.MachineTypeID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    //TempData["Message"] = "Machine has been successfully Edited";
                    return RedirectToAction("Index", "MachineProcurement", new { SalesOrderID = machinesToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MachineExists(machinesToUpdate.ID))
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
                .Include(m => m.MachineType)
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
                 .Include(m => m.MachineType)
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
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Machine.  Remember, you cannot delete a Machine attached to a Sales Order");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(machine);
        }

        private SelectList MachineTypeList(int? selectedId)
        {
            return new SelectList(_context
                .MachineTypes
                .AsEnumerable() // Move data to memory
                .OrderBy(m => m.Description), // Now sorting works
                "ID",
                "Description",
                selectedId);
        }


        [HttpGet]
        public JsonResult GetMachineTypes(int? id)
        {
            return Json(MachineTypeList(id));
        }


        private void PopulateDropDownLists(Machine? machine = null)
        {
            ViewData["MachineTypeID"] = MachineTypeList(machine?.MachineTypeID);
            // Filter SalesOrders where Status is InProgress
            ViewData["SalesOrderID"] = new SelectList(
                _context.SalesOrders.Where(so => so.Status == Status.InProgress),
                "ID",
                "MachineOrderDetail"
            );
        }


      

        private bool MachineExists(int id)
        {
            return _context.Machines.Any(e => e.ID == id);
        }
    }
}
