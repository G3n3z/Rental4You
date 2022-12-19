using System;
using Rental4You.Models;

namespace Rental4You.ViewModel
{
    public class SearchVeiculosViewModel
    {
        public SearchVeiculosViewModel()
        {
        }
        public Veiculo Veiculo { get; set; }
        public Empresa Empresa { get; set; }
        public double Preco { get; set; }
    }
}

