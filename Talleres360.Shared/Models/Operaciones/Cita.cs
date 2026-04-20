using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Talleres360.Enums;

namespace Talleres360.Models.Operaciones
{
    [Table("Citas")]
    public class Cita
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TallerId")]
        public int? TallerId { get; set; }

        [Column("VehiculoId")]
        public int? VehiculoId { get; set; }

        [Column("NombreClienteTemp")]
        [StringLength(100)]
        public string? NombreClienteTemp { get; set; }

        [Column("FechaCita")]
        public DateTime FechaCita { get; set; }

        [Column("HoraAproximada")]
        [StringLength(50)]
        public string? HoraAproximada { get; set; }

        [Column("Descripcion")]
        [StringLength(255)]
        public string? Descripcion { get; set; }

        [Column("Estado")]
        [Required]
        public CitaEstado Estado { get; set; } = CitaEstado.PENDIENTE;

        [Column("Eliminado")]
        public bool Eliminado { get; set; }
    }
}
