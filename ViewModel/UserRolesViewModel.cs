using System;
using Rental4You.Models;

namespace Rental4You.ViewModel
{
    public class UserRolesViewModel
    {
        public UserRolesViewModel()
        {
        }
        public string UserId { get; set; }
        public string PrimeiroNome { get; set; }
        public string UltimoNome { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; }
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

