namespace Rental4You.Models
{
    public class Registo
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public double Kms { get; set; }
        public bool Danos { get; set; }
        public ApplicationUser Funcionario { get; set; }
        public string Observacoes { get; set; }
    }
}
