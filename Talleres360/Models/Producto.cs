using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talleres360.Models
{
    [Table("Productos")]
    public class Producto
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TallerId")]
        public int TallerId { get; set; }

        [Column("CategoriaId")]
        public int CategoriaId { get; set; }

        [Column("Referencia")]
        [StringLength(50)]
        public string? Referencia { get; set; }

        [Column("Nombre")]
        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Column("PrecioCompra")]
        public decimal PrecioCompra { get; set; }

        [Column("PrecioVenta")]
        public decimal PrecioVenta { get; set; }

        [Column("StockActual")]
        public decimal StockActual { get; set; }

        [Column("ControlarStock")]
        public bool ControlarStock { get; set; }

        [Column("Eliminado")]
        public bool Eliminado { get; set; }
    }
}
