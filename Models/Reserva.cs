using System.ComponentModel;

namespace Rental4You.Models
{
    public enum StatusReserva
    {
        approved, delivered, pending, provided, rejected

    }
    public class Reserva
    {
        public int ReservaId { get; set; }
        public bool Concluido { get; set; }
        public DateTime DataLevantamento { get; set; }
        public DateTime DataEntrega { get; set; }
        public double CustoTotal { get; set; }
        public int VeiculoId { get; set; }
        public Veiculo Veiculo { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int? LevantamentoId { get; set; }
        public Registo? Levantamento { get; set; }
        
        public int? EntregaId { get; set; }
        public Registo? Entrega { get; set; }

        public Avaliacao? Avaliacao { get; set; }

        [DefaultValue(StatusReserva.pending)]
        public StatusReserva Estado {get; set;}
        public static string translate(StatusReserva? status){
            if(status == null) return "";
            switch(status){
                case StatusReserva.pending :
                    return "Pendente";
                case StatusReserva.approved:
                    return "Aprovado";
                case StatusReserva.rejected:
                    return "Rejeitado";
                default: 
                    return "";  
            }
        }
    }
}
