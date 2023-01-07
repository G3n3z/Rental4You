using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rental4You.Data;
using Rental4You.ViewModel;

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
		[Authorize]
		public async Task<IActionResult> Index([Bind("DataLevantamento,DataEntrega,NomeCliente,NomeEmpresa,Veiculo,Estado, Categoria, order")] ReservasSearchViewModel viewModel)
		{
			var EstadoList = GetEstado();
			var CategoriaList = GetCategoria(_context.Categorias.ToList());
			var Order = GetOrder();
			var user = await _userManager.GetUserAsync(User);
			IQueryable<Reserva> reservas = null;
			if (user == null)
			{
				return View();
			}
			if (User.IsInRole("Cliente"))
			{
				reservas = _context.Reservas
					.Include(r => r.ApplicationUser)
					.Include(r => r.Avaliacao)
					.Include(r => r.Veiculo)
					.Include(r => r.Veiculo.Empresa)
					.Where(r => r.ApplicationUserId == user.Id)
					.OrderByDescending(r => r.DataLevantamento)
					.ThenByDescending(r => r.CustoTotal);

			}
			else if (User.IsInRole("Funcionario") || User.IsInRole("Gestor"))
			{
				reservas = _context.Reservas.Include(r => r.ApplicationUser).Include(r => r.Veiculo)
					.Include(r => r.Avaliacao)
					.Where(r => r.Veiculo.EmpresaId == user.EmpresaId);

			}
			else if (User.IsInRole("Admin"))
			{
				reservas = _context.Reservas.Include(r => r.ApplicationUser).Include(r => r.Veiculo)
				   .Include(r => r.Avaliacao);
			}
			else
			{
				return Unauthorized();
			}
			if (reservas != null && viewModel.DataLevantamento != null)
			{
				reservas = reservas.Where(r => r.DataLevantamento >= viewModel.DataLevantamento.Value);
			}

			if (reservas != null && viewModel.DataEntrega != null)
			{
				reservas = reservas.Where(r => r.DataEntrega <= viewModel.DataEntrega.Value);
			}

			if (reservas != null && !String.IsNullOrEmpty(viewModel.NomeCliente))
			{
				reservas = reservas.Where(r => (r.ApplicationUser.PrimeiroNome + " " +
											r.ApplicationUser.UltimoNome).Contains(viewModel.NomeCliente));
			}

			if (reservas != null && !String.IsNullOrEmpty(viewModel.NomeEmpresa))
			{
				reservas = reservas.Where(r => r.Veiculo.Empresa.Nome.Contains(viewModel.NomeEmpresa));
			}

			if (reservas != null && !String.IsNullOrEmpty(viewModel.Veiculo))
			{
				reservas = reservas.Where(r => (r.Veiculo.Nome.Contains(viewModel.Veiculo) || r.Veiculo.Marca.Contains(viewModel.Veiculo) || r.Veiculo.Modelo.Contains(viewModel.Veiculo)));
			}

			if (reservas != null && !String.IsNullOrEmpty(viewModel.Estado))
			{
				reservas = SearchForEstado(viewModel.Estado, reservas);
				EstadoList = SelectListItem(viewModel.Estado, EstadoList);
			}

			if (reservas != null && !String.IsNullOrEmpty(viewModel.Categoria))
			{
				reservas = reservas.Where(r => r.Veiculo.Categoria.Nome.Contains(viewModel.Categoria));
				CategoriaList = SelectListItem(viewModel.Categoria, CategoriaList);
			}
			if (reservas != null && !String.IsNullOrEmpty(viewModel.order))
			{
				reservas = OrderBy(viewModel.order, reservas);
				Order = SelectListItem(viewModel.order, Order);
			}

			ViewData["Categorias"] = CategoriaList;
			ViewData["Estado"] = EstadoList;
			ViewData["Order"] = Order;
			ViewData["DataLevantamento"] = viewModel.DataLevantamento;
			ViewData["DataEntrega"] = viewModel.DataEntrega;
			ViewData["NomeCliente"] = viewModel.NomeCliente;
			ViewData["NomeEmpresa"] = viewModel.NomeEmpresa;
			ViewData["Veiculo"] = viewModel.Veiculo;
			viewModel.reservas = reservas.ToList();
			var model = reservas.ToList();
			return View(viewModel);
		}

		// GET: Reservas/Details/5
		[Authorize(Roles = "Gestor,Funcionario,Cliente")]
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
		[Authorize(Roles = "Cliente")]
		public async Task<IActionResult> Create(int? idVeiculo, DateTime datalevantamento, DateTime dataentrega)
		{
			if (idVeiculo == null)
			{
				return NotFound();
			}

			if (datalevantamento > dataentrega)
			{
				return RedirectToAction(nameof(Index));
			}

			ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id");
			ViewData["VeiculoId"] = new SelectList(_context.Veiculos, "Id", "Id");
			var veiculos = _context.Veiculos.Where(v => v.Id == idVeiculo);



			veiculos.Where(veiculo => veiculo.Reservas == null ||
												  veiculo.Reservas.Count() == 0 ||
												  veiculo.Reservas.Where(reserva =>
														//     13-12-2022 < 19-12-2022      &&   20-12-2022 < 24-12-2022  1-1
														(datalevantamento <= reserva.DataLevantamento && reserva.DataEntrega <= dataentrega) ||
														//     18-12-2022 < 19-12-2022      &&   18-12-2022 < 19-12-2022 1-1
														(datalevantamento <= reserva.DataLevantamento && reserva.DataLevantamento <= dataentrega) ||
													   //     19-12-2022 < 20-12-2022      &&   20-12-2022 < 24-12-2022 1-1
													   (datalevantamento <= reserva.DataEntrega && reserva.DataEntrega <= dataentrega) ||
													   //     19-12-2022 11:00 > 19-12-2022 10:00     &&   20-12-2022 11:00 > 20-12-2022 10:00 1-1
													   (datalevantamento >= reserva.DataLevantamento && reserva.DataEntrega >= dataentrega)
													).Count() == 0);
			if (veiculos.Count() == 0)
			{
				return NotFound();
			}

			var veiculo = veiculos.First();

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
		[Authorize(Roles = "Cliente")]
		public async Task<IActionResult> Create([Bind("DataLevantamento,DataEntrega,CustoTotal,VeiculoId")] Reserva reserva)
		{

			var veiculo = await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == reserva.VeiculoId);
			if (veiculo == null)
			{
				return NotFound();
			}
			if (reserva.DataEntrega < reserva.DataLevantamento)
			{
				return BadRequest();
			}
			ModelState.Remove(nameof(reserva.Veiculo));
			ModelState.Remove(nameof(reserva.Avaliacao));
			ModelState.Remove(nameof(reserva.Entrega));
			ModelState.Remove(nameof(reserva.Levantamento));
			ModelState.Remove(nameof(reserva.ApplicationUser));
			ModelState.Remove(nameof(reserva.ApplicationUserId));

			reserva.Concluido = false;
			reserva.Estado = StatusReserva.pending;
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				ModelState.AddModelError("", "Nao tem user logado");
				return View(reserva);
			}
			reserva.ApplicationUser = user;
			reserva.ApplicationUserId = user.Id;
            reserva.DataReserva = DateTime.Today;

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
		[Authorize(Roles = "Gestor,Funcionario")]
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
		[Authorize(Roles = "Gestor,Funcionario")]
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
		[Authorize(Roles = "Gestor,Funcionario")]
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
		[Authorize(Roles = "Gestor,Funcionario")]
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
		[Authorize(Roles = "Cliente")]
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



			double dias = ((DataEntrega - DataLevantamento).TotalHours / 24);

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
		[Authorize(Roles = "Gestor,Funcionario")]
		public async Task<IActionResult> ChangeStatus(StatusReserva? newStatus, int? id)
		{
			if (id == null || newStatus == null)
			{
				return RedirectToAction(nameof(Index));
			}
			var reserva = await _context.Reservas.FindAsync(id);
			if (reserva == null)
			{
				return NotFound();
			}
			if (reserva.Estado != StatusReserva.pending)
			{
				return RedirectToAction(nameof(Index));
			}
			if (newStatus != StatusReserva.approved && newStatus != StatusReserva.rejected)
			{
				return RedirectToAction(nameof(Index));
			}
			reserva.Estado = (StatusReserva)newStatus;
			try
			{
				_context.Update(reserva);
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException ex) { }
			return RedirectToAction(nameof(Index));

		}

		[HttpPost]
		[Authorize(Roles = "Admin,Gestor")]
		public async Task<IActionResult> GetDadosReservasDiarias()
		{
			List<object> dados = new List<object>();
			DataTable dt = new DataTable();
			dt.Columns.Add("Reservas", Type.GetType("System.String"));
			dt.Columns.Add("Quantidade", Type.GetType("System.Int32"));

			var dadosReservas = await _context.Reservas
				.Where(r => r.DataReserva >= DateTime.Now.AddDays(-30) && r.DataReserva <= DateTime.Now)
				.GroupBy(r => r.DataReserva)
				.Select(r => new
				{
					Data = new DateOnly(r.Key.Year, r.Key.Month, r.Key.Day),
					Qtd = r.Count()
				})
				.ToListAsync();

			DataRow dr;
			foreach (var reserva in dadosReservas)
			{
				dr = dt.NewRow();
				dr["Reservas"] = reserva.Data;
				dr["Quantidade"] = reserva.Qtd;
				dt.Rows.Add(dr);
			}

			foreach (DataColumn dc in dt.Columns)
			{
				List<object> x = new List<object>();
				x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
				dados.Add(x);
			}
			return Json(dados);
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetDadosReservasMensais()
		{
			List<object> dados = new List<object>();
			DataTable dt = new DataTable();
			dt.Columns.Add("Reservas", Type.GetType("System.String"));
			dt.Columns.Add("Quantidade", Type.GetType("System.Int32"));

			var dadosReservas = await _context.Reservas
				.Where(r => r.DataReserva >= DateTime.Now.AddMonths(-12) && r.DataReserva <= DateTime.Now)
				.GroupBy(r => new { date = new DateTime(r.DataReserva.Year, r.DataReserva.Month, 1) })
                .Select(r => new
				{
					Mes = r.Key.date,
					Qtd = r.Count()
				})
                .ToListAsync();

			dadosReservas = dadosReservas.OrderBy(d => d.Mes).ToList();

			DataRow dr;
			foreach (var reserva in dadosReservas)
			{
				dr = dt.NewRow();
				dr["Reservas"] = reserva.Mes.ToString("MMM/yyyy");
				dr["Quantidade"] = reserva.Qtd;
				dt.Rows.Add(dr);
			}

			foreach (DataColumn dc in dt.Columns)
			{
				List<object> x = new List<object>();
				x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
				dados.Add(x);
			}
			return Json(dados);
		}

		private IEnumerable<SelectListItem> GetCategoria(List<Categoria> categorias)
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
		private IEnumerable<SelectListItem> GetEstado()
		{
			return new SelectListItem[]{
				new SelectListItem() { Text = " ", Value = string.Empty },
				new SelectListItem() { Text = "Pendente", Value = StatusReserva.pending.ToString() },
				new SelectListItem() { Text = "Rejeitada", Value = StatusReserva.rejected.ToString() },
				new SelectListItem() { Text = "Aprovada", Value = StatusReserva.approved.ToString() },
				new SelectListItem() { Text = "Levantada", Value = StatusReserva.provided.ToString() },
				new SelectListItem() { Text = "Concluida", Value = StatusReserva.delivered.ToString() },
			};
		}

		private IEnumerable<SelectListItem> GetOrder()
		{
			return new SelectListItem[]{
				new SelectListItem() { Text = " ", Value = string.Empty },
				new SelectListItem() { Text = "Estado asc", Value = "estado asc"},
				new SelectListItem() { Text = "Estado Desc", Value = "estado desc" },
				new SelectListItem() { Text = "Data de Levantamento Asc", Value = "datadelevantamento asc" },
				new SelectListItem() { Text = "Data de Levantamento Desc", Value = "datadelevantamento desc" },
				new SelectListItem() { Text = "Data de Entrega Asc", Value = "datadeentrega asc" },
				new SelectListItem() { Text = "Data de Entrega Desc", Value = "datadeentrega desc" },

				(User.IsInRole("Cliente") ?
				new SelectListItem() { Text = "Nome da Empresa Asc", Value = "nomedaempresa asc" } :
				new SelectListItem() { Text = "Nome do Cliente Asc", Value = "nomedocliente asc" }),

				(User.IsInRole("Cliente") ?
				new SelectListItem() { Text = "Nome da Empresa Desc", Value = "nomedaempresa desc" } :
				new SelectListItem() { Text = "Nome do Cliente Desc", Value = "nomedocliente desc" }),

				new SelectListItem() { Text = "Veiculo Asc", Value = "veiculo asc" },
				new SelectListItem() { Text = "Veiculo Desc", Value = "veiculo desc" },
				new SelectListItem() { Text = "Categoria Asc", Value = "categoria asc" },
				new SelectListItem() { Text = "Categoria Desc", Value = "categoria desc" },
			};
		}

		private IQueryable<Reserva> OrderBy(string order, IQueryable<Reserva> reservas)
		{
			switch (order)
			{
				case "estado asc":
					{
						reservas = reservas.OrderBy(r => r.Estado);
						break;
					}
				case "estado desc":
					{
						reservas = reservas.OrderByDescending(r => r.Estado);
						break;
					}
				case "datadelevantamento asc":
					{
						reservas = reservas.OrderBy(r => r.DataLevantamento);
						break;
					}
				case "datadelevantamento desc":
					{
						reservas = reservas.OrderByDescending(r => r.DataEntrega);
						break;
					}
				case "datadeentrega asc":
					{
						reservas = reservas.OrderBy(r => r.DataEntrega);
						break;
					}
				case "datadeentrega desc":
					{
						reservas = reservas.OrderByDescending(r => r.DataEntrega);
						break;
					}
				case "nomedocliente asc":
					{
						reservas = reservas.OrderBy(r => r.ApplicationUser.PrimeiroNome + r.ApplicationUser.UltimoNome);
						break;
					}
				case "nomedocliente desc":
					{
						reservas = reservas.OrderByDescending(r => r.ApplicationUser.PrimeiroNome + r.ApplicationUser.UltimoNome);
						break;
					}
				case "nomedaempresa asc":
					{
						reservas = reservas.OrderBy(r => r.Veiculo.Empresa.Nome);
						break;
					}
				case "nomedaempresa desc":
					{
						reservas = reservas.OrderByDescending(r => r.Veiculo.Empresa.Nome);
						break;
					}
				case "veiculo asc":
					{
						reservas = reservas.OrderBy(r => r.Veiculo.Nome);
						break;
					}
				case "veiculo desc":
					{
						reservas = reservas.OrderByDescending(r => r.Veiculo.Nome);
						break;
					}
				case "categoria asc":
					{
						reservas = reservas.OrderBy(r => r.Veiculo.Categoria.Nome);
						break;
					}
				case "categoria desc":
					{
						reservas = reservas.OrderByDescending(r => r.Veiculo.Categoria.Nome);
						break;
					}
			}



			return reservas;
		}

		private IQueryable<Reserva> SearchForEstado(String Estado, IQueryable<Reserva> Reservas)
		{
			StatusReserva? status = null;
			if (Estado == null) return Reservas;

			if (StatusReserva.approved.ToString() == Estado)
			{
				status = StatusReserva.approved;
			}
			else if (StatusReserva.pending.ToString() == Estado)
			{
				status = StatusReserva.pending;
			}
			else if (StatusReserva.delivered.ToString() == Estado)
			{
				status = StatusReserva.delivered;
			}
			else if (StatusReserva.rejected.ToString() == Estado)
			{
				status = StatusReserva.rejected;
			}
			else if (StatusReserva.provided.ToString() == Estado)
			{
				status = StatusReserva.provided;
			}

			if (status != null)
			{
				return Reservas.Where(r => r.Estado == status);
			}

			return Reservas;
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


		private bool ReservaExists(int id)
		{
			return (_context.Reservas?.Any(e => e.ReservaId == id)).GetValueOrDefault();
		}
	}
}
