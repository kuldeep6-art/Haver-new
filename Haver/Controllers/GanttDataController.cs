using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using haver.Data;
using haver.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using haver.ViewModels;
using haver.CustomControllers;
using haver.Utilities;

namespace haver.Controllers
{
    public class GanttDataController : ElephantController
    {
        private readonly HaverContext _context;

        public GanttDataController(HaverContext context)
        {
            _context = context;
        }

        // GET: GanttData
        public async Task<IActionResult> Index(int? page, int? pageSizeID,DateTime? DtString,
            string? SearchString, string? actionButton, string sortDirection = "asc", string sortField = "Order Number", bool? isFinalized = null)
        {
            string[] sortOptions = new[] { "Order Number" };

            // Default filter to Active (not finalized) if isFinalized is null
            if (!isFinalized.HasValue)
            {
                isFinalized = false;
            }

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;


            var gData = from g in _context.GanttDatas
                        .Include(g => g.SalesOrder)
                .ThenInclude(m => m.Machines)
                .Include(m => m.Machine).ThenInclude(m => m.MachineType)
                .AsNoTracking()
                        select g;


            // Apply `isFinalized` filter only if it's not null
            if (isFinalized.HasValue)
            {
                gData = gData.Where(v => v.IsFinalized == isFinalized.Value);
            }

            // Set ViewBag.Status to control which tab is active
            ViewBag.Status = isFinalized.Value ? "Finalized" : "Active";


            if (!String.IsNullOrEmpty(SearchString))
            {
                gData = gData.Where(p => p.SalesOrder.OrderNumber.Contains(SearchString));
                numberFilters++;
            }
            if (DtString.HasValue)
            {
                DateTime searchDae = DtString.Value.Date;
                gData = gData.Where(s => s.AppDRcd.HasValue &&
                                         s.AppDRcd.Value.Year == searchDae.Year &&
                                         s.AppDRcd.Value.Month == searchDae.Month &&
                                         s.AppDRcd.Value.Day == searchDae.Day);
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
            if (sortField == "Order Number")
            {
                if (sortDirection == "asc")
                {
                    gData = gData
                        .OrderByDescending(p => p.SalesOrder.OrderNumber);
                }
                else
                {
                    gData = gData
                        .OrderBy(p => p.SalesOrder.OrderNumber);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<GanttData>.CreateAsync(gData.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: GanttData/Details/5
        public IActionResult Details(int? id)
        {
            var ganttData = _context.GanttDatas
                 .Include(g => g.SalesOrder)
                .ThenInclude(m => m.Machines)
                .Include(m => m.Machine).ThenInclude(m => m.MachineType)
                .FirstOrDefault(g => g.ID == id);

            if (ganttData == null)
            {
                return NotFound();
            }

            var ganttTasks = GetMilestoneTasks(ganttData);

            var viewModel = new GanttDetailsViewModel
            {
                GanttData = ganttData,
                GanttTasks = ganttTasks
            };

            return View(viewModel);
        }


        // GET: GanttData/GetMachineData/5
        public IActionResult GetMachineData(int salesOrderID)
        {
            //Console.WriteLine($"Machine ID: {machineID}");

            // Find the Sales Order to check if the SalesOrderID is valid
            var salesOrder = _context.SalesOrders.FirstOrDefault(so => so.ID == salesOrderID);
            if (salesOrder == null)
            {
                return Json(new { success = false, message = "Sales Order not found" });
            }


            var machines = _context.Machines

                                      .Where(m => m.SalesOrderID == salesOrderID)
                                      .ToList();

            if (!machines.Any())
            {
                return Json(new { success = false, message = "No machines found for the selected Sales Order." });
            }

            var data = machines.Select(m => new
            {
                AppDRcd = m.SalesOrder?.AppDwgRel?.ToString("yyyy-MM-dd"),
                EngExpected = m.SalesOrder?.EngPExp?.ToString("yyyy-MM-dd"),
                EngReleased = m.SalesOrder?.EngPRel?.ToString("yyyy-MM-dd"),
                //CustomerApproval = machine.SalesOrder?.AppDwgRel?.ToString("yyyy-MM-dd"),
                //PackageReleased = machine.SalesOrder?.PreORel?.ToString("yyyy-MM-dd"),
                PurchaseOrdersIssued = m.SalesOrder?.SoDate.ToString("yyy-MM-dd"),
                //PurchaseOrdersDue = machine.SalesOrder?.
                //SupplierPODue = machine.SalesOrder?.SupplierPODue?.ToString("yyyy-MM-dd"),  // ✅ Now included
                //AssemblyStart = machine?.AssemblyStart?.ToString("yyyy-MM-dd"),  // ✅ Now included
                //AssemblyComplete = machine?.AssemblyComplete?.ToString("yyyy-MM-dd"),
                ShipExpected = m.RToShipExp?.ToString("yyyy-MM-dd"),
                ShipActual = m.RToShipA?.ToString("yyyy-MM-dd"),
                // DeliveryExpected = machine.SalesOrder?.AppDwgRet?.ToString("yyyy-MM-dd"),
                //DeliveryActual = machine.SalesOrder?.PreOExp.ToString("yyyy-MM-dd")
            });

            return Json(new { success = true, machines = data });
        }



        // GET: GanttData/Create
        // GET: GanttData/Create
        public IActionResult Create()
        {
            // Get all Sales Orders for selection
            var salesOrders = _context.SalesOrders.ToList();
            ViewData["SalesOrderID"] = new SelectList(salesOrders, "ID", "OrderNumber");

            return View();
        }



        // POST: GanttData/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SalesOrderID,AppDRcd,EngExpected,EngReleased,CustomerApproval,PackageReleased,PurchaseOrdersIssued,PurchaseOrdersCompleted,SupplierPODue,AssemblyStart,AssemblyComplete,ShipExpected,ShipActual,DeliveryExpected,DeliveryActual,Notes")] GanttData ganttData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get all machines linked to the selected Sales Order
                    var machines = _context.Machines
                                           .Where(m => m.SalesOrderID == ganttData.SalesOrderID)
                                           .ToList();

                    if (machines.Any())
                    {
                        foreach (var machine in machines)
                        {
                            var newGanttData = new GanttData
                            {
                                SalesOrderID = ganttData.SalesOrderID,
                                MachineID = machine.ID, // Assign MachineID
                                AppDRcd = ganttData.AppDRcd,
                                EngExpected = ganttData.EngExpected,
                                EngReleased = ganttData.EngReleased,
                                CustomerApproval = ganttData.CustomerApproval,
                                PackageReleased = ganttData.PackageReleased,
                                PurchaseOrdersIssued = ganttData.PurchaseOrdersIssued,
                                PurchaseOrdersCompleted = ganttData.PurchaseOrdersCompleted,
                                SupplierPODue = ganttData.SupplierPODue,
                                AssemblyStart = ganttData.AssemblyStart,
                                AssemblyComplete = ganttData.AssemblyComplete,
                                ShipExpected = ganttData.ShipExpected,
                                ShipActual = ganttData.ShipActual,
                                DeliveryExpected = ganttData.DeliveryExpected,
                                DeliveryActual = ganttData.DeliveryActual,
                                Notes = ganttData.Notes,
                                IsFinalized = false, // Default to false
                                StartOfWeek = WeekStartOption.Monday // Or dynamically set based on user input
                            };

                            _context.GanttDatas.Add(newGanttData);
                        }

                        await _context.SaveChangesAsync();
                        TempData["Message"] = "Gantt Data has been successfully created for all machines in the Sales Order.";
                    }
                    else
                    {
                        TempData["Error"] = "No machines found for the selected Sales Order.";
                    }

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            // Re-populate dropdown with Sales Orders
            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber");

            return View(ganttData);
        }




        // GET: GanttData/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganttData = await _context.GanttDatas
                .Include(g => g.SalesOrder).ThenInclude(m => m.Machines)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (ganttData == null)
            {
                return NotFound();
            }
            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber");
            return View(ganttData);
        }

        // POST: GanttData/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var gDataToUpdate = await _context.GanttDatas
       .Include(g => g.SalesOrder)
       .Include(g => g.Machine) // Ensure related data is loaded
       .FirstOrDefaultAsync(e => e.ID == id);

            if (gDataToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<GanttData>(gDataToUpdate, "",
                 p => p.SalesOrderID, p => p.AppDRcd, p => p.AppDExp, p => p.EngExpected, p => p.EngReleased, p => p.CustomerApproval,
                 p => p.CustomerApproval, p => p.PackageReleased, p => p.PurchaseOrdersIssued, p => p.PurchaseOrdersCompleted, p => p.PurchaseOrdersReceived,
                  p => p.SupplierPODue, p => p.AssemblyStart,p => p.AssemblyComplete, p => p.ShipExpected, p => p.DeliveryExpected, p => p.DeliveryActual, p => p.Notes))
            {
                try
                {
                    bool isUpdated = false;

                    // Ensure related SalesOrder and Machine reflect changes
                    if (gDataToUpdate.SalesOrder != null)
                    {
                        if (gDataToUpdate.AppDExp.HasValue && gDataToUpdate.SalesOrder.AppDwgExp != gDataToUpdate.AppDExp.Value)
                        {
                            gDataToUpdate.SalesOrder.AppDwgExp = gDataToUpdate.AppDExp.Value;
                            isUpdated = true;
                        }


                        if (gDataToUpdate.AppDRcd != null && gDataToUpdate.SalesOrder.AppDwgRel != gDataToUpdate.AppDRcd)
                        {
                            gDataToUpdate.SalesOrder.AppDwgRel = gDataToUpdate.AppDRcd;
                            isUpdated = true;
                        }

                        if (gDataToUpdate.EngReleased != null && gDataToUpdate.SalesOrder.EngPRel != gDataToUpdate.EngReleased)
                        {
                            gDataToUpdate.SalesOrder.EngPRel = gDataToUpdate.EngReleased;
                            isUpdated = true;
                        }

                        //if (gDataToUpdate.ShipExpected != null && gDataToUpdate.SalesOrder.ExpectedShippingDate != gDataToUpdate.ShipExpected)
                        //{
                        //    gDataToUpdate.SalesOrder.ExpectedShippingDate = gDataToUpdate.ShipExpected;
                        //    isUpdated = true;
                        //}
                    }

                    if (gDataToUpdate.Machine != null)
                    {
                        //if (gDataToUpdate.EngReleased != null && gDataToUpdate.Machine.EngineeringReleaseDate != gDataToUpdate.EngReleased)
                        //{
                        //    gDataToUpdate.Machine.EngineeringReleaseDate = gDataToUpdate.EngReleased;
                        //    isUpdated = true;
                        //}

                        if (gDataToUpdate.AssemblyStart != null && gDataToUpdate.Machine.AssemblyStart != gDataToUpdate.AssemblyStart)
                        {
                            gDataToUpdate.Machine.AssemblyStart = gDataToUpdate.AssemblyStart;
                            isUpdated = true;
                        }

                        if (gDataToUpdate.AssemblyComplete != null && gDataToUpdate.Machine.AssemblyComplete != gDataToUpdate.AssemblyComplete)
                        {
                            gDataToUpdate.Machine.AssemblyComplete = gDataToUpdate.AssemblyComplete;
                            isUpdated = true;
                        }

                        if (gDataToUpdate.ShipExpected != null && gDataToUpdate.Machine.RToShipExp != gDataToUpdate.ShipExpected)
                        {
                            gDataToUpdate.Machine.RToShipExp = gDataToUpdate.ShipExpected;
                            isUpdated = true;
                        }

                        if (gDataToUpdate.ShipActual != null && gDataToUpdate.Machine.RToShipA != gDataToUpdate.ShipActual)
                        {
                            gDataToUpdate.Machine.RToShipA = gDataToUpdate.ShipActual;
                            isUpdated = true;
                        }
                    }

                    // Save changes if updates were made
                    if (isUpdated)
                    {
                        await _context.SaveChangesAsync();
                    }
                    TempData["Message"] = "Gantt Data has been successfully edited, Neccesary Dates have been updated on the sales order and machine";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GanttDataExists(gDataToUpdate.ID))
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
            ViewData["SalesOrderID"] = new SelectList(_context.SalesOrders, "ID", "OrderNumber");
            return View(gDataToUpdate);
        }

        // GET: GanttData/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganttData = await _context.GanttDatas
                .Include(g => g.SalesOrder).ThenInclude(m => m.Machines)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (ganttData == null)
            {
                return NotFound();
            }

            return View(ganttData);
        }

        // POST: GanttData/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ganttData = await _context.GanttDatas
                .Include(g => g.Machine)
                 .Include(g => g.Machine).ThenInclude(s => s.SalesOrder)
                 .Include(g => g.Machine).ThenInclude(s => s.MachineType)
                .FirstOrDefaultAsync(m => m.ID == id);

            try
            {
                if (ganttData != null)
                {
                    _context.GanttDatas.Remove(ganttData);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            { 
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(ganttData);

        }


        //public IActionResult Chart()
        //{
        //    var ganttData = _context.GanttDatas
        //        .Include(g => g.Machine) // Ensure Machine is loaded
        //        .ToList() // Convert to memory before calling a custom method
        //        .Select(g => new GanttViewModel
        //        {
        //            ID = g.ID,
        //            MachineName = g.Machine?.Description ?? "Unknown",
        //            StartDate = g.AppDRcd,
        //            EndDate = g.DeliveryExpected,
        //            Progress = 0,
        //            MilestoneClass = GetMilestoneClass(g) // Now safe to use
        //        })
        //        .ToList();

        //    return View(ganttData);
        //}

        public IActionResult Chart()
        {
            var ganttData = _context.GanttDatas
                .Include(g => g.SalesOrder)
                .Include(g => g.Machine)
                .ToList() // Fetch all data first
                .SelectMany(g => GetMilestoneTasks(g)) // Break into multiple segments per machine
                .ToList();

            return View(ganttData);
        }

        private List<GanttViewModel> GetMilestoneTasks(GanttData g)
        {
            var tasks = new List<GanttViewModel>();

            if (g.Machine == null) // Safety check
                return tasks;

            // Define only the required milestones with colors
            var milestones = new List<(DateTime? Start, DateTime? End, string Name, string Color)>
    {
        (g.EngExpected, g.EngReleased, "Engineering Released to Customer", "#FABF8F"),
        (g.CustomerApproval, g.PackageReleased, "Customer Approval Received", "#E26B0A"),
        (g.PackageReleased, g.PackageReleased, "Package Released to PIC/Spare Parts to Customer Service", "#ADF802"),
        (g.PurchaseOrdersIssued, g.PurchaseOrdersCompleted, "Purchase Orders Issued", "#8DB4E2"),
        (g.SupplierPODue, g.SupplierPODue, "Supplier Purchase Orders Due", "#FF99CC"),
        (g.AssemblyStart, g.AssemblyComplete, "Machine Assembly and Testing", "#00B050")
    };

            // Generate Gantt tasks only for valid milestones
            foreach (var (start, end, name, color) in milestones)
            {
                if (start.HasValue && end.HasValue)
                {
                    tasks.Add(new GanttViewModel
                    {
                        ID = g.ID,
                        UniqueID = $"{g.ID}-{g.Machine.ID}-{name}", // Ensure uniqueness
                        MachineName = $"{g.Machine.Description} - {name}", // Reference single machine
                        StartDate = start.Value,
                        EndDate = end.Value,
                        Progress = 100,
                        MilestoneClass = color
                    });
                }
            }

            return tasks;
        }



        //private string GetMilestoneClass(GanttData g)
        //{
        //    if (g.EngReleased.HasValue) return "eng-released";
        //    if (g.PackageReleased.HasValue) return "package-released";
        //    if (g.ShipExpected.HasValue) return "shipping";
        //    if (g.DeliveryExpected.HasValue) return "delivery";
        //    return "default-task";
        //}

        public async Task<IActionResult> FinalizeGantt(int id)
        {
            var gantt = _context.GanttDatas.Find(id);
            if (gantt == null)
            {
                return NotFound();
            }

            gantt.IsFinalized = true;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Gantt chart finalized successfully!";
            return RedirectToAction("Index", new { isFinalized = true });
        }
        public async Task<IActionResult> UnfinalizeGantt(int id)
        {
            var gantt = _context.GanttDatas.Find(id);
            if (gantt == null)
            {
                return NotFound();
            }

            gantt.IsFinalized = false;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Gantt chart has been unfinalized.";
            return RedirectToAction("Index", new { isFinalized = false });
        }


        private bool GanttDataExists(int id)
        {
            return _context.GanttDatas.Any(e => e.ID == id);
        }
    }
}
