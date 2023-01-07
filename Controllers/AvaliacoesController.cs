using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rental4You.Data;

namespace Rental4You.Models
{
    public class AvaliacoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AvaliacoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Avaliacoes
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Avaliacoes.Include(a => a.Empresa);
            return View(await applicationDbContext.ToListAsync());
        }


        // POST: Avaliacoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ReservaId,EmpresaId,Nota")] Avaliacao avaliacao)
        {
            ModelState.Remove(nameof(avaliacao.Empresa));
            ModelState.Remove(nameof(avaliacao.Reserva));
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(avaliacao);
                    await _context.SaveChangesAsync();
                    var media = _context.Avaliacoes.Where(a => a.EmpresaId == avaliacao.EmpresaId).Select(a => a.Nota).Average();
                    var empresa = _context.Empresas.FirstOrDefault(e => e.Id == avaliacao.EmpresaId);
                    if (empresa == null)
                    {
						return NotFound();
					}
                    empresa.MediaAvaliacao = media;
                
                    _context.Update(empresa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
					return BadRequest();
				}
                return RedirectToAction("Index","Reservas");
            }
            
                ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Id", avaliacao.EmpresaId);
            return RedirectToAction("Details", "Reservas", new { id = avaliacao.ReservaId});
        }

        // POST: Avaliacoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Edit(int id, [Bind("Id,EmpresaId")] Avaliacao avaliacao)
        {
            if (id != avaliacao.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(avaliacao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AvaliacaoExists(avaliacao.Id))
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
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Id", avaliacao.EmpresaId);
            return View(avaliacao);
        }

        private bool AvaliacaoExists(int id)
        {
            return (_context.Avaliacoes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
