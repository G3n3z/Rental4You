using System;
using Microsoft.AspNetCore.Identity;
using Rental4You.Models;

namespace Escola_Segura.Data
{
    public enum Roles
    {
        Admin,
        Formador,
        Cliente,
        Gestor
    }
    public static class Inicializacao
    {
        public static async Task CriaDadosIniciais(UserManager<ApplicationUser>
            userManager, RoleManager<IdentityRole> roleManager)
        {
            //Adicionar default Roles
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Formador.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Cliente.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Gestor.ToString()));
            //Adicionar Default User - Admin
            var defaultUser = new ApplicationUser
            {
                UserName = "admin@localhost.com",
                Email = "admin@localhost.com",
                PrimeiroNome = "Administrador",
                UltimoNome = "Admin",
                NIF = "11111111",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Is3C..00");
                await userManager.AddToRoleAsync(defaultUser,
               Roles.Admin.ToString());
            }
        }
    }
}

