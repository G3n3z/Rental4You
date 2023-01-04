using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Rental4You.Models
{
    public class Empresa
    {
        public int Id { get; set; }

		[RegularExpression(@"[a-zA-Z0-9-]+$", ErrorMessage = "Não podem ser usados caractéres especiais ou com acentuação")]
		public string Nome { get; set; }

        public string Localidade { get; set; }


        [DefaultValue(0.0)]
        [BindProperty, DisplayFormat(DataFormatString = "{0: #.#}", ApplyFormatInEditMode = true)]
        public double MediaAvaliacao { get; set; }

        [Display(Name = "Subscrição")]
        [DefaultValue(true)]
        public bool Activo { get; set; }

        public ICollection<Veiculo> Veiculos { get; set; }

        public ICollection<Avaliacao> Avaliacoes { get; set; }

        public ICollection<ApplicationUser> Utilizadores { get; set; }

    }
}
