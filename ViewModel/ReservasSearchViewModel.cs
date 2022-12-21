using System;
using Rental4You.Models;

namespace Rental4You.ViewModel
{
    public class ReservasSearchViewModel
    {
        public ReservasSearchViewModel()
        {
        }
        public List<Reserva> reservas {get; set;}
        public DateTime? DataLevantamento {get; set;}
        public DateTime? DataEntrega {get; set;}
        public string NomeCliente {get; set;}
        public string Veiculo {get; set;}
        public string Estado {get; set;}
        public string Categoria {get; set;}
        public string order {get; set;}
    }
}

