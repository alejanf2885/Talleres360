using Microsoft.EntityFrameworkCore;
using Talleres360.Models;

namespace Talleres360.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Taller> Talleres { get; set; }
        public DbSet<Credencial> Credenciales { get; set; }
        public DbSet<Plan> Planes { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<TokenSeguridad> TokensSeguridad { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Modelo> Modelos { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<VehiculoDetalle> VehiculosDetalle { get; set; }
        public DbSet<UsuarioVerificacion> UsuarioVerificaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Rol)
                .HasConversion<string>();

            modelBuilder.Entity<Usuario>().HasQueryFilter(u => !u.Eliminado);
            modelBuilder.Entity<Credencial>().HasQueryFilter(c => !c.Eliminado);
            modelBuilder.Entity<Cliente>().HasQueryFilter(c => !c.Eliminado);
            modelBuilder.Entity<Vehiculo>().HasQueryFilter(v => !v.Eliminado);

            modelBuilder.Entity<Plan>()
                .Property(p => p.PrecioMensual)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Plan>()
                .Property(p => p.PrecioAnual)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Vehiculo>()
                .Property(v => v.PromedioKmDiarios)
                .HasPrecision(8, 2);

            modelBuilder.Entity<VehiculoDetalle>()
                .ToView("VW_VehiculoDetalles")
                .HasNoKey();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Credencial>()
                .HasIndex(c => c.UsuarioId);

            modelBuilder.Entity<Marca>()
                .HasIndex(m => new { m.Nombre, m.TallerId })
                .HasFilter("[EsOficial] = 0")
                .IsUnique();

            modelBuilder.Entity<Marca>()
                .HasIndex(m => m.Nombre)
                .HasFilter("[EsOficial] = 1")
                .IsUnique();
        }
    }
}