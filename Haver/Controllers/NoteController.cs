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
    public class NoteController : Controller
    {
        private readonly HaverContext _context;

        public NoteController(HaverContext context)
        {
            _context = context;
        }

        // GET: Note
        public async Task<IActionResult> Index()
        {
            var haverContext = _context.Notes.Include(n => n.MachineSchedule);
            return View(await haverContext.ToListAsync());
        }

        // GET: Note/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
<<<<<<< HEAD
                .AsNoTracking()
=======
                .Include(n => n.MachineSchedule)
>>>>>>> 72ec3151358c738571b34bf22b29aa45f8631ede
                .FirstOrDefaultAsync(m => m.ID == id);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        // GET: Note/Create
        public IActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        }

        // POST: Note/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,PreOrder,Scope,AssemblyHours,ReworkHours,BudgetHours,NamePlate,MachineScheduleID")] Note note)
        {

            try
            {

                if (ModelState.IsValid)
                {
                    _context.Add(note);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            PopulateDropDownLists(note);
            return View(note);
        }

        // GET: Note/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(note);
            return View(note);
        }

        // POST: Note/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
<<<<<<< HEAD
        public async Task<IActionResult> Edit(int id)
        {

            //Go get the note to update
            var noteToUpdate = await _context.Notes.FirstOrDefaultAsync(c => c.ID == id);
=======
<<<<<<< HEAD
        public async Task<IActionResult> Edit(int id)
=======
        public async Task<IActionResult> Edit(int id, [Bind("ID,PreOrder,Scope,AssemblyHours,ReworkHours,BudgetHours,NamePlate,MachineScheduleID")] Note note)
>>>>>>> 72ec3151358c738571b34bf22b29aa45f8631ede
        {
            var noteToUpdate = await _context.Notes.FirstOrDefaultAsync(n => n.ID == id);
>>>>>>> e8abcea7fc1b155af8e55d167416451dde874354

            if (noteToUpdate == null)
            {
                return NotFound();
            }

<<<<<<< HEAD
            if (await TryUpdateModelAsync<Note>(noteToUpdate, "",
                  p => p.PreOrder, p => p.Scope, p => p.AssemblyHours, p => p.ReworkHours,
                 p => p.BudgetHours, p => p.NamePlate,p => p.MachineScheduleID))
=======
            if (await TryUpdateModelAsync<Note>(noteToUpdate, "", n => n.PreOrder,
                    n => n.Scope, n => n.AssemblyHours, n => n.ReworkHours, n => n.BudgetHours,
                    n => n.NamePlate, n => n.MachineScheduleID))
>>>>>>> e8abcea7fc1b155af8e55d167416451dde874354
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NoteExists(noteToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
<<<<<<< HEAD
                }      
            }     
            return View(noteToUpdate);
=======
                }
                catch (DbUpdateException dex)
                {
                  
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }

            }
<<<<<<< HEAD
            PopulateDropDownLists(noteToUpdate);
            return View(noteToUpdate);
=======
            ViewData["MachineScheduleID"] = new SelectList(_context.MachineSchedules, "ID", "ID", note.MachineScheduleID);
            return View(note);
>>>>>>> 72ec3151358c738571b34bf22b29aa45f8631ede
>>>>>>> e8abcea7fc1b155af8e55d167416451dde874354
        }

        // GET: Note/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
<<<<<<< HEAD
                .AsNoTracking()
=======
                .Include(n => n.MachineSchedule)
>>>>>>> 72ec3151358c738571b34bf22b29aa45f8631ede
                .FirstOrDefaultAsync(m => m.ID == id);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        // POST: Note/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
<<<<<<< HEAD
            var note = await _context.Notes.FindAsync(id);
=======
            var note = await _context.Notes
                .FirstOrDefaultAsync(m => m.ID == id);
            if (note != null)
            {
                _context.Notes.Remove(note);
            }
>>>>>>> e8abcea7fc1b155af8e55d167416451dde874354

            try
            {
                if (note != null)
                {
                    _context.Notes.Remove(note);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                
            }
            return View(note);
           
          
        }


        private void PopulateDropDownLists(Note? note = null)
        {
            ViewData["MachineScheduleID"] = new SelectList(_context.MachineSchedules, "ID", "ID");
        }
        private bool NoteExists(int id)
        {
            return _context.Notes.Any(e => e.ID == id);
        }
    }
}
