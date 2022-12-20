using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Escola_Segura.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Index(string Categoria, StatusVeiculo? statusVeiculo, string order)
        {
            var filtrosCategorias = GetFilterCategorias(_context.Categorias.ToList());
            var filtrosEstado = GetFilterEstado();
            var OrderList = GetOrder();
            IQueryable<Veiculo>? veiculos = null;
            var model = new List<Veiculo>();
            if(User.IsInRole(Roles.Funcionario.ToString()) || User.IsInRole(Roles.Gestor.ToString())){
                var user = await _userManager.GetUserAsync(User);
                veiculos = _context.Veiculos.Include(v => v.Categoria).Include(v => v.Empresa)
                .Include(r => r.Reservas).Where(v => v.EmpresaId == user.EmpresaId);
            }else if(User.IsInRole(Roles.Admin.ToString())){
                veiculos = _context.Veiculos.Include(r => r.Reservas).Include(v => v.Categoria)
                .Include(v => v.Empresa);
            }
            if(veiculos != null && !String.IsNullOrEmpty(Categoria)){
                veiculos = veiculos.Where(v => v.Categoria.Nome == Categoria);
                filtrosCategorias = SelectListItem(Categoria, filtrosCategorias);
            }
            if(veiculos != null && statusVeiculo != null){
                if(statusVeiculo == StatusVeiculo.A_CIRCULAR){
                    veiculos = veiculos.Where(v => v.Reservas.Where(r => r.Levantamento != null && r.Entrega == null).Count() > 0);
                }else if(statusVeiculo == StatusVeiculo.DISPONIVEL){
                    veiculos = veiculos.Where(v => v.Reservas.Where(r => r.Levantamento != null).Count() == 0);
                }
                filtrosEstado = SelectListItem(statusVeiculo.ToString(), filtrosEstado);
            }
            if(veiculos != null && !String.IsNullOrEmpty(order))
            {
                veiculos = OrderBy(order, veiculos);
                OrderList = SelectListItem(order, OrderList);
            }


            model = veiculos?.ToList();
            ViewData["Categorias"] = filtrosCategorias;
            ViewData["Estado"] = filtrosEstado;
            ViewData["Order"] = OrderList;
            return View(model);
        }



        // GET: Veiculos/Details/5
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

            return View(veiculo);
        }

        // GET: Veiculos/Create
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
        public async Task<IActionResult> Create([Bind("Id,Nome,Descricao,Localizacao,CustoDia,Disponivel,EmpresaId,CategoriaId")] Veiculo veiculo)
        {
            ViewData["ListaDeCategorias"] = new SelectList(_context.Categorias.ToList(), "Id", "Nome");
            ModelState.Remove(nameof(veiculo.Categoria));

            ViewData["ListaDeEmpresas"] = new SelectList(_context.Empresas.ToList(), "Id", "Nome");
            ModelState.Remove(nameof(veiculo.Empresa));
            
            ModelState.Remove(nameof(veiculo.Reservas));

            if (veiculo.EmpresaId == -1)
            {
                ApplicationUser applicationUser = await _userManager.GetUserAsync(User);
                if (applicationUser.EmpresaId != null)
                {
                    veiculo.EmpresaId = (int)applicationUser.EmpresaId;
                }
                else
                {
                    ModelState.AddModelError("", "N�o foi poss�vel criar o ve�culo!");
                    return View(veiculo);
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(veiculo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(veiculo);
        }

        // GET: Veiculos/Edit/5
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

            ViewData["ListaDeCategorias"] = new SelectList(_context.Categorias.ToList(), "Id", "Nome");
            ViewData["ListaDeEmpresas"] = new SelectList(_context.Empresas.ToList(), "Id", "Nome");

            return View(veiculo);
        }

        // POST: Veiculos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Descricao,Localizacao,CustoDia,Disponivel,EmpresaId,CategoriaId")] Veiculo veiculo)
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(veiculo);
                    await _context.SaveChangesAsync();
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

            if(veiculo.Reservas != null && veiculo.Reservas.Count() >= 0){
                return RedirectToAction(nameof(Index));
            }

            return View(veiculo);
        }

        // POST: Veiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
            
            if(veiculo.Reservas != null && veiculo.Reservas.Count() >= 0){
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
            for (int i = 1; i < filter.Count()+1; i++)
            {
                items[i] = new SelectListItem() { Text = filter[i-1], Value = filter[i-1] };
            }
            return items;
        }
         private IEnumerable<SelectListItem> GetFilterEstado()
        {
            return  new SelectListItem[]{
                new SelectListItem() { Text = " ", Value = string.Empty },
                new SelectListItem() { Text = "Disponivel", Value = StatusVeiculo.DISPONIVEL.ToString() },
                new SelectListItem() { Text = "A Circular", Value = StatusVeiculo.A_CIRCULAR.ToString() }
            };
        }

        private IEnumerable<SelectListItem> GetOrder()
        {
            return new SelectListItem[]{
                new SelectListItem() { Text = " ", Value = string.Empty },
                new SelectListItem() { Text = "Nome Asc", Value = "nome asc"},
                new SelectListItem() { Text = "Nome Desc", Value = "nome desc" },
                new SelectListItem() { Text = "Custo Desc", Value = "custo dia asc" },
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
