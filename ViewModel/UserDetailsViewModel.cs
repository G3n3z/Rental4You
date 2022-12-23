using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Rental4You.Models;

namespace Rental4You.ViewModel
{
    public class UserDetailsViewModel
    {
        public string Id;

        [DisplayName("Primeiro Nome")]
        public string PrimeiroNome { get; set; }

        [DisplayName("Último Nome")]
        public string UltimoNome { get; set; }

        [Display(Name = "Data de Nascimento")]
        [Required(ErrorMessage = "Necessita indicar a data de nascimento")]
        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }

        [StringLength(9, ErrorMessage = "O {0} tem de ser composto por {1} digitos.", MinimumLength = 9)]
        [RegularExpression(@"[0-9]{9}$", ErrorMessage = "O NIF tem de ser composto por 9 digitos.")]
        [Required(ErrorMessage = "Necessita indicar número de contribuinte")]
        public string NIF { get; set; }
        public int? EmpresaId { get; set; }
        public bool Active { get; set; }
        public List<RolesViewModel> roles { get; set; }

        public static UserDetailsViewModel mapToViewModel(ApplicationUser user)
        {
            UserDetailsViewModel obj = new UserDetailsViewModel();
            obj.Id = user.Id;
            obj.PrimeiroNome = user.PrimeiroNome;
            obj.UltimoNome = user.UltimoNome;
            obj.DataNascimento = user.DataNascimento;
            obj.NIF = user.NIF;
            obj.Active = user.Active;
            return obj;
        }
    }
}

