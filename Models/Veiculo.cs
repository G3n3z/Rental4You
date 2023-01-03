using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Rental4You.Validations;

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

		[DisplayName("Descrição")]
		public string Descricao { get; set; }

        [Required(ErrorMessage = "Necessita indicar a marca")]
        public string Marca {get; set;}

        [Required(ErrorMessage = "Necessita indicar o modelo")]
        public string Modelo {get; set;}

        [DisplayName("Matrícula")]
		[StringLength(6)]
        [MatriculaUnique]
        [RegularExpression(@"[A-Z\d]{6}", ErrorMessage = "Matrícula inválida")]
        [Required(ErrorMessage = "Necessita indicar a matrícula")]
        public string Matricula {get; set;}
        
        [DisplayName("Custo por dia")]
		[DataType(DataType.Currency)]
		public double CustoDia { get; set; }

		[DisplayName("Ativo")]
		public bool Ativo { get; set; }

        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
        public ICollection<Reserva> Reservas { get; set; }
    }
}
