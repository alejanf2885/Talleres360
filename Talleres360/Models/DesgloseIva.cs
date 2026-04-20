using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("DesglosesIva")]
    public class DesgloseIva
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("FacturaId")]
        public int FacturaId { get; set; }

        [Column("TipoIvaPorcentaje")]
        public decimal TipoIvaPorcentaje { get; set; }

        [Column("BaseImponible")]
        public decimal BaseImponible { get; set; }

        [Column("CuotaIva")]
        public decimal CuotaIva { get; set; }
    }
}
