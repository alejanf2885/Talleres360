using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Dtos;
using Talleres360.Dtos.Inventario;
using Talleres360.Interfaces.Inventario;
using Talleres360.Models;

namespace Talleres360.Repositories.Inventario
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<ProductoDto>> ObtenerProductosPagedAsync(int tallerId, PaginationParams paginacion, string? buscar = null, int? categoriaId = null)
        {
            IQueryable<Producto> queryProductos = _context.Productos
                .AsNoTracking()
                .Where(producto => producto.TallerId == tallerId && !producto.Eliminado);

            if (categoriaId.HasValue)
            {
                queryProductos = queryProductos.Where(producto => producto.CategoriaId == categoriaId.Value);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string criterio = buscar.Trim().ToLower();
                queryProductos = queryProductos.Where(producto =>
                    producto.Nombre.ToLower().Contains(criterio) ||
                    (producto.Referencia != null && producto.Referencia.ToLower().Contains(criterio)));
            }

            int totalCount = await queryProductos.CountAsync();

            List<Producto> productos = await queryProductos
                .OrderBy(producto => producto.Nombre)
                .Skip((paginacion.PageNumber - 1) * paginacion.PageSize)
                .Take(paginacion.PageSize)
                .ToListAsync();

            List<int> categoriaIds = productos
                .Select(producto => producto.CategoriaId)
                .Distinct()
                .ToList();

            Dictionary<int, string> categorias = await _context.CategoriasProducto
                .AsNoTracking()
                .Where(categoria => categoriaIds.Contains(categoria.Id))
                .ToDictionaryAsync(categoria => categoria.Id, categoria => categoria.Nombre);

            List<ProductoDto> data = productos
                .Select(producto => new ProductoDto
                {
                    Id = producto.Id,
                    CategoriaId = producto.CategoriaId,
                    CategoriaNombre = categorias.ContainsKey(producto.CategoriaId)
                        ? categorias[producto.CategoriaId]
                        : string.Empty,
                    Referencia = producto.Referencia,
                    Nombre = producto.Nombre,
                    PrecioCompra = producto.PrecioCompra,
                    PrecioVenta = producto.PrecioVenta,
                    StockActual = producto.StockActual,
                    ControlarStock = producto.ControlarStock
                })
                .ToList();

            PagedResponse<ProductoDto> respuesta = new PagedResponse<ProductoDto>
            {
                Data = data,
                PageNumber = paginacion.PageNumber,
                PageSize = paginacion.PageSize,
                TotalCount = totalCount
            };

            return respuesta;
        }

        public async Task<ProductoDto?> ObtenerDetallePorIdAsync(int productoId)
        {
            ProductoDto? producto = await _context.Productos
                .AsNoTracking()
                .Where(producto => producto.Id == productoId && !producto.Eliminado)
                .Join(
                    _context.CategoriasProducto.AsNoTracking(),
                    producto => producto.CategoriaId,
                    categoria => categoria.Id,
                    (producto, categoria) => new ProductoDto
                    {
                        Id = producto.Id,
                        CategoriaId = producto.CategoriaId,
                        CategoriaNombre = categoria.Nombre,
                        Referencia = producto.Referencia,
                        Nombre = producto.Nombre,
                        PrecioCompra = producto.PrecioCompra,
                        PrecioVenta = producto.PrecioVenta,
                        StockActual = producto.StockActual,
                        ControlarStock = producto.ControlarStock
                    })
                .FirstOrDefaultAsync();

            return producto;
        }

        public async Task<Producto?> ObtenerEntidadPorIdAsync(int productoId)
        {
            Producto? producto = await _context.Productos.FindAsync(productoId);
            return producto;
        }

        public async Task<bool> ExisteNombreAsync(int tallerId, string nombre, int? productoExcluirId = null)
        {
            string nombreNormalizado = nombre.Trim().ToUpper();

            bool existe = await _context.Productos
                .AsNoTracking()
                .AnyAsync(producto =>
                    producto.TallerId == tallerId &&
                    !producto.Eliminado &&
                    producto.Nombre.ToUpper() == nombreNormalizado &&
                    (!productoExcluirId.HasValue || producto.Id != productoExcluirId.Value));

            return existe;
        }

        public async Task<bool> ExisteReferenciaAsync(int tallerId, string referencia, int? productoExcluirId = null)
        {
            string referenciaNormalizada = referencia.Trim().ToUpper();

            bool existe = await _context.Productos
                .AsNoTracking()
                .AnyAsync(producto =>
                    producto.TallerId == tallerId &&
                    !producto.Eliminado &&
                    producto.Referencia != null &&
                    producto.Referencia.ToUpper() == referenciaNormalizada &&
                    (!productoExcluirId.HasValue || producto.Id != productoExcluirId.Value));

            return existe;
        }

        public async Task AddAsync(Producto producto)
        {
            await _context.Productos.AddAsync(producto);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Producto producto)
        {
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> PerteneceATallerAsync(int id, int tallerId)
        {
            bool pertenece = await _context.Productos
                .AsNoTracking()
                .AnyAsync(producto => producto.Id == id && producto.TallerId == tallerId && !producto.Eliminado);

            return pertenece;
        }
    }
}
