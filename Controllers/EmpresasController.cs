using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Escola_Segura.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Rental4You.Data;

namespace Rental4You.Models
{
    public class EmpresasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmpresasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private IEnumerable<SelectListItem> GetSubscricao()
        {
            return new SelectListItem[]
            {
                new SelectListItem() { Text = "Ativa", Value = "true" },
                new SelectListItem() { Text = "Inativa", Value = "false" }
            };
        }

        // GET: Empresas
        public async Task<IActionResult> Index(string sortOrder)
        {
            var localidades = await _context.Empresas.Select(x => x.Localidade).ToListAsync();

            ViewData["ListaDeLocalidades"] = new SelectList(_context.Empresas.Select(x => x.Localidade).ToList().Distinct());
            ViewData["Subscricao"] = GetSubscricao();

            ViewData["NomeSortParam"] = string.IsNullOrEmpty(sortOrder) ? "nome_desc" : "";
            ViewData["LocalSortParam"] = sortOrder == "Local" ? "local_desc" : "Local";
            ViewData["SubscricaoSortParam"] = sortOrder == "Subscricao" ? "subscricao_desc" : "Subscricao";
                        
            var resultado = from c in _context.Empresas
                            select c;

            switch (sortOrder)
            {
                case "nome_desc":
                    resultado = resultado.OrderByDescending(s => s.Nome);
                    break;
                case "Local":
                    resultado = resultado.OrderBy(s => s.Localidade);
                    break;
                case "local_desc":
                    resultado = resultado.OrderByDescending(s => s.Localidade);
                    break;
                case "Subscricao":
                    resultado = resultado.OrderBy(s => s.Activo);
                    break;
                case "subscricao_desc":
                    resultado = resultado.OrderByDescending(s => s.Activo);
                    break;
                default:
                    resultado = resultado.OrderBy(s => s.Nome);
                    break;
            }

            return View(await resultado.AsNoTracking().ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Index(string sortOrder, string TextoAPesquisar, string Localidade, string Subscricao)
        {
            SelectList listLocalidades = new SelectList(_context.Empresas.Select(x => x.Localidade).ToList().Distinct());
            foreach (var item in listLocalidades)
            {
                if (item.Text == Localidade)
                    item.Selected = true;
            }
            var listSubscricoes = GetSubscricao();
            foreach (var item in listSubscricoes)
            {
                if (item.Value == Subscricao)
                    item.Selected = true;
            }

            ViewData["ListaDeLocalidades"] = listLocalidades;
            ViewData["Subscricao"] = listSubscricoes;
            
            var estadoSubscricao = Subscricao.Equals("true") ? true : false;

            IQueryable<Empresa> resultado;

            if (string.IsNullOrWhiteSpace(TextoAPesquisar))
            {
                ViewData["TextoPesquisa"] = "";
                resultado = from c in _context.Empresas
                                where c.Localidade == Localidade && c.Activo == estadoSubscricao
                                select c;
            }
            else
            {
                ViewData["TextoPesquisa"] = TextoAPesquisar;
                resultado = from c in _context.Empresas
                                where c.Nome.Contains(TextoAPesquisar) && c.Localidade == Localidade && c.Activo == estadoSubscricao
                                select c;
            }

            return View(resultado);
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empresa == null)
            {
                return NotFound();
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
                .Include(e => e.Utilizadores)
                .Include(e => e.Avaliacoes)
                .FirstOrDefault(e => e.Id == id);

            if (empresa != null)
            {
                if(empresa.Veiculos == null || empresa.Veiculos.Count == 0) { 
                   
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
