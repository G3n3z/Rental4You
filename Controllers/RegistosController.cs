using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rental4You.Data;

namespace Rental4You.Models
{
    public class RegistosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Registos
        public async Task<IActionResult> Index(Boolean? pendentes, string flag)
        {
            

              return _context.Registos != null ? 
                          View(await _context.Registos.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Registos'  is null.");
        }

        // GET: Registos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Registos == null)
            {
                return NotFound();
            }

            var registo = await _context.Registos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registo == null)
            {
                return NotFound();
            }

            return View(registo);
        }

        // GET: Registos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Registos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Data,Kms,Danos,Observacoes")] Registo registo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(registo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(registo);
        }

        // GET: Registos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Registos == null)
            {
                return NotFound();
            }

            var registo = await _context.Registos.FindAsync(id);
            if (registo == null)
            {
                return NotFound();
            }
            return View(registo);
        }

        // POST: Registos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Data,Kms,Danos,Observacoes")] Registo registo)
        {
            if (id != registo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(registo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegistoExists(registo.Id))
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
            return View(registo);
        }

        // GET: Registos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Registos == null)
            {
                return NotFound();
            }

            var registo = await _context.Registos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registo == null)
            {
                return NotFound();
            }

            return View(registo);
        }

        // POST: Registos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Registos == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Registos'  is null.");
            }
            var registo = await _context.Registos.FindAsync(id);
            if (registo != null)
            {
                _context.Registos.Remove(registo);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RegistoExists(int id)
        {
          return (_context.Registos?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
