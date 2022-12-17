using Microsoft.AspNetCore.Mvc;
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
            PesquisaParametersViewModel vm = new PesquisaParametersViewModel();
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

            return View(vm);
        }



        
        public IActionResult Search([Bind("Localizacao, DataLevantamento, DataEntrega")] PesquisaParametersViewModel pesquisa)
        {
            if(pesquisa.Localizacao == null)
            {
                return View("Index");
            }
            List<Veiculo> model;
            var empresas = _context.Empresas.Where(empresa => empresa.Localidade.Contains(pesquisa.Localizacao)).Select(empresa => empresa.Id).ToList();
            if(empresas == null || empresas.Count() == 0)
            {
                return View(new List<Veiculo>());
            }
            var veiculos = _context.Veiculos.Join(empresas, veiculo => veiculo.EmpresaId, empresa => empresa, (veiculo, empresa) => veiculo);
            veiculos = veiculos.Where(veiculo => veiculo.Reservas.Where(reserva => Sobrepoe(reserva, pesquisa.DataLevantamento, pesquisa.DataEntrega)).Count() == 0);

            return View(veiculos);
        }


        private bool Sobrepoe(Reserva reserva, DateTime levantamento, DateTime entrega)
        {
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