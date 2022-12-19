using System;
using Rental4You.Models;

namespace Rental4You.ViewModel
{
    public class SearchViewModel
    {
        public SearchViewModel()
        {
        }

        public List<SearchVeiculosViewModel> Veiculos { get; set; }
        public string Localizacao { get; set; }
        public DateTime DataLevantamento { get; set; }
        public DateTime DataEntrega { get; set; }
        public int Categoria { get; set; }
        public string FiltroEmpresa { get; set; }
        public string FiltroCategoria { get; set; }
        public string Order { get; set; }

    }
}

