using System.ComponentModel;
using Microsoft.AspNetCore.Identity;

namespace Rental4You.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string PrimeiroNome { get; set; }

        public string UltimoNome { get; set; }

        public DateTime DataNascimento { get; set; }

        public string NIF { get; set; }

        [DefaultValue(true)]
        public bool Active { get; set; }

        public int? EmpresaId { get; set; }

        public Empresa Empresa { get; set; }

        public ICollection<Reserva> Reservas { get; set; }
    }
}
