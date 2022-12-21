using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
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
        [DataType(DataType.DateTime)]
        [BindProperty, DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DataLevantamento { get; set; }
        public DateTime DataEntrega { get; set; }
        public int Categoria { get; set; }
        public string FiltroEmpresa { get; set; }
        public string FiltroCategoria { get; set; }
        public string Order { get; set; }

    }
}

