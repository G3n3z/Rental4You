using System.ComponentModel.DataAnnotations.Schema;

namespace Rental4You.Models
{
    public class Avaliacao
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }
        public double Nota { get; set; }

        [ForeignKey("ReservaId")]
        public Reserva Reserva { get; set; }
        public int ReservaId { get; set; }

    }
}
