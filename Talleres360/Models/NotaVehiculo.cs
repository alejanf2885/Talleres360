using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("NotasVehiculo")]
    public class NotaVehiculo
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TallerId")]
        public int TallerId { get; set; }

        [Column("VehiculoId")]
        public int VehiculoId { get; set; }

        [Column("UsuarioId")]
        public int? UsuarioId { get; set; }

        [Column("Texto")]
        [Required, StringLength(1000)]
        public string Texto { get; set; } = string.Empty;

        [Column("Tipo")]
        [Required, StringLength(20)]
        public string Tipo { get; set; } = "GENERAL";

        [Column("Resuelta")]
        public bool Resuelta { get; set; } = false;

        [Column("FechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("FechaResolucion")]
        public DateTime? FechaResolucion { get; set; }

        [Column("Eliminado")]
        public bool Eliminado { get; set; } = false;
    }
}