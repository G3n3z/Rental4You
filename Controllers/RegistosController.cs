using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Escola_Segura.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rental4You.Data;

namespace Rental4You.Models
{
    public class RegistosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Registos
        [Authorize(Roles = "Admin,Gestor,Funcionario,Cliente")]
        public async Task<IActionResult> Index(RegistoType? tipo)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            List<Registo> registos;
            IQueryable<Registo> reg;
            if (User.IsInRole(Roles.Admin.ToString()))
            {
                reg = _context.Registos;
            }else if (User.IsInRole(Roles.Gestor.ToString()) || User.IsInRole(Roles.Funcionario.ToString()))
            {
                reg = _context.Registos.Where(r => r.Funcionario.Id == user.Id);   
            }else{
                reg = _context.Registos.Include(r => r.Reserva)
                .Where(r => r.Reserva.ApplicationUserId == user.Id);
            }

            if(tipo != null)
            {
                reg.Where(r => r.Tipo == tipo);
            }
            registos = reg.ToList();
            return View(registos);                         
        }

        // GET: Registos/Details/5
        [Authorize(Roles = "Admin,Gestor,Funcionario,Cliente")]
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
            try
            {
                string CoursePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Ficheiros/" + id.ToString());
                var files = from file in Directory.EnumerateFiles(CoursePath) select string.Format("/Ficheiros/{0}/{1}", id, Path.GetFileName(file));
                ViewData["NFich"] = files.Count();
                ViewData["Ficheiros"] = files;

            }
            catch (Exception ex)
            {

            }

            return View(registo);
        }

        // GET: Registos/Create
        [Authorize(Roles = "Gestor,Funcionario")]
        public async Task<IActionResult> CreateAsync(RegistoType tipo, int idReserva)
        {
            var r = new Registo();
            r.Tipo = tipo;
            Reserva? reserva = _context.Reservas.Include(r => r.Levantamento).Include(r => r.Entrega).FirstOrDefault( r => r.ReservaId == idReserva);
            if(reserva == null)
            {
                return NotFound();
            }
            if (tipo == RegistoType.LEVANTAMENTO && reserva.Levantamento != null)
            {
                return NotFound();
            }
            else if (tipo == RegistoType.LEVANTAMENTO && reserva.Entrega != null)
            {
                return NotFound();
            }
            else if (tipo == RegistoType.ENTREGA && reserva.Levantamento == null)
            {
                return NotFound();
            }
            else if (tipo == RegistoType.ENTREGA && reserva.Entrega != null)
            {
                return NotFound();
            }
            r.Data = DateTime.Now;
            r.ReservaId = idReserva;
            return View(r);
        }

        // POST: Registos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Gestor,Funcionario")]
        public async Task<IActionResult> Create([Bind("Id,Data,Kms,Danos,Tipo,ReservaId,Observacoes")] Registo registo, [FromForm] List<IFormFile> ficheiros)
        {

            ModelState.Remove(nameof(registo.Reserva));
            ModelState.Remove(nameof(registo.Funcionario));
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                return Unauthorized();
            }

            Reserva? reserva = await _context.Reservas.FindAsync(registo.ReservaId);
            if(reserva == null)
            {
                return View(registo);
            }

            if(registo.Tipo == RegistoType.LEVANTAMENTO && reserva.Estado == StatusReserva.approved)
            {
                reserva.Estado = StatusReserva.provided;
                reserva.Levantamento = registo;
            }else if (registo.Tipo == RegistoType.ENTREGA && reserva.Estado == StatusReserva.provided)
            {
                reserva.Estado = StatusReserva.delivered;
                reserva.Entrega = registo;
                reserva.Concluido = true;
            }
            else
            {
                return View(registo);
            }
            
            registo.Funcionario = user;



            if (ModelState.IsValid)
            {
                try { 
                    _context.Add(registo);
                    _context.Update(reserva);
                    await _context.SaveChangesAsync();

                    string CoursePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Ficheiros/" + registo.Id.ToString());

                    if (!Directory.Exists(CoursePath))
                    {
                        Directory.CreateDirectory(CoursePath);
                    }

                    foreach (var formFile in ficheiros)
                    {
                        if (formFile.Length > 0)
                        {
                            var filePath = Path.Combine(CoursePath, Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName));
                            while (System.IO.File.Exists(filePath))
                            {
                                filePath = Path.Combine(CoursePath, Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName));
                            }
                            using (var stream = System.IO.File.Create(filePath))
                            {
                                await formFile.CopyToAsync(stream);
                            }
                        }
                    }

                }
                catch(Exception e)
                {

                }

                return RedirectToAction(nameof(Index));
            }
            return View(registo);
        }

        // GET: Registos/Edit/5
        [Authorize(Roles = "Gestor,Funcionario")]
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
        [Authorize(Roles = "Gestor,Funcionario")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Data,Kms,Danos,Observacoes")] Registo registo, [FromForm] List<IFormFile> ficheiros)
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

        private bool RegistoExists(int id)
        {
          return (_context.Registos?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
