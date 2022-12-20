using System.ComponentModel.DataAnnotations.Schema;
namespace Rental4You.Models
{
    public enum StatusVeiculo
    {
        DISPONIVEL, A_CIRCULAR
    }
    public class Veiculo
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Localizacao { get; set; }
        public double CustoDia { get; set; }
        public bool Disponivel { get; set; }

        // public string Marca {get; set;}

        // public string Modelo {get; set;}
        
        // public string Matricula {get; set;}

        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
        public ICollection<Reserva> Reservas { get; set; }
    }
}
