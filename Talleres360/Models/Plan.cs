using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("PLANES")]
    public class Plan
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Nombre")]
        [Required, StringLength(100)] // Ajustado a 100 como en tu SQL
        public string Nombre { get; set; }

        [Column("PrecioMensual")]
        public decimal PrecioMensual { get; set; }

        [Column("PrecioAnual")] // Faltaba en tu C#
        public decimal PrecioAnual { get; set; }

        // --- LÍMITES (SaaS Core) ---
        [Column("LimiteClientes")] // Faltaba en tu C#
        public int? LimiteClientes { get; set; }

        [Column("LimiteVehiculos")] 
        public int? LimiteVehiculos { get; set; }

        [Column("MaxUsuarios")]
        public int? MaxUsuarios { get; set; }

        // --- MÓDULOS ACTIVOS (Flags para la lógica de negocio) ---
        [Column("ModuloAlertas")]
        public bool? ModuloAlertas { get; set; }

        [Column("ModuloEstadisticas")]
        public bool? ModuloEstadisticas { get; set; }

        [Column("ModuloAgenda")]
        public bool? ModuloAgenda { get; set; }

        [Column("Activo")]
        public bool? Activo { get; set; } = true;
    }
}