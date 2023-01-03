using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Rental4You.Validations;

namespace Rental4You.Models
{
    public enum RegistoType
    {
        LEVANTAMENTO, ENTREGA
    }

    public class Registo
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }

        [KmsReserva]
        [Range(0, Double.PositiveInfinity, ErrorMessage ="O valor deve ser superior a 0")]
        public double Kms { get; set; }
        public bool Danos { get; set; }
        public ApplicationUser Funcionario { get; set; }

        [StringLength(400, ErrorMessage ="As observações devem ter no maximo 400 caracteres")]
        public string Observacoes { get; set; }
        
        public int ReservaId { get; set; }
        
        public Reserva Reserva { get; set; }

        [DefaultValue(RegistoType.LEVANTAMENTO)]
        public RegistoType Tipo { get; set;}
    }
}
