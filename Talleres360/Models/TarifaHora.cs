using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("TarifasHora")]
    public class TarifaHora
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TallerId")]
        public int TallerId { get; set; }

        [Column("PrecioHora")]
        public decimal PrecioHora { get; set; }

        [Column("Descripcion")]
        [StringLength(150)]
        public string? Descripcion { get; set; }

        [Column("FechaVigencia")]
        public DateOnly FechaVigencia { get; set; }

        [Column("Activa")]
        public bool Activa { get; set; } = true;

        [Column("CreadoPorId")]
        public int? CreadoPorId { get; set; }

        [Column("FechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
