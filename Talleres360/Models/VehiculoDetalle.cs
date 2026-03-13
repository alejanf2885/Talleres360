using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Talleres360.Models.Enums;

namespace Talleres360.Models
{
    [Table("VW_VEHICULODETALLES")]
    public class VehiculoDetalle
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("TALLERID")]
        public int TallerId { get; set; }

        [Column("CLIENTEID")]
        public int? ClienteId { get; set; }

        [Column("TIPO_VEHICULO_ID")]
        public int TipoVehiculoId { get; set; }

        [Column("MARCAID")]
        public int MarcaId { get; set; }

        [Column("MODELOID")]
        public int ModeloId { get; set; }

        [Column("MATRICULA")]
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
        public bool Eliminado { get; set; }

        [Column("MARCANOMBRE")]
        public string MarcaNombre { get; set; } = string.Empty;

        [Column("MODELONOMBRE")]
        public string ModeloNombre { get; set; } = string.Empty;

        [NotMapped]
        public TipoVehiculo Tipo => (TipoVehiculo)TipoVehiculoId;
    }
}