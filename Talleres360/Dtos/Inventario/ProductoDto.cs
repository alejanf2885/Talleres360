namespace Talleres360.Dtos.Inventario
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public string? Referencia { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal StockActual { get; set; }
        public bool ControlarStock { get; set; }
    }
}
