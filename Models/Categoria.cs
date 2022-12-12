namespace Rental4You.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public ICollection<Veiculo> Veiculos { get; set; }
    }
}
