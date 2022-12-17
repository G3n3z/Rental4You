namespace Rental4You.Models
{
    public class Reserva
    {
        public int ReservaId { get; set; }
        public bool Concluido { get; set; }
        public DateTime DataLevantamento { get; set; }
        public DateTime DataEntrega { get; set; }

        public int VeiculoId { get; set; }
        public Veiculo Veiculo { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public Registo? Levantamento { get; set; }
        public Registo? Entrega { get; set; }

        public Avaliacao? Avaliacao { get; set; }
    }
}
