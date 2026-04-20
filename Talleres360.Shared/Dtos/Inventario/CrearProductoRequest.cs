using System.ComponentModel.DataAnnotations;

namespace Talleres360.Dtos.Inventario
{
    public class CrearProductoRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "La categoría es obligatoria")]
        public int CategoriaId { get; set; }

        [StringLength(50, ErrorMessage = "La referencia no puede superar 50 caracteres")]
        public string? Referencia { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar 150 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Range(0, 99999999.99, ErrorMessage = "El precio de compra debe ser mayor o igual a 0")]
        public decimal PrecioCompra { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "El precio de venta debe ser mayor o igual a 0")]
        public decimal PrecioVenta { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "El stock actual debe ser mayor o igual a 0")]
        public decimal StockActual { get; set; }

        public bool ControlarStock { get; set; }
    }
}
