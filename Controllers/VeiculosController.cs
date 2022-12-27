using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Escola_Segura.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Rental4You.Data;

namespace Rental4You.Models
{
	public class VeiculosController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public VeiculosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		// GET: Veiculos
        [Authorize(Roles = "Admin,Gestor, Funcionario")]
		public async Task<IActionResult> Index(string Categoria, StatusVeiculo? statusVeiculo, string order, string Marca)
		{
			var filtrosCategorias = GetFilterCategorias(_context.Categorias.ToList());
			var filtrosEstado = GetFilterEstado();
			var OrderList = GetOrder();
			var filtrosMarcas = GetMarcasFilters(_context.Veiculos.ToList());
			IQueryable<Veiculo>? veiculos = null;
			var model = new List<Veiculo>();

			if (User.IsInRole(Roles.Funcionario.ToString()) || User.IsInRole(Roles.Gestor.ToString()))
			{
				var user = await _userManager.GetUserAsync(User);
				veiculos = _context.Veiculos.Include(v => v.Categoria).Include(v => v.Empresa)
				.Include(r => r.Reservas).Where(v => v.EmpresaId == user.EmpresaId);
			}
			else if (User.IsInRole(Roles.Admin.ToString()))
			{
				veiculos = _context.Veiculos.Include(r => r.Reservas).Include(v => v.Categoria)
				.Include(v => v.Empresa);
			}
			if (veiculos != null && !String.IsNullOrEmpty(Categoria))
			{
				veiculos = veiculos.Where(v => v.Categoria.Nome == Categoria);
				filtrosCategorias = SelectListItem(Categoria, filtrosCategorias);
			}
			if (veiculos != null && statusVeiculo != null)
			{
				if (statusVeiculo == StatusVeiculo.A_CIRCULAR)
				{
					veiculos = veiculos.Where(v => v.Reservas.Where(r => r.Levantamento != null && r.Entrega == null).Count() > 0);
				}
				else if (statusVeiculo == StatusVeiculo.DISPONIVEL)
				{
					veiculos = veiculos.Where(v => v.Reservas.Where(r => r.Levantamento != null).Count() == 0);
				}
				filtrosEstado = SelectListItem(statusVeiculo.ToString(), filtrosEstado);
			}
			if (veiculos != null && !String.IsNullOrEmpty(order))
			{
				veiculos = OrderBy(order, veiculos);
				OrderList = SelectListItem(order, OrderList);
			}
			if (veiculos != null && !string.IsNullOrWhiteSpace(Marca))
			{
				veiculos = veiculos.Where(v => v.Marca == Marca);
				filtrosMarcas = SelectListItem(Marca, filtrosMarcas);
			}


			model = veiculos?.ToList();
			ViewData["Categorias"] = filtrosCategorias;
			ViewData["Estado"] = filtrosEstado;
			ViewData["Order"] = OrderList;
			ViewData["Marcas"] = filtrosMarcas;
			return View(model);
		}



