using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using haver.Data;
using haver.Models;

namespace haver.Controllers
{
    public class GanttDataController : Controller
    {
        private readonly HaverContext _context;

        public GanttDataController(HaverContext context)
        {
            _context = context;
        }

        // GET: GanttData
        public async Task<IActionResult> Index()
        {
            var haverContext = _context.GanttDatas.Include(g => g.Machine);
            return View(await haverContext.ToListAsync());
        }

        // GET: GanttData/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganttData = await _context.GanttDatas
                .Include(g => g.Machine)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (ganttData == null)
            {
                return NotFound();
            }

            return View(ganttData);
        }


        // GET: GanttData/GetMachineData/5
        public IActionResult GetMachineData(int machineID)
        {
            Console.WriteLine($"Machine ID: {machineID}");

            var machine = _context.Machines
                                 .Include(m => m.SalesOrder)
                                 .FirstOrDefault(m => m.ID == machineID);

            if (machine == null)
            {
                Console.WriteLine("Machine not found");
                return Json(new { success = false, message = "Machine not found" });
            }

            var data = new
            {
                EngExpected = machine.SalesOrder?.EngPExp?.ToString("yyyy-MM-dd"),
                EngReleased = machine.SalesOrder?.EngPRel?.ToString("yyyy-MM-dd"),
                //CustomerApproval = machine.SalesOrder?.AppDwgRel?.ToString("yyyy-MM-dd"),
                //PackageReleased = machine.SalesOrder?.PreORel?.ToString("yyyy-MM-dd"),
                PurchaseOrdersIssued = machine.SalesOrder?.SoDate.ToString("yyy-MM-dd"),
                //PurchaseOrdersDue = machine.SalesOrder?.
                //SupplierPODue = machine.SalesOrder?.SupplierPODue?.ToString("yyyy-MM-dd"),  // ✅ Now included
                //AssemblyStart = machine?.AssemblyStart?.ToString("yyyy-MM-dd"),  // ✅ Now included
                //AssemblyComplete = machine?.AssemblyComplete?.ToString("yyyy-MM-dd"),
                //ShipExpected = machine.RToShipExp?.ToString("yyyy-MM-dd"),
                ShipActual = machine.RToShipA?.ToString("yyyy-MM-dd"),
                DeliveryExpected = machine.SalesOrder?.AppDwgRet?.ToString("yyyy-MM-dd"),
                DeliveryActual = machine.SalesOrder?.PreOExp.ToString("yyyy-MM-dd")
            };

            return Json(data);
        }



        // GET: GanttData/Create
        public IActionResult Create()
        {
            ViewData["MachineID"] = new SelectList(_context.Machines, "ID", "ProductionOrderNumber");
            return View();
        }

        // POST: GanttData/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,MachineID,EngExpected,EngReleased,CustomerApproval,PackageReleased,PurchaseOrdersIssued,PurchaseOrdersCompleted,SupplierPODue,AssemblyStart,AssemblyComplete,ShipExpected,ShipActual,DeliveryExpected,DeliveryActual,Notes")] GanttData ganttData)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ganttData);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MachineID"] = new SelectList(_context.Machines, "ID", "ProductionOrderNumber", ganttData.MachineID);
            return View(ganttData);
        }

        // GET: GanttData/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganttData = await _context.GanttDatas.FindAsync(id);
            if (ganttData == null)
            {
                return NotFound();
            }
            ViewData["MachineID"] = new SelectList(_context.Machines, "ID", "ProductionOrderNumber", ganttData.MachineID);
            return View(ganttData);
        }

        // POST: GanttData/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,MachineID,EngExpected,EngReleased,CustomerApproval,PackageReleased,PurchaseOrdersIssued,PurchaseOrdersCompleted,SupplierPODue,AssemblyStart,AssemblyComplete,ShipExpected,ShipActual,DeliveryExpected,DeliveryActual,Notes")] GanttData ganttData)
        {
            if (id != ganttData.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ganttData);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GanttDataExists(ganttData.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MachineID"] = new SelectList(_context.Machines, "ID", "ProductionOrderNumber", ganttData.MachineID);
            return View(ganttData);
        }

        // GET: GanttData/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ganttData = await _context.GanttDatas
                .Include(g => g.Machine)
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
            var ganttData = await _context.GanttDatas.FindAsync(id);
            if (ganttData != null)
            {
                _context.GanttDatas.Remove(ganttData);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GanttDataExists(int id)
        {
            return _context.GanttDatas.Any(e => e.ID == id);
        }
    }
}
