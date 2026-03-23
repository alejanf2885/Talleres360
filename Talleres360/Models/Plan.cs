using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("Planes")]  // CAMBIO: era PLANES
    public class Plan
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Nombre")]
        [Required, StringLength(50)]  
        public string Nombre { get; set; } = string.Empty;

        [Column("PrecioMensual")]
        public decimal PrecioMensual { get; set; }

        [Column("PrecioAnual")]
        public decimal PrecioAnual { get; set; }

        [Column("LimiteClientes")]
        public int LimiteClientes { get; set; }

        [Column("LimiteVehiculos")]
        public int LimiteVehiculos { get; set; }

        [Column("MaxUsuarios")]
        public int MaxUsuarios { get; set; }

        [Column("ModuloAlertas")]
        public bool ModuloAlertas { get; set; }

        [Column("ModuloEstadisticas")]
        public bool ModuloEstadisticas { get; set; }

        [Column("ModuloAgenda")]
        public bool ModuloAgenda { get; set; }

        [Column("Activo")]
        public bool Activo { get; set; } = true;
    }
}