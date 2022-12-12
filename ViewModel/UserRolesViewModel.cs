using System;
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
        public string UserName { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public Boolean IsActive { get; set; }
    }

}

