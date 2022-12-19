using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Rental4You.ViewModel
{
    public class PesquisaParametersViewModel
    {
        public PesquisaParametersViewModel()
        {
        }
        public string Localizacao { get; set; }
        public DateTime DataLevantamento { get; set; }
        public DateTime DataEntrega { get; set; }
        public int Categoria { get; set; }
        
    }
}

