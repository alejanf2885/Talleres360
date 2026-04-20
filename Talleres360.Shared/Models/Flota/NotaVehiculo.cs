using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Talleres360.Enums;

namespace Talleres360.Models.Flota
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
        [Required]
        public NotaVehiculoTipo Tipo { get; set; } = NotaVehiculoTipo.GENERAL;

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