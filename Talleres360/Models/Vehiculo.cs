using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Talleres360.Models.Enums;

namespace Talleres360.Models
{
    [Table("VEHICULOS")]
    public class Vehiculo
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("CLIENTEID")]
        public int? ClienteId { get; set; }

        [Column("TIPO_VEHICULO_ID")]
        public int TipoVehiculoId { get; set; }

        [Column("MARCAID")]
        public int MarcaId { get; set; }

        [Column("MODELOID")]
        public int ModeloId { get; set; }

        [Column("MATRICULA")]
        [Required]
        [StringLength(15)]
        public string Matricula { get; set; } = string.Empty;

        [Column("AÑO")]
        public int? Anio { get; set; }

        [Column("KMACTUALES")]
        public int? KmActuales { get; set; }

        [Column("PROMEDIOKMDIARIOS")]
        public decimal? PromedioKmDiarios { get; set; }

        [Column("FECHAULTIMAACTUALIZACIONKM")]
        public DateTime? FechaUltimaActualizacionKm { get; set; }

        [Column("ELIMINADO")]
        public bool Eliminado { get; set; } = false;

        [NotMapped]
        public TipoVehiculo Tipo => (TipoVehiculo)TipoVehiculoId;
    }
}