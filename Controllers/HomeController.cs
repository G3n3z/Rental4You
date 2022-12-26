using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rental4You.Data;
using Rental4You.Models;
using Rental4You.ViewModel;
using System.Diagnostics;
using System.Linq;

namespace Rental4You.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Admin")) 
            { 
                return RedirectToAction("DashboardAdmin");
            }
			if (User.IsInRole("Gestor"))
			{
				return RedirectToAction("DashboardGestor");
			}

			SearchViewModel vm = new SearchViewModel();
            DateTime now = DateTime.Now;
            now = now.AddMinutes(-now.Minute); // para colocar os minutos a 00 na view
            if(now.Hour < 8)
            {
                var h = 8 - now.Hour;
                now = now.AddHours(h);
            }else if(now.Hour >= 16)
            {
                var h = 24 + 8 - now.Hour;
                now = now.AddHours(h);
            }
            else
            {
                now = now.AddHours(2);
            }
            
            vm.DataLevantamento = now;
            vm.DataEntrega = vm.DataLevantamento.AddHours(24);
            ViewData["Categorias"]= GetFilterCategorias(_context.Categorias.ToList());

            return View(vm);
		}

        [Authorize(Roles = "Admin")]
		public async Task<IActionResult> DashboardAdmin()
        {
            return View();
        }

		[Authorize(Roles = "Gestor")]
		public async Task<IActionResult> DashboardGestor()
		{
			return View();
		}

		public IActionResult Search([Bind("Localizacao, DataLevantamento, DataEntrega, Categoria, FiltroEmpresa, FiltroCategoria, Order")] SearchViewModel pesquisa)
        {
            ViewData["pesquisa"] = pesquisa;
            
            ViewData["Empresas"] = GetFilterEmpresas(_context.Empresas.ToList());
            ViewData["Categorias"] = GetFilterCategorias(_context.Categorias.ToList());
            ViewData["Order"] = GetOrders();

            if (string.IsNullOrWhiteSpace(pesquisa.Localizacao))
            {
                ModelState.AddModelError("Localizacao", "Necessita indicar uma localização.");
                return View("Search", pesquisa);
            }
            
            var empresasList = _context.Empresas.Where(empresa => empresa.Localidade.Contains(pesquisa.Localizacao)).Include(empresa => empresa.Veiculos).ToList();
            if (empresasList == null || empresasList.Count() == 0)
            {
                //Verificar se nao da erro
                pesquisa.Veiculos = new List<SearchVeiculosViewModel>();

                //return NotFound($"Não existem empresas registadas em {pesquisa.Localizacao}! Por favor volte a efetuar a sua pesquisa.");
                ModelState.AddModelError("Localizacao", "Não eximtem empresas registadas na localização indicada");
                return View("Search", pesquisa);
            }

            if (pesquisa.DataLevantamento > pesquisa.DataEntrega)
            {
                ModelState.AddModelError("DataLevantamento", "A data de levantamento não pode ser superior à data de entrega");
                return View("Search", pesquisa);
            }
            if(pesquisa.DataLevantamento < DateTime.Now)
            {
                ModelState.AddModelError("DataLevantamento", "A data de levantamento não pode ser inferior à data atual");

                return View("Search", pesquisa);
            }

           // ViewData["Empresas"] = new SelectList(empresasList, "Id", "Nome");
            var empresas = empresasList.Select(empresa => empresa.Id).ToList();

            if (empresas == null || empresas.Count() == 0)
            {
                return View("Search", pesquisa);
            }
            var veiculos = _context.Veiculos.Include(v => v.Empresa).Include(v => v.Empresa.Avaliacoes)
            .Include(v => v.Categoria).Where(veiculo => empresas.Contains(veiculo.EmpresaId));
            veiculos = veiculos.Where(v => v.Empresa.Activo == true);
            veiculos = veiculos.Include(v => v.Reservas)
                                        .Where(veiculo => veiculo.Reservas == null || 
                                                  veiculo.Reservas.Count() == 0 ||
                                                  veiculo.Reservas.Where(reserva =>
                                                        //     13-12-2022 < 19-12-2022      &&   20-12-2022 < 24-12-2022  1-1
                                                        (pesquisa.DataLevantamento <= reserva.DataLevantamento && reserva.DataEntrega <= pesquisa.DataEntrega) ||
                                                        //     18-12-2022 < 19-12-2022      &&   18-12-2022 < 19-12-2022 1-1
                                                        (pesquisa.DataLevantamento <= reserva.DataLevantamento && reserva.DataLevantamento <= pesquisa.DataEntrega) ||
                                                       //     19-12-2022 < 20-12-2022      &&   20-12-2022 < 24-12-2022 1-1
                                                       (pesquisa.DataLevantamento <= reserva.DataEntrega && reserva.DataEntrega <= pesquisa.DataEntrega) ||
                                                       //     19-12-2022 11:00 > 19-12-2022 10:00     &&   20-12-2022 11:00 > 20-12-2022 10:00 1-1
                                                       (pesquisa.DataLevantamento >= reserva.DataLevantamento && reserva.DataEntrega >= pesquisa.DataEntrega)
                                                    ).Count() == 0);
            var aux = veiculos.ToList();
            if(pesquisa.FiltroCategoria != null)
            {
                veiculos = veiculos.Where(v => v.Categoria.Nome == pesquisa.FiltroCategoria);       
            }
            if (pesquisa.FiltroEmpresa != null)
            {
                veiculos = veiculos.Where(v => v.Empresa.Nome == pesquisa.FiltroEmpresa);
            }
            if(!String.IsNullOrEmpty(pesquisa.Order)){
                if(pesquisa.Order == "preco asc"){
                    veiculos = veiculos.OrderBy(v => v.CustoDia);
                }else if(pesquisa.Order == "preco desc"){
                    veiculos = veiculos.OrderByDescending(v => v.CustoDia);
                }else if(pesquisa.Order == "classificacao asc"){
                    veiculos = veiculos.OrderBy(v => v.Empresa.MediaAvaliacao);
                }
                else if(pesquisa.Order == "classificacao desc"){
                    veiculos = veiculos.OrderByDescending(v => v.Empresa.MediaAvaliacao);
                }
            }
            List<Veiculo> model = veiculos.ToList();
            double dias = (pesquisa.DataEntrega - pesquisa.DataLevantamento).TotalHours / 24;
            SearchViewModel viewModels = new SearchViewModel();
            viewModels.Veiculos = new List<SearchVeiculosViewModel>();



            foreach (var v in model)
            {
                SearchVeiculosViewModel vm = new SearchVeiculosViewModel();
                vm.Veiculo = v;
                vm.Empresa = v.Empresa;
                vm.Preco = ((int)Math.Ceiling(dias)) * v.CustoDia;
                viewModels.Veiculos.Add(vm);
            }
            
            return View("Search", viewModels);
        }

        private bool checkReservas(ICollection<Reserva> reservas, DateTime levantamento, DateTime entrega)
        {
            if(reservas == null)
            {
                return true;
            }
            reservas = reservas.Where(reserva => !Sobrepoe(reserva, levantamento, entrega)).ToList();

            return reservas.Count() == 0;
        }

        private bool Sobrepoe(Reserva reserva, DateTime levantamento, DateTime entrega)
        {
            if (reserva == null)
                return false;
            if(reserva.DataLevantamento < levantamento && reserva.DataEntrega > levantamento)
            {
                return true;
            }

            if(reserva.DataLevantamento < entrega && reserva.DataEntrega > entrega)
            {
                return true;
            }


            return false;
        }

        private IEnumerable<SelectListItem> GetOrders()
        {
            return new SelectListItem[]
            {
                new SelectListItem() { Text = " ", Value = string.Empty },
                new SelectListItem() { Text = "Preco Asc", Value = "preco asc" },
                new SelectListItem() { Text = "Preco Desc", Value = "preco desc" },
                new SelectListItem() { Text = "Classificação Asc", Value = "classificacao asc" },
                new SelectListItem() { Text = "Classificação Desc", Value = "classificacao desc" }
            };
        }
        private IEnumerable<SelectListItem> GetFilterEmpresas(List<Empresa> empresas)
        {
            var filter = empresas.Select(empresa => empresa.Nome).Distinct().ToArray();
            var items = new SelectListItem[filter.Count() + 1];
            items[0] = new SelectListItem() { Text = " ", Value = string.Empty };
            for (int i = 1; i < filter.Count()+1; i++)
            {
                items[i] = new SelectListItem() { Text = filter[i-1], Value = filter[i-1] };
            }
            return items;
        }

        private IEnumerable<SelectListItem> GetFilterCategorias(List<Categoria> categorias)
        {
            var filter = categorias.Select(categoria => categoria.Nome).Distinct().ToArray();
            var items = new SelectListItem[filter.Count() + 1];
            items[0] = new SelectListItem() { Text = "Escolha a Categoria...", Value = string.Empty };
            for (int i = 1; i < filter.Count()+1; i++)
            {
                items[i] = new SelectListItem() { Text = filter[i-1], Value = filter[i-1] };
            }
            return items;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
    }
}