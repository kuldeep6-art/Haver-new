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
        public async Task<IActionResult> Index()
        {
            return View(await _context.Engineers.ToListAsync());
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
                    return RedirectToAction("Details", new { engineer.ID });
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Engineers.Phone"))
                {
                    ModelState.AddModelError("Phone", "Unable to save changes. Remember, you cannot have duplicate Phone numbers.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
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
                n => n.LastName, n => n.Phone, n=> n.Email )))
            {
                try
                {
                    await _context.SaveChangesAsync();
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
                   
                    
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    
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
