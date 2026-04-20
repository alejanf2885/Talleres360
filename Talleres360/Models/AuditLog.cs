using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Talleres360.Enums;

namespace Talleres360.Models
{
    [Table("AuditLog")]
    public class AuditLog
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [Column("TallerId")]
        public int? TallerId { get; set; }

        [Column("UsuarioId")]
        public int? UsuarioId { get; set; }

        [Column("Tabla")]
        [Required, StringLength(100)]
        public string Tabla { get; set; } = string.Empty;

        [Column("RegistroId")]
        public long? RegistroId { get; set; }

        [Column("Operacion")]
        [Required]
        public AuditOperacion Operacion { get; set; }

        [Column("ValoresAnteriores")]
        public string? ValoresAnteriores { get; set; }

        [Column("ValoresNuevos")]
        public string? ValoresNuevos { get; set; }

        [Column("FechaHora")]
        public DateTime FechaHora { get; set; } = DateTime.UtcNow;

        [Column("IpOrigen")]
        [StringLength(50)]
        public string? IpOrigen { get; set; }
    }
}
