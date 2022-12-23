using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;

namespace Rental4You.Models
{
    public class ApplicationUser : IdentityUser
    {
        [DisplayName("Primeiro Nome")]
        [Required(ErrorMessage = "Necessita indicar o primeiro nome")]
        public string PrimeiroNome { get; set; }

        [DisplayName("Último Nome")]
        [Required(ErrorMessage = "Necessita indicar o último nome")]
        public string UltimoNome { get; set; }

        [Display(Name = "Data de Nascimento")]
        [Required(ErrorMessage = "Necessita indicar a data de nascimento")]
        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }

        [StringLength(9, ErrorMessage = "O {0} tem de ser composto por {1} digitos.", MinimumLength = 9)]
        [RegularExpression(@"[0-9]{9}$", ErrorMessage = "O NIF tem de ser composto por 9 digitos.")]
        [Required(ErrorMessage = "Necessita indicar número de contribuinte")]
        public string NIF { get; set; }

        [DefaultValue(true)]
        public bool Active { get; set; }

        public int? EmpresaId { get; set; }

        public Empresa Empresa { get; set; }

        public ICollection<Reserva> Reservas { get; set; }

        public ICollection<Registo> Registos { get; set; }
        
    }
}
