using Rental4You.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Rental4You.ViewModel
{
    public class UserRolesViewModel
    {
        public UserRolesViewModel()
        {
        }
        public string UserId { get; set; }

        [DisplayName("Primeiro Nome")]
        public string PrimeiroNome { get; set; }

        [DisplayName("Último Nome")]
        public string UltimoNome { get; set; }

        [Required(ErrorMessage = "Necessita indicar o Email")]
        public string Email { get; set; }

        [DisplayName("Role")]
        public IEnumerable<string> Roles { get; set; }
        
        [DisplayName("Estado")]
        public Boolean IsActive { get; set; }
        public Boolean HaveRegister { get; set; }


        public static UserRolesViewModel mapUserToViewModel(ApplicationUser user, IList<string> list)
        {
            UserRolesViewModel viewModel = new UserRolesViewModel();
            viewModel.UserId = user.Id;
            viewModel.Email = user.Email;
            viewModel.PrimeiroNome = user.PrimeiroNome;
            viewModel.UltimoNome = user.UltimoNome;
            viewModel.IsActive = user.Active;
            viewModel.HaveRegister = user.Registos == null ? false : user.Registos.Count() != 0; 
            viewModel.Roles = list;
            return viewModel;
        }
        
    }



}

