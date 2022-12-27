using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Escola_Segura.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
		[Authorize(Roles = "Admin, Gestor")]
		public async Task<IActionResult> Index()
        {
            List<UserRolesViewModel> viewModel = new List<UserRolesViewModel>();
            List<ApplicationUser> users = null;
            ApplicationUser applicationUser = await _userManager.GetUserAsync(User);
            ViewBag.userId = applicationUser.Id;
            if (User.IsInRole("Admin"))
            {
                users = _userManager.Users.ToList();


            }
            else if (User.IsInRole("Gestor"))
            {
                users = _userManager.Users.Where(user => user.EmpresaId == applicationUser.EmpresaId).ToList();
            }
            users?.ForEach(user =>
            {
                viewModel.Add(UserRolesViewModel.mapUserToViewModel(user, _userManager.GetRolesAsync(user).GetAwaiter().GetResult()));
            });

            return View(viewModel);
        }

        [Authorize(Roles = "Gestor")]
        public async Task<IActionResult> Create()
        {
            List<IdentityRole> roles = _roleManager.Roles.Where(r => r.Name == Roles.Gestor.ToString() || r.Name == Roles.Funcionario.ToString()).ToList();
            ViewData["Roles"] = new SelectList(roles, "Name", "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Gestor")]
        public async Task<IActionResult> Create([Bind("Id,PrimeiroNome,UltimoNome,DataNascimento,Email,NIF,EmpresaId")] ApplicationUser user, string role)
        {
            if (user.DataNascimento >= DateTime.Today)
            {
                ModelState.AddModelError("DataNascimento", "A data de nascimento não pode ser igual ou superior à data atual");

                return View(user);
            }
            user.Active = true;
            ModelState.Remove(nameof(user.Empresa));
            ModelState.Remove(nameof(user.Reservas));
            ModelState.Remove(nameof(user.Registos));

            List<IdentityRole> roles;
            roles = _roleManager.Roles.Where(r => r.Name == Roles.Gestor.ToString() || r.Name == Roles.Funcionario.ToString()).ToList();
            ViewData["Roles"] = new SelectList(roles, "Name", "Name");

            if (role == null || (role != Roles.Funcionario.ToString() && role != Roles.Gestor.ToString()))
            {
                return View(user);
            }
            ApplicationUser gestor = await _userManager.GetUserAsync(User);
            user.EmpresaId = gestor.EmpresaId;
            user.EmailConfirmed = true;


            if (ModelState.IsValid)
            {
                user.UserName = user.Email;
                var result = await _userManager.CreateAsync(user, "Funcionario123!");
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Nao foi possivel criar o utilizador");

                    return View(user);
                }
                result = await _userManager.AddToRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Nao foi possivel atribuir o role ao utilizador");
                    return View(user);
                }
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }


		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Edit(string UserId)
        {
            if (String.IsNullOrEmpty(UserId))
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

            foreach (var role in roles)
            {
                RolesViewModel rolesViewModel = new RolesViewModel();
                rolesViewModel.RoleId = role.Id;
                rolesViewModel.RoleName = role.Name;
                rolesViewModel.Selected = rolesUser.Contains(role.Name);
                userDetails.roles.Add(rolesViewModel);
            }

            return View(userDetails);
        }




        [HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Edit(UserDetailsViewModel userDetails, string userId)
        {
            if (String.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index");

            }

            if(userDetails.DataNascimento >= DateTime.Today)
            {
                ModelState.AddModelError("DataNascimento", "A data de nascimento não pode ser igual ou superior à data atual");
            }

            ModelState.Remove(nameof(userDetails.roles));
            ApplicationUser user = _userManager.FindByIdAsync(userId).Result;
            user.UserName = user.Email;
            if (!ModelState.IsValid)
            {
                userDetails.Id = user.Id;
                return View(userDetails);
            }

            user.DataNascimento = userDetails.DataNascimento;
            user.Active = userDetails.Active;
            user.NIF = userDetails.NIF;
            user.PrimeiroNome = userDetails.PrimeiroNome;
            user.UltimoNome = userDetails.UltimoNome;

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

        [HttpPost]
        [Authorize(Roles= "Admin")]
        public async Task<IActionResult> ChangeStatus(string userId, [Bind("Active")] Boolean Active)
        {
            if(userId == null)
            {
                return NotFound();
            }
            ApplicationUser user = _userManager.FindByIdAsync(userId).Result;
            if(user == null)
            {
                return NotFound();
            }
            user.Active = Active;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

		[Authorize(Roles = "Gestor")]
		public async Task<IActionResult> Delete(string userId)
        {
            if (userId == null || _context.Users == null)
            {
                return NotFound();
            }

			ApplicationUser applicationUser = await _userManager.GetUserAsync(User);
            if(applicationUser == null || applicationUser.Id == userId)
            {
                return BadRequest();
            }

			var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
		[Authorize(Roles = "Gestor")]
		public async Task<IActionResult> DeleteConfirmed(string Id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Users'  is null.");
            }
            var user = await _context.Users.FindAsync(Id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
            if(user.Registos != null && user.Registos.Count() != 0)
            {
                ModelState.AddModelError("","Não é possivel eliminar um utilizador com reservas");
                return View(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetDadosNovosUtilizadoresMensais()
		{
			//dados de exemplo
			List<object> dados = new List<object>();
			DataTable dt = new DataTable();
			dt.Columns.Add("Utilizadores", Type.GetType("System.String"));
			dt.Columns.Add("Quantidade", Type.GetType("System.Int32"));

			var dadosUsers = await _context.Users
				.Where(r => r.DataRegisto >= DateTime.Now.AddMonths(-12) && r.EmpresaId == null)
				.GroupBy(r => new { date = new DateTime(r.DataRegisto.Year, r.DataRegisto.Month, 1) })
				.Select(r => new
				{
					Mes = r.Key.date.ToString("MMM/yyyy"),
					Qtd = r.Count()
				})
				.ToListAsync();
			DataRow dr;
			foreach (var user in dadosUsers)
			{
				dr = dt.NewRow();
				dr["Utilizadores"] = user.Mes;
				dr["Quantidade"] = user.Qtd;
				dt.Rows.Add(dr);
			}

			foreach (DataColumn dc in dt.Columns)
			{
				List<object> x = new List<object>();
				x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
				dados.Add(x);
			}
			return Json(dados);
		}

	}
}

