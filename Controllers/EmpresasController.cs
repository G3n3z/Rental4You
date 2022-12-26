using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Escola_Segura.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Rental4You.Data;

namespace Rental4You.Models
{
    [Authorize(Roles = "Admin")]
    public class EmpresasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmpresasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private IEnumerable<SelectListItem> GetSubscriptionFilters()
        {
            return new SelectListItem[]
            {
                new SelectListItem() { Text = " ", Value = string.Empty },
                new SelectListItem() { Text = "Ativa", Value = "true" },
                new SelectListItem() { Text = "Inativa", Value = "false" }
            };
        }

        private IEnumerable<SelectListItem> GetLocationFilters(List<Empresa> empresas)
        {
            var localidades = empresas.Select(e => e.Localidade).Distinct();
            List<SelectListItem> listItems = new List<SelectListItem>
            {
                new SelectListItem("", string.Empty)
            };
            foreach (var localidade in localidades)
            {
                listItems.Add(new SelectListItem(localidade, localidade));
            }
            return listItems;
        }

        private IEnumerable<SelectListItem> SelectedListItem(string SelectItem, IEnumerable<SelectListItem> ListItem)
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

        private IEnumerable<SelectListItem> GetOrderFilter()
        {
            return new SelectListItem[]{
                new SelectListItem() { Text = " ", Value = string.Empty },
                new SelectListItem() { Text = "Nome Asc", Value = "nome_asc"},
                new SelectListItem() { Text = "Nome Desc", Value = "nome_desc" },
                new SelectListItem() { Text = "Localidade Asc", Value = "loc_asc" },
                new SelectListItem() { Text = "Localidade Desc", Value = "loc_desc" },
                new SelectListItem() { Text = "Subscrição Asc", Value = "sub_asc" },
                new SelectListItem() { Text = "Subscrição Desc", Value = "sub_desc" },                
            };
        }

        private IQueryable<Empresa> Order(string sortOrder, IQueryable<Empresa> empresas)
        {
            switch (sortOrder)
            {
                case "nome_asc":
                    empresas = empresas.OrderBy(s => s.Nome);
                    break;
                case "nome_desc":
                    empresas = empresas.OrderByDescending(s => s.Nome);
                    break;
                case "loc_asc":
                    empresas = empresas.OrderBy(s => s.Localidade);
                    break;
                case "loc_desc":
                    empresas = empresas.OrderByDescending(s => s.Localidade);
                    break;
                case "sub_asc":
                    empresas = empresas.OrderBy(s => s.Activo);
                    break;
                case "sub_desc":
                    empresas = empresas.OrderByDescending(s => s.Activo);
                    break;
            }
            return empresas.AsQueryable();
        }

        // GET: Empresas
        public async Task<IActionResult> Index(string sortOrder, string TextoAPesquisar, string Localidade, string Subscricao)
        {
            var filtrosLocalidade = GetLocationFilters(_context.Empresas.ToList());
            var filtrosSubscricao = GetSubscriptionFilters();
            var filtrosOrdenacao = GetOrderFilter();

            IQueryable<Empresa> resultado;

            //Filtrar texto de pesquisa
            if (string.IsNullOrWhiteSpace(TextoAPesquisar))
            {
                ViewData["TextoPesquisa"] = "";
                resultado = _context.Empresas;
            }
            else
            {
                ViewData["TextoPesquisa"] = TextoAPesquisar;
                resultado = _context.Empresas.Where(e => e.Nome.Contains(TextoAPesquisar));
            }
            //Filtrar Localidade
            if (!string.IsNullOrWhiteSpace(Localidade))
            {
                resultado = resultado.Where(e => e.Localidade == Localidade);
                filtrosLocalidade = SelectedListItem(Localidade, filtrosLocalidade);
            }
            //Filtrar Subscricao
            if (!string.IsNullOrWhiteSpace(Subscricao))
            {
                resultado = resultado.Where(e => e.Activo == (Subscricao == "true"));
                filtrosSubscricao = SelectedListItem(Subscricao, filtrosSubscricao);
            }
            //Aplicar Ordenacao selecionada
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                resultado = Order(sortOrder, resultado);
                filtrosOrdenacao = SelectedListItem(sortOrder, filtrosOrdenacao);
            }

            ViewData["TextoPesquisa"] = TextoAPesquisar;
            ViewData["FiltroLocalidade"] = filtrosLocalidade;
            ViewData["FiltroSubscricao"] = filtrosSubscricao;
            ViewData["FiltroOrdenacao"] = filtrosOrdenacao;

            return View(await resultado.Include(e => e.Veiculos).AsNoTracking().ToListAsync());
        }
        
        // GET: Empresas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Empresas == null)
            {
                return NotFound();
            }

            var empresa = await _context.Empresas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empresa == null)
            {
                return NotFound();
            }

            return View(empresa);
        }

        // GET: Empresas/Create
        public IActionResult Create()
        {
            return View();
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }

        // POST: Empresas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Localidade,Activo")] Empresa empresa)
        {
            ModelState.Remove(nameof(empresa.Avaliacoes));
            ModelState.Remove(nameof(empresa.Utilizadores));
            ModelState.Remove(nameof(empresa.Veiculos));
            if (ModelState.IsValid)
            {
                ApplicationUser gestor = new ApplicationUser();
                gestor.Email = "gestor@" + empresa.Nome.ToLower() + ".pt";
                if (!IsValidEmail(gestor.Email))
                {
                    //TODO
                }

                gestor.EmailConfirmed = true;
                gestor.DataNascimento = new DateTime();
                gestor.PrimeiroNome = "gestor";
                gestor.UltimoNome = "gestor";
                gestor.PhoneNumber = null;
                gestor.NIF = "0000000";
                gestor.UserName = gestor.Email;
                empresa.MediaAvaliacao = -1;
                _context.Add(empresa);  
                await _context.SaveChangesAsync();
                gestor.EmpresaId = empresa.Id;
                var result = await _userManager.CreateAsync(gestor, "Gestor123!");
                
                await _userManager.AddToRoleAsync(gestor, Roles.Gestor.ToString());
                return RedirectToAction(nameof(Index));
            }
            return View(empresa);
        }

        // GET: Empresas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Empresas == null)
            {
                return NotFound();
            }

            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
            {
                return NotFound();
            }
            return View(empresa);
        }

        // POST: Empresas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Localidade,Activo")] Empresa empresa)
        {
            if (id != empresa.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(empresa.Avaliacoes));
            ModelState.Remove(nameof(empresa.Utilizadores));
            ModelState.Remove(nameof(empresa.Veiculos));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(empresa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpresaExists(empresa.Id))
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
            return View(empresa);
        }

        // GET: Empresas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Empresas == null)
            {
                return NotFound();
            }

            var empresa = await _context.Empresas
                .Include(e => e.Veiculos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empresa == null)
            {
                return NotFound();
            }
			if (empresa.Veiculos != null)
            {
                return BadRequest();
            }

			return View(empresa);
        }

        // POST: Empresas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Empresas == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Empresas'  is null.");
            }
            var empresa = _context.Empresas
                .Include(e => e.Veiculos)
                .FirstOrDefault(e => e.Id == id);

            if (empresa != null)
            {
                if(empresa.Veiculos == null || empresa.Veiculos.Count == 0) { 
                    var users = _context.Users.Where(u => u.EmpresaId == id);
                    _context.Users.RemoveRange(users);

                    _context.Empresas.Remove(empresa);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    ModelState.AddModelError("", "Não é possivel remover empresas com veiculos");
                    return View();
                }
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool EmpresaExists(int id)
        {
          return (_context.Empresas?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
