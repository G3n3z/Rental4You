using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rental4You.Data;

namespace Rental4You.Models
{
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservas
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                return View();
            }
            if (User.IsInRole("Cliente"))
            {
                 var reservas = _context.Reservas.Include(r => r.ApplicationUser).Where(r => r.ApplicationUserId == user.Id)
                    .OrderByDescending(r => r.DataLevantamento)
                    .ThenByDescending(r => r.CustoTotal);
                return View(reservas);
            }
            else if (User.IsInRole("Funcionario") || User.IsInRole("Gestor"))
            {
               var reservas = _context.Reservas.Include(r => r.ApplicationUser).Include(r => r.Veiculo)
                    .Include(r => r.Avaliacao)
                    .Where(r => r.Veiculo.EmpresaId == user.EmpresaId);
                return View(reservas);
            }
            
            return View();
        }

        // GET: Reservas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Reservas == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.ApplicationUser)
                .Include(r => r.Veiculo)
                .Include(r => r.Avaliacao)
                .FirstOrDefaultAsync(m => m.ReservaId == id);
            if (reserva == null)
            {
                return NotFound();
            }
            
            return View(reserva);
        }

        // GET: Reservas/Create
        public async Task<IActionResult> Create(int? idVeiculo, DateTime datalevantamento, DateTime dataentrega)
        {
            if (idVeiculo == null)
            {
                return NotFound();
            }

            if(datalevantamento > dataentrega)
            {
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["VeiculoId"] = new SelectList(_context.Veiculos, "Id", "Id");
            var veiculo = await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == idVeiculo);
            if (veiculo == null)
            {
                return NotFound();
            }

            decimal dias = (dataentrega.Hour - datalevantamento.Hour) / 24;
            ViewData["Valor"] = ((int)Math.Ceiling(dias)) * veiculo.CustoDia;
            ViewBag.veiculo = veiculo;
            Reserva reserva = new Reserva();
            reserva.CustoTotal = ((int)Math.Ceiling(dias)) * veiculo.CustoDia;
            reserva.DataLevantamento = datalevantamento;
            reserva.DataEntrega = dataentrega;
            return View();
        }

        // POST: Reservas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DataLevantamento,DataEntrega,CustoTotal,VeiculoId")] Reserva reserva)
        {

            var veiculo = await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == reserva.VeiculoId);
            if (veiculo == null)
            {
                return NotFound();
            }
            ModelState.Remove(nameof(reserva.Veiculo));
            ModelState.Remove(nameof(reserva.Avaliacao));
            ModelState.Remove(nameof(reserva.Entrega));
            ModelState.Remove(nameof(reserva.Levantamento));
            ModelState.Remove(nameof(reserva.ApplicationUser));
            ModelState.Remove(nameof(reserva.ApplicationUserId));

            reserva.Concluido = false;
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                ModelState.AddModelError("", "Nao tem user logado");
                return View(reserva);
            }
            reserva.ApplicationUser = user;
            reserva.ApplicationUserId = user.Id;
            if (ModelState.IsValid)
            {
                _context.Add(reserva);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", reserva.ApplicationUserId);
            ViewData["VeiculoId"] = new SelectList(_context.Veiculos, "Id", "Id", reserva.VeiculoId);
            ViewBag.veiculo = veiculo;
            return View(reserva);
        }

        // GET: Reservas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Reservas == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", reserva.ApplicationUserId);
            ViewData["VeiculoId"] = new SelectList(_context.Veiculos, "Id", "Id", reserva.VeiculoId);
            return View(reserva);
        }

        // POST: Reservas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservaId,Concluido,VeiculoId,ApplicationUserId")] Reserva reserva)
        {
            if (id != reserva.ReservaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reserva);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservaExists(reserva.ReservaId))
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
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", reserva.ApplicationUserId);
            ViewData["VeiculoId"] = new SelectList(_context.Veiculos, "Id", "Id", reserva.VeiculoId);
            return View(reserva);
        }

        // GET: Reservas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Reservas == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.ApplicationUser)
                .Include(r => r.Veiculo)
                .FirstOrDefaultAsync(m => m.ReservaId == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Reservas == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Reservas'  is null.");
            }
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> NovaReserva(int? idVeiculo, DateTime DataLevantamento, DateTime DataEntrega)
        {
            if (idVeiculo == null)
            {
                return NotFound();
            }

            if (DataLevantamento > DataEntrega)
            {
                return RedirectToAction(nameof(Index));
            }

            var veiculo = await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == idVeiculo);
            if (veiculo == null)
            {
                return NotFound();
            }
            
            double dias = ((DataEntrega - DataLevantamento).TotalHours/24);
            
            ViewData["Valor"] = ((int)Math.Ceiling(dias)) * veiculo.CustoDia;
            ViewBag.veiculo = veiculo;
            Reserva reserva = new Reserva();
            reserva.CustoTotal = ((int)Math.Ceiling(dias)) * veiculo.CustoDia;
            reserva.DataLevantamento = (DateTime)DataLevantamento;
            reserva.DataEntrega = (DateTime)DataEntrega;
            reserva.Veiculo = veiculo;
            reserva.VeiculoId = veiculo.Id;
            return View(reserva);
        }

        
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(StatusReserva? newStatus, int? id){
            if(id == null || newStatus == null){
                return RedirectToAction(nameof(Index));
            }
            var reserva = await _context.Reservas.FindAsync(id);
            if(reserva == null){
                return NotFound();
            }
            if(reserva.Estado != StatusReserva.pending){
                return RedirectToAction(nameof(Index));
            }
            reserva.Estado = (StatusReserva)newStatus;
            try{
                _context.Update(reserva);
                await _context.SaveChangesAsync();
            }catch(DbUpdateException ex){}
            return RedirectToAction(nameof(Index));

        }

        private bool ReservaExists(int id)
        {
          return (_context.Reservas?.Any(e => e.ReservaId == id)).GetValueOrDefault();
        }
    }
}
