using Microsoft.AspNetCore.Identity;

namespace Rental4You.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int Id { get; set; }
        public string PrimeiroName { get; set; }
        public string UltimoNome { get; set; }
        public DateTime DataNascimento { get; set; }
        public int NIF { get; set; }
        public int? EmpresaId { get; set; }
        public Empresa Empresa { get; set; }
        public ICollection<Reserva> Reservas { get; set; }
    }
}
