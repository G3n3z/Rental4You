using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Escola_Segura.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rental4You.Data;

namespace Rental4You.Models
{
	public class CategoriasController : Controller
	{
		private readonly ApplicationDbContext _context;

		public CategoriasController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: Categorias
		[Authorize(Roles = "Admin, Gestor, Funcionario")]
		public async Task<IActionResult> Index()
		{
			return _context.Categorias != null ?
						View(await _context.Categorias.Include(c => c.Veiculos).ToListAsync()) :
						Problem("Entity set 'ApplicationDbContext.Categorias'  is null.");
		}

		// GET: Categorias/Details/5
		[Authorize(Roles = "Admin, Gestor, Funcionario")]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _context.Categorias == null)
			{
				return NotFound();
			}

			var categoria = await _context.Categorias
				.FirstOrDefaultAsync(m => m.Id == id);
			if (categoria == null)
			{
				return NotFound();
			}

			return View(categoria);
		}

		// GET: Categorias/Create
		[Authorize(Roles = "Admin")]
		public IActionResult Create()
		{
			return View();
		}

		// POST: Categorias/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Create([Bind("Id,Nome")] Categoria categoria)
		{
			ModelState.Remove(nameof(categoria.Veiculos));
			if (ModelState.IsValid)
			{
				_context.Add(categoria);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(categoria);
		}

		// GET: Categorias/Edit/5
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null || _context.Categorias == null)
			{
				return NotFound();
			}

			var categoria = await _context.Categorias.FindAsync(id);
			if (categoria == null)
			{
				return NotFound();
			}
			return View(categoria);
		}

		// POST: Categorias/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] Categoria categoria)
		{
			if (id != categoria.Id)
			{
				return NotFound();
			}
			ModelState.Remove(nameof(categoria.Veiculos));
			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(categoria);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!CategoriaExists(categoria.Id))
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
			return View(categoria);
		}

		// GET: Categorias/Delete/5
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || _context.Categorias == null)
			{
				return NotFound();
			}

			var categoria = await _context.Categorias
				.Include(m => m.Veiculos)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (categoria == null)
			{
				return NotFound();
			}
			if (categoria.Veiculos != null && categoria.Veiculos.Count() != 0)
			{
				return BadRequest();
			}

			return View(categoria);
		}

		// POST: Categorias/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (_context.Categorias == null)
			{
				return Problem("Entity set 'ApplicationDbContext.Categorias'  is null.");
			}
			var categoria = await _context.Categorias.FindAsync(id);
			if(categoria == null)
            {
				return NotFound();
            }

            if (categoria.Veiculos != null && categoria.Veiculos.Count() != 0)
            {
                return BadRequest();
            }
            if (categoria != null)
            {
                _context.Categorias.Remove(categoria);
            }

            await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool CategoriaExists(int id)
		{
			return (_context.Categorias?.Any(e => e.Id == id)).GetValueOrDefault();
		}
	}
}
