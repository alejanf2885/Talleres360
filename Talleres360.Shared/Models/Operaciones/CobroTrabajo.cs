using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Talleres360.Enums;

namespace Talleres360.Models.Operaciones
{
    [Table("CobrosTrabajo")]
    public class CobroTrabajo
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TallerId")]
        public int TallerId { get; set; }

        [Column("TrabajoId")]
        public int TrabajoId { get; set; }

        [Column("Importe")]
        public decimal Importe { get; set; }

        [Column("MetodoPago")]
        public CobroMetodoPago? MetodoPago { get; set; }

        [Column("Referencia")]
        [StringLength(100)]
        public string? Referencia { get; set; }

        [Column("Notas")]
        [StringLength(500)]
        public string? Notas { get; set; }

        [Column("FechaCobro")]
        public DateTime FechaCobro { get; set; }

        [Column("CreadoPorId")]
        public int? CreadoPorId { get; set; }

        [Column("Eliminado")]
        public bool Eliminado { get; set; } = false;
    }
}
