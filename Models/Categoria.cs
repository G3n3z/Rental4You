using System.ComponentModel.DataAnnotations;

namespace Rental4You.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Necessita introduzir o nome da Categoria")]
        public string Nome { get; set; }
        public ICollection<Veiculo> Veiculos { get; set; }
    }
}
