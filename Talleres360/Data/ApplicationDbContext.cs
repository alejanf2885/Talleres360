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
        public DbSet<VehiculoTipo> VehiculoTipos { get; set; }
        public DbSet<Trabajo> Trabajos { get; set; }
        public DbSet<DetalleTrabajo> DetallesTrabajo { get; set; }
        public DbSet<NotaVehiculo> NotasVehiculo { get; set; }
        public DbSet<CategoriaProducto> CategoriasProducto { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<CobroTrabajo> CobrosTrabajo { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<LineaFactura> LineasFactura { get; set; }
        public DbSet<TarifaHora> TarifasHora { get; set; } = null!;
        public DbSet<AuditLog> AuditLog { get; set; } = null!;
        public DbSet<DesgloseIva> DesglosesIva { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Rol)
                .HasConversion<string>();

            modelBuilder.Entity<Trabajo>()
                .Property(t => t.Estado)
                .HasConversion<string>();

            modelBuilder.Entity<Trabajo>()
                .Property(t => t.EstadoPago)
                .HasConversion<string>();

            modelBuilder.Entity<Cita>()
                .Property(c => c.Estado)
                .HasConversion<string>();

            modelBuilder.Entity<NotaVehiculo>()
                .Property(n => n.Tipo)
                .HasConversion<string>();

            modelBuilder.Entity<Factura>()
                .Property(f => f.TipoDocumento)
                .HasConversion<string>();

            modelBuilder.Entity<CobroTrabajo>()
                .Property(c => c.MetodoPago)
                .HasConversion<string>();

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Operacion)
                .HasConversion<string>();

            modelBuilder.Entity<TarifaHora>()
                .Property(t => t.PrecioHora)
                .HasPrecision(10, 2);

            modelBuilder.Entity<DetalleTrabajo>()
                .Property(d => d.PrecioHoraAplicado)
                .HasPrecision(10, 2);

            modelBuilder.Entity<DetalleTrabajo>()
                .Property(d => d.HorasAplicadas)
                .HasPrecision(6, 2);

            modelBuilder.Entity<Usuario>().HasQueryFilter(u => !u.Eliminado);
            modelBuilder.Entity<Credencial>().HasQueryFilter(c => !c.Eliminado);
            modelBuilder.Entity<Cliente>().HasQueryFilter(c => !c.Eliminado);
            modelBuilder.Entity<Vehiculo>().HasQueryFilter(v => !v.Eliminado);
            modelBuilder.Entity<Trabajo>().HasQueryFilter(t => !t.Eliminado);
            modelBuilder.Entity<DetalleTrabajo>().HasQueryFilter(d => !d.Eliminado);
            modelBuilder.Entity<NotaVehiculo>().HasQueryFilter(n => !n.Eliminado);
            modelBuilder.Entity<CategoriaProducto>().HasQueryFilter(c => !c.Eliminado);
            modelBuilder.Entity<Producto>().HasQueryFilter(p => !p.Eliminado);
            modelBuilder.Entity<Servicio>().HasQueryFilter(s => !s.Eliminado);
            modelBuilder.Entity<Cita>().HasQueryFilter(c => !c.Eliminado);
            modelBuilder.Entity<CobroTrabajo>().HasQueryFilter(c => !c.Eliminado);

            modelBuilder.Entity<Plan>()
                .Property(p => p.PrecioMensual)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Plan>()
                .Property(p => p.PrecioAnual)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Vehiculo>()
                .Property(v => v.PromedioKmDiarios)
                .HasPrecision(8, 2);

            modelBuilder.Entity<Trabajo>()
                .Property(t => t.Subtotal)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Trabajo>()
                .Property(t => t.ImporteImpuestos)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Trabajo>()
                .Property(t => t.Total)
                .HasPrecision(10, 2);

            modelBuilder.Entity<CobroTrabajo>()
                .Property(c => c.Importe)
                .HasPrecision(10, 2);

            modelBuilder.Entity<DetalleTrabajo>()
                .Property(d => d.Cantidad)
                .HasPrecision(8, 2);

            modelBuilder.Entity<DetalleTrabajo>()
                .Property(d => d.PrecioUnitario)
                .HasPrecision(10, 2);

            modelBuilder.Entity<DetalleTrabajo>()
                .Property(d => d.DescuentoPorcentaje)
                .HasPrecision(5, 2);

            modelBuilder.Entity<DetalleTrabajo>()
                .Property(d => d.ImpuestoPorcentaje)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioCompra)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioVenta)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.StockActual)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Servicio>()
                .Property(s => s.PrecioBase)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Servicio>()
                .Property(s => s.ImpuestoPorcentaje)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Factura>()
                .Property(f => f.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Factura>()
                .Property(f => f.ImporteImpuestos)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Factura>()
                .Property(f => f.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LineaFactura>()
                .Property(l => l.Cantidad)
                .HasPrecision(8, 2);

            modelBuilder.Entity<LineaFactura>()
                .Property(l => l.PrecioUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LineaFactura>()
                .Property(l => l.DescuentoPorcentaje)
                .HasPrecision(5, 2);

            modelBuilder.Entity<LineaFactura>()
                .Property(l => l.ImpuestoPorcentaje)
                .HasPrecision(5, 2);

            modelBuilder.Entity<LineaFactura>()
                .Property(l => l.SubtotalLinea)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LineaFactura>()
                .Property(l => l.TotalLinea)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DesgloseIva>()
                .Property(d => d.TipoIvaPorcentaje)
                .HasPrecision(5, 2);

            modelBuilder.Entity<DesgloseIva>()
                .Property(d => d.BaseImponible)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DesgloseIva>()
                .Property(d => d.CuotaIva)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DesgloseIva>()
                .HasIndex(d => new { d.FacturaId, d.TipoIvaPorcentaje })
                .IsUnique();

            modelBuilder.Entity<VehiculoDetalle>()
                .ToView("VW_VehiculoDetalles")
                .HasNoKey();

            modelBuilder.Entity<VehiculoDetalle>()
                .Property(v => v.Eliminado)
                .HasConversion<bool>();

            modelBuilder.Entity<VehiculoDetalle>()
                .Property(v => v.TieneAviso)
                .HasConversion<int>();

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