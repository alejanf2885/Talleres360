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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- CONVERSIÓN DE ENUM A STRING ---
            modelBuilder.Entity<Usuario>()
                .Property(u => u.Rol)
                .HasConversion<string>();

            // --- FILTRO GLOBAL (SOFT DELETE) ---
            modelBuilder.Entity<Usuario>().HasQueryFilter(u => !u.Eliminado);
            modelBuilder.Entity<Credencial>().HasQueryFilter(c => !c.Eliminado);

            // --- CONFIGURACIÓN DE PRECISIÓN PARA DECIMAL ---
            modelBuilder.Entity<Plan>()
                .Property(p => p.PrecioMensual)
                .HasPrecision(18, 2); // 18 dígitos totales, 2 decimales

            // --- ÍNDICES PARA VELOCIDAD ---
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Índice en UsuarioId de Credenciales para mejorar rendimiento de consultas
            modelBuilder.Entity<Credencial>()
                .HasIndex(c => c.UsuarioId);
        }
    }
}