using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("Servicios")]
    public class Servicio
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TallerId")]
        public int TallerId { get; set; }

        [Column("Nombre")]
        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Column("Descripcion")]
        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Column("PrecioBase")]
        public decimal PrecioBase { get; set; }

        [Column("ImpuestoPorcentaje")]
        public decimal ImpuestoPorcentaje { get; set; }

        [Column("Activo")]
        public bool Activo { get; set; } = true;

        [Column("Eliminado")]
        public bool Eliminado { get; set; } = false;
    }
}
