using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Talleres360.Models.Enums;

namespace Talleres360.Models
{
    [Table("Vehiculos")]
    public class Vehiculo
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TallerId")]
        public int TallerId { get; set; }

        [Column("ClienteId")]
        public int? ClienteId { get; set; }

        [Column("VehiculoTipoId")]
        public int VehiculoTipoId { get; set; }

        [Column("MarcaId")]
        public int MarcaId { get; set; }

        [Column("ModeloId")]
        public int ModeloId { get; set; }

        [Column("Matricula")]
        [Required, StringLength(15)]
        public string Matricula { get; set; } = string.Empty;

        [Column("Anno")]
        public int? Anio { get; set; }

        [Column("KmActuales")]
        public int? KmActuales { get; set; }

        [Column("PromedioKmDiarios")]
        public decimal? PromedioKmDiarios { get; set; }

        [Column("FechaUltimaActualizacionKm")]
        public DateTime? FechaUltimaActualizacionKm { get; set; }

        [Column("Eliminado")]
        public bool Eliminado { get; set; } = false;

        [Column("FechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [NotMapped]
        public TipoVehiculo Tipo => (TipoVehiculo)VehiculoTipoId;
    }
}