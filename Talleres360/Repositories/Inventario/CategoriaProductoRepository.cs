using Microsoft.EntityFrameworkCore;
using Talleres360.Data;
using Talleres360.Interfaces.Inventario;

namespace Talleres360.Repositories.Inventario
{
    public class CategoriaProductoRepository : ICategoriaProductoRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoriaProductoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoriaProducto>> ObtenerCategoriasAsync(int tallerId)
        {
            List<CategoriaProducto> categorias = await _context.CategoriasProducto
                .AsNoTracking()
                .Where(categoria => categoria.TallerId == tallerId && !categoria.Eliminado)
                .OrderBy(categoria => categoria.Nombre)
                .ToListAsync();

            return categorias;
        }

        public async Task<CategoriaProducto?> ObtenerPorIdAsync(int id)
        {
            CategoriaProducto? categoria = await _context.CategoriasProducto
                .FirstOrDefaultAsync(categoria => categoria.Id == id && !categoria.Eliminado);

            return categoria;
        }

        public async Task<bool> ExisteNombreAsync(int tallerId, string nombre, int? categoriaExcluirId = null)
        {
            string nombreNormalizado = nombre.Trim().ToUpper();

            bool existe = await _context.CategoriasProducto
                .AsNoTracking()
                .AnyAsync(categoria =>
                    categoria.TallerId == tallerId &&
                    !categoria.Eliminado &&
                    categoria.Nombre.ToUpper() == nombreNormalizado &&
                    (!categoriaExcluirId.HasValue || categoria.Id != categoriaExcluirId.Value));

            return existe;
        }

        public async Task AddAsync(CategoriaProducto categoria)
        {
            await _context.CategoriasProducto.AddAsync(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CategoriaProducto categoria)
        {
            _context.CategoriasProducto.Update(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> PerteneceATallerAsync(int id, int tallerId)
        {
            bool pertenece = await _context.CategoriasProducto
                .AsNoTracking()
                .AnyAsync(categoria => categoria.Id == id && categoria.TallerId == tallerId && !categoria.Eliminado);

            return pertenece;
        }
    }
}
