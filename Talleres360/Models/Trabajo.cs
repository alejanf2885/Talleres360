using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("Trabajos")]
    public class Trabajo
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TallerId")]
        public int? TallerId { get; set; }

        [Column("VehiculoId")]
        public int? VehiculoId { get; set; }

        [Column("MecanicoAsignadoId")]
        public int? MecanicoAsignadoId { get; set; }

        [Column("NumeroDocumento")]
        [StringLength(50)]
        public string? NumeroDocumento { get; set; }

        [Column("TituloMantenimiento")]
        [StringLength(150)]
        public string? TituloMantenimiento { get; set; }

        [Column("TrabajoRealizado")]
        public string? TrabajoRealizado { get; set; }

        [Column("KmEntrada")]
        public int KmEntrada { get; set; }

        [Column("Estado")]
        [Required, StringLength(20)]
        public string Estado { get; set; } = "ABIERTO";

        [Column("EstadoPago")]
        [Required, StringLength(20)]
        public string EstadoPago { get; set; } = "PENDIENTE";

        [Column("Subtotal")]
        public decimal Subtotal { get; set; }

        [Column("ImporteImpuestos")]
        public decimal ImporteImpuestos { get; set; }

        [Column("Total")]
        public decimal Total { get; set; }

        [Column("CreadoPorId")]
        public int? CreadoPorId { get; set; }

        [Column("FechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("ModificadoPorId")]
        public int? ModificadoPorId { get; set; }

        [Column("FechaUltimaModificacion")]
        public DateTime? FechaUltimaModificacion { get; set; }

        [Column("Eliminado")]
        public bool Eliminado { get; set; } = false;

        [Column("DatosIncompletos")]
        public bool DatosIncompletos { get; set; } = false;
    }
}