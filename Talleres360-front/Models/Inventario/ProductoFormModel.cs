using System.ComponentModel.DataAnnotations;

namespace Talleres360_front.Models.Inventario;

public class ProductoFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida")]
    public int CategoriaId { get; set; }

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
    public string? Referencia { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "El precio de compra no puede ser negativo")]
    public decimal PrecioCompra { get; set; }

    [Required(ErrorMessage = "El precio de venta es obligatorio")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio de venta no puede ser negativo")]
    public decimal PrecioVenta { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El stock actual no puede ser negativo")]
    public decimal StockActual { get; set; }

    public bool ControlarStock { get; set; } = true;
}
