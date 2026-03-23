using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Talleres360.Models.Enums;

namespace Talleres360.Models
{
    [Table("VW_VehiculoDetalles")]
    public class VehiculoDetalle
    {
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
        public bool Eliminado { get; set; }

        [Column("MarcaNombre")]
        public string MarcaNombre { get; set; } = string.Empty;

        [Column("ModeloNombre")]
        public string ModeloNombre { get; set; } = string.Empty;

        [Column("TipoNombre")]
        public string TipoNombre { get; set; } = string.Empty;

        [Column("NotasPendientes")]
        public int NotasPendientes { get; set; }

        [Column("TieneAviso")]
        public bool TieneAviso { get; set; }

        [NotMapped]
        public TipoVehiculo Tipo => (TipoVehiculo)VehiculoTipoId;
    }
}