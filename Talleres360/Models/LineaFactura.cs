using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("LineasFactura")]
    public class LineaFactura
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("FacturaId")]
        public int FacturaId { get; set; }

        [Column("ServicioId")]
        public int? ServicioId { get; set; }

        [Column("Concepto")]
        [StringLength(255)]
        public string Concepto { get; set; } = string.Empty;

        [Column("Cantidad")]
        public decimal Cantidad { get; set; }

        [Column("PrecioUnitario")]
        public decimal PrecioUnitario { get; set; }

        [Column("DescuentoPorcentaje")]
        public decimal DescuentoPorcentaje { get; set; }

        [Column("ImpuestoPorcentaje")]
        public decimal ImpuestoPorcentaje { get; set; }

        [Column("SubtotalLinea")]
        public decimal SubtotalLinea { get; set; }

        [Column("TotalLinea")]
        public decimal TotalLinea { get; set; }
    }
}
