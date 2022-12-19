using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Rental4You.Models
{
    public class Empresa
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public string Localidade { get; set; }


        [DefaultValue(0.0)]
        public double MediaAvaliacao { get; set; }

        [Display(Name = "Subscrição")]
        [DefaultValue(true)]
        public bool Activo { get; set; }

        public ICollection<Veiculo> Veiculos { get; set; }

        public ICollection<Avaliacao> Avaliacoes { get; set; }

        public ICollection<ApplicationUser> Utilizadores { get; set; }

    }
}
