using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

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
        public double Kms { get; set; }
        public bool Danos { get; set; }
        public ApplicationUser Funcionario { get; set; }
        public string Observacoes { get; set; }
        
        public int ReservaId { get; set; }
        
        public Reserva Reserva { get; set; }

        [DefaultValue(RegistoType.LEVANTAMENTO)]
        public RegistoType Tipo { get; set;}
    }
}
