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
            SearchViewModel vm = new SearchViewModel();
            DateTime now = DateTime.Now;
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


        public IActionResult Search2([Bind("Localizacao, DataLevantamento, DataEntrega, Categoria")] PesquisaParametersViewModel pesquisa)
        {
            if(pesquisa.Localizacao == null)
            {
                ViewData["Categorias"] = GetFilterCategorias(_context.Categorias.ToList());
                return View("Index");
            }
            List<Veiculo> model;
            var empresas = _context.Empresas.Where(empresa => empresa.Localidade.Contains(pesquisa.Localizacao)).Select(empresa => empresa.Id).ToList();
            if(empresas == null || empresas.Count() == 0)
            {
                return View(new List<Veiculo>());
            }
            if (pesquisa.DataLevantamento > pesquisa.DataEntrega)
            {
                return RedirectToAction(nameof(Index));
            }

            var veiculos = _context.Veiculos.Include(v => v.Empresa).Where(veiculo => empresas.Contains(veiculo.EmpresaId) &&
                veiculo.CategoriaId == pesquisa.Categoria);
            
            veiculos = veiculos.Where(veiculo => veiculo.Reservas == null ||
                                                  veiculo.Reservas.Count() == 0 ||
                                                  veiculo.Reservas.Where(reserva =>
                                                      (reserva.DataLevantamento < pesquisa.DataLevantamento && reserva.DataEntrega > pesquisa.DataLevantamento) ||
                                                       (reserva.DataLevantamento < pesquisa.DataEntrega && reserva.DataEntrega > pesquisa.DataEntrega)
                                                    ).Count() >= 0);
            model = veiculos.ToList();
            double dias = (pesquisa.DataEntrega - pesquisa.DataLevantamento).TotalHours / 24;
            SearchViewModel viewModels = new SearchViewModel();
            viewModels.Veiculos = new List<SearchVeiculosViewModel>();
            viewModels.DataEntrega = pesquisa.DataEntrega;
            viewModels.DataLevantamento = pesquisa.DataLevantamento;
            viewModels.Categoria = pesquisa.Categoria;
            viewModels.Localizacao = pesquisa.Localizacao;


            foreach (var v in model)
            {
                SearchVeiculosViewModel vm = new SearchVeiculosViewModel();
                vm.Veiculo = v;
                vm.Empresa = v.Empresa;
                vm.Preco = ((int)Math.Ceiling(dias)) * v.CustoDia;
                viewModels.Veiculos.Add(vm);
            }
            ViewData["pesquisa"] = pesquisa;
            ViewData["Empresas"] = GetFilterEmpresas(_context.Empresas.ToList());
            ViewData["Categorias"] = GetFilterCategorias(_context.Categorias.ToList());;
            ViewData["Order"] =  GetOrders();
            return View(viewModels);
        }


        public IActionResult Search([Bind("Localizacao, DataLevantamento, DataEntrega, Categoria, FiltroEmpresa, FiltroCategoria, OrderEmpresa, OrderCategoria")] SearchViewModel pesquisa)
        {
            ViewData["pesquisa"] = pesquisa;
            
            ViewData["Empresas"] = GetFilterEmpresas(_context.Empresas.ToList());
            ViewData["Categorias"] = GetFilterCategorias(_context.Categorias.ToList());;
            ViewData["Order"] = GetOrders();

            if (pesquisa.Localizacao == null)
            {
                ViewData["Categorias"] = GetFilterCategorias(_context.Categorias.ToList());;
                return RedirectToAction(nameof(Index));
            }
            
            var empresasList = _context.Empresas.Where(empresa => empresa.Localidade.Contains(pesquisa.Localizacao)).ToList();
            if (empresasList == null || empresasList.Count() == 0)
            {
                return View("Search",new List<Veiculo>());
            }

           // ViewData["Empresas"] = new SelectList(empresasList, "Id", "Nome");
            var empresas = empresasList.Select(empresa => empresa.Id).ToList();

            if (empresas == null || empresas.Count() == 0)
            {
                return View("Search", new List<Veiculo>());
            }
            if (pesquisa.DataLevantamento > pesquisa.DataEntrega)
            {
                return RedirectToAction(nameof(Index));
            }

            var veiculos = _context.Veiculos.Include(v => v.Empresa).Include(v => v.Categoria).Where(veiculo => empresas.Contains(veiculo.EmpresaId));

            veiculos = veiculos.Where(veiculo => veiculo.Reservas == null ||
                                                  veiculo.Reservas.Count() == 0 ||
                                                  veiculo.Reservas.Where(reserva =>
                                                      (reserva.DataLevantamento < pesquisa.DataLevantamento && reserva.DataEntrega > pesquisa.DataLevantamento) ||
                                                       (reserva.DataLevantamento < pesquisa.DataEntrega && reserva.DataEntrega > pesquisa.DataEntrega)
                                                    ).Count() >= 0);
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
            items[0] = new SelectListItem() { Text = " ", Value = string.Empty };
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