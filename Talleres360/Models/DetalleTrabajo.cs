using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("DetallesTrabajo")]
    public class DetalleTrabajo
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TrabajoId")]
        public int TrabajoId { get; set; }

        [Column("ProductoId")]
        public int? ProductoId { get; set; }

        [Column("Concepto")]
        [Required, StringLength(255)]
        public string Concepto { get; set; } = string.Empty;

        [Column("Cantidad")]
        public decimal Cantidad { get; set; } = 1;

        [Column("PrecioUnitario")]
        public decimal PrecioUnitario { get; set; }

        [Column("DescuentoPorcentaje")]
        public decimal DescuentoPorcentaje { get; set; }

        [Column("ImpuestoPorcentaje")]
        public decimal ImpuestoPorcentaje { get; set; } = 21;

        [Column("EsManoObra")]
        public bool EsManoObra { get; set; } = false;

        [Column("EstadoMaterial")]
        [StringLength(50)]
        public string? EstadoMaterial { get; set; }

        [Column("Eliminado")]
        public bool Eliminado { get; set; } = false;
    }
}