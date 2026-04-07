using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Talleres360.Enums;

namespace Talleres360.Models
{
    [Table("Facturas")]
    public class Factura
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TallerId")]
        public int TallerId { get; set; }

        [Column("TrabajoId")]
        public int? TrabajoId { get; set; }

        [Column("ClienteId")]
        public int ClienteId { get; set; }

        [Column("NumeroFactura")]
        [StringLength(50)]
        public string NumeroFactura { get; set; } = string.Empty;

        [Column("TipoDocumento")]
        public TipoDocumentoComercial TipoDocumento { get; set; } = TipoDocumentoComercial.FACTURA;

        [Column("FacturaRectificadaId")]
        public int? FacturaRectificadaId { get; set; }

        [Column("FechaEmision")]
        public DateTime FechaEmision { get; set; }

        [Column("FechaVencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [Column("Subtotal")]
        public decimal Subtotal { get; set; }

        [Column("ImporteImpuestos")]
        public decimal ImporteImpuestos { get; set; }

        [Column("Total")]
        public decimal Total { get; set; }

        [Column("EstadoPago")]
        [StringLength(20)]
        public string EstadoPago { get; set; } = string.Empty;

        [Column("MetodoPago")]
        [StringLength(50)]
        public string? MetodoPago { get; set; }

        [Column("NotasLegales")]
        public string? NotasLegales { get; set; }

        [Column("ClienteNombre")]
        [StringLength(150)]
        public string ClienteNombre { get; set; } = string.Empty;

        [Column("ClienteNifCif")]
        [StringLength(20)]
        public string ClienteNifCif { get; set; } = string.Empty;

        [Column("ClienteDireccion")]
        [StringLength(500)]
        public string? ClienteDireccion { get; set; }

        [Column("ClienteCodigoPostal")]
        [StringLength(15)]
        public string? ClienteCodigoPostal { get; set; }

        [Column("ClienteLocalidad")]
        [StringLength(150)]
        public string? ClienteLocalidad { get; set; }

        [Column("ClienteProvincia")]
        [StringLength(150)]
        public string? ClienteProvincia { get; set; }
    }
}
