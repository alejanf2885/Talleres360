using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("Talleres")]
    public class Taller
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("PlanId")]
        public int? PlanId { get; set; }

        [Column("Nombre")]
        [Required, StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        // CAMBIO: CIF -> Cif
        [Column("Cif")]
        [StringLength(20)]
        public string? Cif { get; set; }

        [Column("Direccion")]
        [StringLength(255)]
        public string? Direccion { get; set; }

        [Column("Localidad")]
        [StringLength(100)]
        public string? Localidad { get; set; }

        [Column("Telefono")]
        [StringLength(20)]
        public string? Telefono { get; set; }

        [Column("Logo")]
        public string? Logo { get; set; }

        // --- STRIPE ---
        [Column("StripeCustomerId")]
        public string? StripeCustomerId { get; set; }

        [Column("StripeSubscriptionId")]
        public string? StripeSubscriptionId { get; set; }

        [Column("TipoSuscripcion")]
        public string? TipoSuscripcion { get; set; }

        // NUEVO: campo añadido en la BD nueva
        [Column("EstadoSuscripcion")]
        [Required, StringLength(20)]
        public string EstadoSuscripcion { get; set; } = "ACTIVO";

        [Column("ProximoCobro")]
        public DateTime? ProximoCobro { get; set; }

        // --- ESTADO Y AUDITORÍA ---
        [Column("PerfilConfigurado")]
        public bool PerfilConfigurado { get; set; } = false;

        [Column("FechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column("FechaActualizacion")]
        public DateTime? FechaActualizacion { get; set; }

        [Column("Activo")]
        public bool Activo { get; set; } = true;
    }
}