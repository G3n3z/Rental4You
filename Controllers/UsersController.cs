using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Escola_Segura.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rental4You.Data;
using Rental4You.Models;
using Rental4You.ViewModel;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Rental4You.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            List<UserRolesViewModel> viewModel = new List<UserRolesViewModel>();
            List<ApplicationUser> users = null;
            ApplicationUser applicationUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Admin"))
            {
                users = _userManager.Users.ToList();

               
            }else if (User.IsInRole("Gestor"))
            {
                users = _userManager.Users.Where(user => user.EmpresaId == applicationUser.EmpresaId).ToList();
            }
            users?.ForEach(user =>
            {
                viewModel.Add(UserRolesViewModel.mapUserToViewModel(user, _userManager.GetRolesAsync(user).GetAwaiter().GetResult()));
            });
            
            return View(viewModel);
        }



        public async Task<IActionResult> Edit(string UserId)
        {
            if(String.IsNullOrEmpty(UserId))
            {
                return RedirectToAction("Index");

            }
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var roles = _roleManager.Roles.ToList();
            List<string> rolesUser = (List<string>)_userManager.GetRolesAsync(user).GetAwaiter().GetResult();
            UserDetailsViewModel userDetails = UserDetailsViewModel.mapToViewModel(user);
            
            userDetails.roles = new List<RolesViewModel>();

            foreach(var role in roles) { 
                RolesViewModel rolesViewModel = new RolesViewModel();
                rolesViewModel.RoleId = role.Id;
                rolesViewModel.RoleName = role.Name;
                rolesViewModel.Selected = rolesUser.Contains(role.Name);
                userDetails.roles.Add(rolesViewModel);
            }

            return View(userDetails);
        }



        [HttpPost]
        public async Task<IActionResult> Edit(UserDetailsViewModel userDetails, string userId)
        {
            if (String.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index");

            }

            ModelState.Remove(nameof(userDetails.roles));
            ApplicationUser user = _userManager.FindByIdAsync(userId).Result;
            user.UserName = user.Email;
            if (!ModelState.IsValid)
            {
                return View(userDetails);
            }
            _context.Update(user);
            await _context.SaveChangesAsync();

            List<string> rolesUser = (List<string>)_userManager.GetRolesAsync(user).GetAwaiter().GetResult();
            var result = await _userManager.RemoveFromRolesAsync(user, rolesUser);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Não é possivel remover uma role existente");
                return View(userDetails);
            }
            result = await _userManager.AddToRolesAsync(user, userDetails.roles.Where(m => m.Selected).Select(m => m.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Não é possivel adionar uma role existente");
                return View(userDetails);
            }

            return RedirectToAction("Index");
        }

    }
}

