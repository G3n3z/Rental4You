namespace Rental4You.Models
{
    public class Empresa
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Localidade { get; set; }
        public ICollection<Veiculo> Veiculos { get; set; }
        public ICollection<Avaliacao> Avaliacoes { get; set; }
        public ICollection<ApplicationUser> Utilizadores { get; set; }
    }
}