		// GET: Veiculos/Details/5
		[Authorize(Roles = "Admin,Gestor, Funcionario")]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _context.Veiculos == null)
			{
				return NotFound();
			}

			var veiculo = await _context.Veiculos
				.Include(v => v.Categoria)
				.Include(v => v.Empresa)
				.FirstOrDefaultAsync(m => m.Id == id);

			if (veiculo == null)
			{
				return NotFound();
			}
			
            try
            {
                string CoursePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Ficheiros/Veiculos/" + id.ToString());
                var files = from file in Directory.EnumerateFiles(CoursePath) select string.Format("/Ficheiros/Veiculos/{0}/{1}", id, Path.GetFileName(file));
                ViewData["NFich"] = files.Count();
                ViewData["Ficheiro"] = files.FirstOrDefault("");

            }
            catch (Exception ex)
            {}
            return View(veiculo);
		}

		// GET: Veiculos/Create
		[Authorize(Roles = "Gestor, Funcionario")]
		public IActionResult Create()
		{
			ViewData["ListaDeCategorias"] = new SelectList(_context.Categorias.ToList(), "Id", "Nome");
			ViewData["ListaDeEmpresas"] = new SelectList(_context.Empresas.ToList(), "Id", "Nome");

			return View();
		}

		// POST: Veiculos/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Gestor, Funcionario")]
		public async Task<IActionResult> Create([Bind("Id,Nome,Marca,Modelo,Descricao,Matricula,Localizacao,CustoDia,Disponivel,EmpresaId,CategoriaId")] Veiculo veiculo, [FromForm] IFormFile ficheiro)
		{
			ViewData["ListaDeCategorias"] = new SelectList(_context.Categorias.ToList(), "Id", "Nome");
			ModelState.Remove(nameof(veiculo.Categoria));

			ViewData["ListaDeEmpresas"] = new SelectList(_context.Empresas.ToList(), "Id", "Nome");
			ModelState.Remove(nameof(veiculo.Empresa));
            ModelState.Remove(nameof(veiculo.Disponivel));
            ModelState.Remove(nameof(veiculo.Reservas));
			veiculo.Disponivel = true;

			if (veiculo.EmpresaId == -1)
			{
				ApplicationUser applicationUser = await _userManager.GetUserAsync(User);
				if (applicationUser.EmpresaId != null)
				{
					veiculo.EmpresaId = (int)applicationUser.EmpresaId;
				}
				else
				{
					ModelState.AddModelError("", "Nao foi poss�vel criar o veiculo!");
					return View(veiculo);
				}
			}

			if (ModelState.IsValid)
			{
				_context.Add(veiculo);
				await _context.SaveChangesAsync();

                string CoursePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Ficheiros/Veiculos/" + veiculo.Id.ToString());

                if (!Directory.Exists(CoursePath))
                {
                    Directory.CreateDirectory(CoursePath);
                }
                
                if (ficheiro != null && ficheiro.Length > 0)
                {
                    var filePath = Path.Combine(CoursePath, Guid.NewGuid().ToString() + Path.GetExtension(ficheiro.FileName));
                    while (System.IO.File.Exists(filePath))
                    {
                        filePath = Path.Combine(CoursePath, Guid.NewGuid().ToString() + Path.GetExtension(ficheiro.FileName));
                    }
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await ficheiro.CopyToAsync(stream);
                    }
                }

                return RedirectToAction(nameof(Index));
			}

			return View(veiculo);
		}

		// GET: Veiculos/Edit/5
		[Authorize(Roles = "Admin,Gestor, Funcionario")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null || _context.Veiculos == null)
			{
				return NotFound();
			}

			var veiculo = await _context.Veiculos.FindAsync(id);
			if (veiculo == null)
			{
				return NotFound();
			}
            try
            {
                string CoursePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Ficheiros/Veiculos/" + id.ToString());
                var files = from file in Directory.EnumerateFiles(CoursePath) select string.Format("/Ficheiros/Veiculos/{0}/{1}", id, Path.GetFileName(file));
                ViewData["NFich"] = files.Count();
                ViewData["Ficheiro"] = files.FirstOrDefault("");

            }
            catch (Exception ex)
            { }
            ViewData["ListaDeCategorias"] = new SelectList(_context.Categorias.ToList(), "Id", "Nome");
			ViewData["ListaDeEmpresas"] = new SelectList(_context.Empresas.ToList(), "Id", "Nome");

			return View(veiculo);
		}

		// POST: Veiculos/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Gestor, Funcionario")]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Marca,Modelo,Descricao,Matricula,Localizacao,CustoDia,Disponivel,EmpresaId,CategoriaId")] Veiculo veiculo, [FromForm] IFormFile ficheiro)
		{
			if (id != veiculo.Id)
			{
				return NotFound();
			}

			ViewData["ListaDeCategorias"] = new SelectList(_context.Categorias.ToList(), "Id", "Nome");
			ModelState.Remove(nameof(veiculo.Categoria));

			ViewData["ListaDeEmpresas"] = new SelectList(_context.Empresas.ToList(), "Id", "Nome");
			ModelState.Remove(nameof(veiculo.Empresa));

			ModelState.Remove(nameof(veiculo.Reservas));
            ModelState.Remove(nameof(ficheiro));
            if (ModelState.IsValid)
			{
				try
				{
					_context.Update(veiculo);
					await _context.SaveChangesAsync();
                    try
                    {
						if(ficheiro != null) { 
							string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Ficheiros/Veiculos/" + id.ToString());
							System.IO.DirectoryInfo di = new DirectoryInfo(path);

							foreach (FileInfo file in di.GetFiles())
							{
								file.Delete();
							}
                        }
                    }
                    catch (Exception ex)
                    { }
                    string CoursePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Ficheiros/Veiculos/" + veiculo.Id.ToString());
                    if (!Directory.Exists(CoursePath))
                    {
                        Directory.CreateDirectory(CoursePath);
                    }



                    if (ficheiro != null && ficheiro.Length > 0)
                    {
                        var filePath = Path.Combine(CoursePath, Guid.NewGuid().ToString() + Path.GetExtension(ficheiro.FileName));
                        while (System.IO.File.Exists(filePath))
                        {
                            filePath = Path.Combine(CoursePath, Guid.NewGuid().ToString() + Path.GetExtension(ficheiro.FileName));
                        }
                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await ficheiro.CopyToAsync(stream);
                        }
                    }
                }
				catch (DbUpdateConcurrencyException)
				{
					if (!VeiculoExists(veiculo.Id))
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

			return View(veiculo);
		}

		// GET: Veiculos/Delete/5
		[Authorize(Roles = "Admin,Gestor, Funcionario")]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || _context.Veiculos == null)
			{
				return NotFound();
			}

			var veiculo = await _context.Veiculos
				.Include(v => v.Categoria)
				.Include(v => v.Empresa)
				.Include(v => v.Reservas)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (veiculo == null)
			{
				return NotFound();
			}

			if (veiculo.Reservas != null && veiculo.Reservas.Count() > 0)
			{
				return RedirectToAction(nameof(Index));
			}

			return View(veiculo);
		}

		// POST: Veiculos/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Gestor, Funcionario")]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (_context.Veiculos == null)
			{
				return Problem("Entity set 'ApplicationDbContext.Veiculos'  is null.");
			}
			var veiculo = await _context.Veiculos.FindAsync(id);
			if (veiculo != null)
			{
				_context.Veiculos.Remove(veiculo);
			}

			if (veiculo.Reservas != null && veiculo.Reservas.Count() > 0)
			{
				return RedirectToAction(nameof(Index));
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		private IEnumerable<SelectListItem> GetFilterCategorias(List<Categoria> categorias)
		{
			var filter = categorias.Select(categoria => categoria.Nome).Distinct().ToArray();
			var items = new SelectListItem[filter.Count() + 1];
			items[0] = new SelectListItem() { Text = " ", Value = string.Empty };
			for (int i = 1; i < filter.Count() + 1; i++)
			{
				items[i] = new SelectListItem() { Text = filter[i - 1], Value = filter[i - 1] };
			}
			return items;
		}
		private IEnumerable<SelectListItem> GetMarcasFilters(List<Veiculo> veiculos)
		{
			var marcas = veiculos.Select(e => e.Marca).Distinct();
			List<SelectListItem> listItems = new List<SelectListItem>
			{
				new SelectListItem("", string.Empty)
			};
			foreach (var marca in marcas)
			{
				if(!string.IsNullOrWhiteSpace(marca))
					listItems.Add(new SelectListItem(marca, marca));
			}
			return listItems;
		}
		private IEnumerable<SelectListItem> GetFilterEstado()
		{
			return new SelectListItem[]{
				new SelectListItem() { Text = " ", Value = string.Empty },
				new SelectListItem() { Text = "Disponível", Value = StatusVeiculo.DISPONIVEL.ToString() },
				new SelectListItem() { Text = "A Circular", Value = StatusVeiculo.A_CIRCULAR.ToString() }
			};
		}

		private IEnumerable<SelectListItem> GetOrder()
		{
			return new SelectListItem[]{
				new SelectListItem() { Text = " ", Value = string.Empty },
				new SelectListItem() { Text = "Nome Asc", Value = "nome asc"},
				new SelectListItem() { Text = "Nome Desc", Value = "nome desc" },
				new SelectListItem() { Text = "Marca Asc", Value = "marca asc" },
				new SelectListItem() { Text = "Marca Desc", Value = "marca desc" },
				new SelectListItem() { Text = "Custo Asc", Value = "custo dia asc" },
				new SelectListItem() { Text = "Custo Desc", Value = "custo dia desc" },
				new SelectListItem() { Text = "Disponivel Asc", Value = "disponibilidade asc" },
				new SelectListItem() { Text = "Disponivel Desc", Value = "disponibilidade desc" },
				new SelectListItem() { Text = "Categoria Asc", Value = "categoria asc" },
				new SelectListItem() { Text = "Categoria Desc", Value = "categoria desc" },
			};
		}

		private IQueryable<Veiculo> OrderBy(string order, IQueryable<Veiculo> veiculos)
		{
			switch (order)
			{
				case "nome asc":
					{
						veiculos = veiculos.OrderBy(v => v.Nome);
						break;
					}
				case "nome desc":
					{
						veiculos = veiculos.OrderByDescending(v => v.Nome);
						break;
					}
				case "marca asc":
					{
						veiculos = veiculos.OrderBy(v => v.Marca);
						break;
					}
				case "marca desc":
					{
						veiculos = veiculos.OrderByDescending(v => v.Marca);
						break;
					}
				case "custo dia asc":
					{
						veiculos = veiculos.OrderBy(v => v.CustoDia);
						break;
					}
				case "custo dia desc":
					{
						veiculos = veiculos.OrderByDescending(v => v.CustoDia);
						break;
					}
				case "disponibilidade asc":
					{
						veiculos = veiculos.OrderBy(v => v.Disponivel);
						break;
					}
				case "disponibilidade desc":
					{
						veiculos = veiculos.OrderByDescending(v => v.Disponivel);
						break;
					}
				case "categoria asc":
					{
						veiculos = veiculos.OrderByDescending(v => v.Categoria.Nome);
						break;
					}
				case "categoria desc":
					{
						veiculos = veiculos.OrderByDescending(v => v.Categoria.Nome);
						break;
					}
			}



			return veiculos;
		}
		private IEnumerable<SelectListItem> SelectListItem(string SelectItem, IEnumerable<SelectListItem> ListItem)
		{
			foreach (var item in ListItem)
			{
				if (item.Value == SelectItem)
				{
					item.Selected = true;
				}
				else
				{
					item.Selected = false;
				}
			}
			return ListItem;
		}

		private bool VeiculoExists(int id)
		{
			return (_context.Veiculos?.Any(e => e.Id == id)).GetValueOrDefault();
		}
	}
}
