namespace Rental4You.Models
{
    public class Avaliacao
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }
        public int ReservaId { get; set; }
        public Reserva Reserva { get; set; }

    }
}
