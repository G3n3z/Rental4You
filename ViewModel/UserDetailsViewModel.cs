using System;
using Rental4You.Models;

namespace Rental4You.ViewModel
{
    public class UserDetailsViewModel
    {
        public string Id;
        public string PrimeiroNome { get; set; }
        public string UltimoNome { get; set; }
        public DateTime DataNascimento { get; set; }
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

