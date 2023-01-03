using System;
using Microsoft.AspNetCore.Identity;
using Rental4You.Data;
using Rental4You.Models;

namespace Escola_Segura.Data
{
    public enum Roles
    {
        Admin,
        Funcionario,
        Cliente,
        Gestor
    }
    public static class Inicializacao
    {
        public static async Task CriaDadosIniciais(UserManager<ApplicationUser>
            userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context )
        {
            //Adicionar default Roles
            //await roleManager.DeleteAsync(roleManager.Roles.Where(r => r.Name == "Formador").First());
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Funcionario.ToString()));
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
                PhoneNumberConfirmed = true,
                Active = true
            };
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Is3C..00");
                await userManager.AddToRoleAsync(defaultUser,
               Roles.Admin.ToString());
            }
            else
            {
                return;
            }

            var empresa1 = new Empresa();
            empresa1.Activo = true;
            empresa1.Localidade = "Coimbra";
            empresa1.Nome = "RentaCoimbra";
            empresa1.MediaAvaliacao = -1;
            context.Add(empresa1);


            var empresa2 = new Empresa();
            empresa2.Activo = true;
            empresa2.Localidade = "Coimbra";
            empresa2.Nome = "RentaEiras";
            empresa2.MediaAvaliacao = -1;
            context.Add(empresa2);
            await context.SaveChangesAsync();

            var empresa3 = new Empresa();
            empresa3.Activo = true;
            empresa3.Localidade = "Porto";
            empresa3.Nome = "RentaPorto";
            empresa3.MediaAvaliacao = -1;
            context.Add(empresa3);
            await context.SaveChangesAsync();

            var user1 = new ApplicationUser
            {
                UserName = "gestor@rentacoimbra.com",
                Email = "gestor@rentacoimbra.com",
                PrimeiroNome = "Gestor",
                UltimoNome = "RentaCoimbra",
                NIF = "11111111",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                EmpresaId = empresa1.Id,
                Active = true
            };



            var user2 = new ApplicationUser
            {
                UserName = "gestor@rentaeiras.com",
                Email = "gestor@rentaeiras.com",
                PrimeiroNome = "Gestor",
                UltimoNome = "RentaEiras",
                NIF = "11111111",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                EmpresaId = empresa2.Id,
                Active = true
            };

            var user5 = new ApplicationUser
            {
                UserName = "gestor@rentaporto.com",
                Email = "gestor@rentaporto.com",
                PrimeiroNome = "Gestor",
                UltimoNome = "RentaPorto",
                NIF = "11111111",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                EmpresaId = empresa3.Id,
                Active = true
            };


            await userManager.CreateAsync(user1, "Asd123!");
            await userManager.AddToRoleAsync(user1,
           Roles.Gestor.ToString());

            await userManager.CreateAsync(user2, "Asd123!");
            await userManager.AddToRoleAsync(user2,
           Roles.Gestor.ToString());
            

            await userManager.CreateAsync(user5, "Asd123!");
            await userManager.AddToRoleAsync(user5,
           Roles.Gestor.ToString());
            
            await context.SaveChangesAsync();

            var user3 = new ApplicationUser
            {
                UserName = "func@rentaeiras.com",
                Email = "func@rentaeiras.com",
                PrimeiroNome = "Carlos",
                UltimoNome = "Josefino",
                NIF = "11111111",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                EmpresaId = empresa2.Id,
                Active = true
            };


            var user4 = new ApplicationUser
            {
                UserName = "func@rentacoimbra.com",
                Email = "func@rentacoimbra.com",
                PrimeiroNome = "Daniel",
                UltimoNome = "Americo",
                NIF = "11111111",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                EmpresaId = empresa1.Id,
                Active = true
            };


            await userManager.CreateAsync(user3, "Asd123!");
            await userManager.AddToRoleAsync(user3,
           Roles.Funcionario.ToString());

            await userManager.CreateAsync(user4, "Asd123!");
            await userManager.AddToRoleAsync(user4,
           Roles.Funcionario.ToString());
            //var empresa1 = context.Empresas.Where(v => v.Nome == "RentaCoimbra").First();
            //var empresa2 = context.Empresas.Where(v => v.Nome == "RentaEiras").First();

            var user6 = new ApplicationUser
            {
                UserName = "daniel@fake.com",
                Email = "daniel@fake.com",
                PrimeiroNome = "Daniel",
                UltimoNome = "Fernandes",
                NIF = "12345678",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                Active = true
            };

            var user7 = new ApplicationUser
            {
                UserName = "hugo@fake.com",
                Email = "hugo@fake.com",
                PrimeiroNome = "Hugo",
                UltimoNome = "Jorge",
                NIF = "12348678",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                Active = true
            };
            await userManager.CreateAsync(user6, "Asd123!");
            await userManager.AddToRoleAsync(user6,
           Roles.Cliente.ToString());

            await userManager.CreateAsync(user7, "Asd123!");
            await userManager.AddToRoleAsync(user7,
           Roles.Cliente.ToString());

            Categoria categoria = new Categoria();
            categoria.Nome = "Jeep";
            Categoria categoria1 = new Categoria();
            categoria1.Nome = "Carrinha";
            Categoria categoria2 = new Categoria();
            categoria2.Nome = "Citadino";

            context.Add(categoria);
            context.Add(categoria1);
            context.Add(categoria2);
            await context.SaveChangesAsync();

            Veiculo v = new Veiculo();
            v.EmpresaId = empresa1.Id;
            v.CategoriaId = categoria1.Id;
            v.Marca = "Renault";
            v.Modelo = "Megane";
            v.Nome = v.Marca + " " + v.Modelo;
            v.Ativo = true;
            v.Descricao = "Gasolina 1.5 5 portas";
            v.CustoDia = 25;
            v.Matricula = "AA58XX";

            Veiculo v1 = new Veiculo();
            v1.EmpresaId = empresa1.Id;
            v1.CategoriaId = categoria.Id;
            v1.Marca = "Land Rover";
            v1.Modelo = "Sport";
            v1.Nome = v1.Marca + " " + v1.Modelo;
            v1.Descricao = "Gasolina 2.5 5 portas";
            v1.Ativo = true;
            v1.CustoDia = 35;
            v1.Matricula = "DD46ZZ";



            Veiculo v2 = new Veiculo();
            v2.EmpresaId = empresa1.Id;
            v2.CategoriaId = categoria1.Id;
            v2.Marca = "Pegeot";
            v2.Modelo = "508";
            v2.Nome = v2.Marca + " " + v2.Modelo;
            v2.Ativo = true;
            v2.Descricao = "Gasolina 2.0 5 portas";
            v2.CustoDia = 20;
            v2.Matricula = "OO46DD";


            Veiculo v3 = new Veiculo();
            v3.EmpresaId = empresa2.Id;
            v3.CategoriaId = categoria1.Id;
            v3.Marca = "Renault";
            v3.Modelo = "Megane";
            v3.Nome = v3.Marca + " " + v3.Modelo;
            v3.Ativo = true;
            v3.Descricao = "Gasoleo 1.5 5 portas";
            v3.CustoDia = 23;
            v3.Matricula = "XX87DF";

            Veiculo v4 = new Veiculo();
            v4.EmpresaId = empresa1.Id;
            v4.CategoriaId = categoria2.Id;
            v4.Marca = "Citroen";
            v4.Modelo = "C3";
            v4.Descricao = "Gasolina 1.0 3 portas";
            v4.Nome = v4.Marca + " " + v4.Modelo;
            v4.Ativo = true;
            v4.CustoDia = 10;
            v4.Matricula = "EE76DD";

            Veiculo v5 = new Veiculo();
            v5.EmpresaId = empresa3.Id;
            v5.CategoriaId = categoria2.Id;
            v5.Marca = "Renault";
            v5.Modelo = "Clio";
            v5.Descricao = "Gasolina 1.2";
            v5.Nome = v5.Marca + " " + v5.Modelo;
            v5.Ativo = true;
            v5.CustoDia = 12;
            v5.Matricula = "OOUU66";

            context.Add(v);
            context.Add(v1);
            context.Add(v2);
            context.Add(v3);
            context.Add(v4);
            context.Add(v5);
            await context.SaveChangesAsync();
        }




    }
}

