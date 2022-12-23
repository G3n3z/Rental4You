using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Rental4You.Models;

namespace Rental4You.Data
{
    public class ApplicationDbContext : IdentityDbContext <ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Registo> Registos { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Avaliacao>()
                .HasOne(p => p.Reserva)
                .WithOne(b => b.Avaliacao).OnDelete(DeleteBehavior.ClientCascade); 

            modelBuilder.Entity<Reserva>()
            .HasOne(s => s.Levantamento) // Mark Address property optional in Student entity
            .WithOne()
            .HasForeignKey<Reserva>(ars => ars.LevantamentoId);

            modelBuilder.Entity<Reserva>()
            .HasOne(s => s.Entrega).WithOne()
            .HasForeignKey<Reserva>(ars => ars.EntregaId);
    }
    }

}