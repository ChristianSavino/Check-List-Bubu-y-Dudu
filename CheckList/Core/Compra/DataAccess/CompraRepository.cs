using CheckList.Core.Compra.Domain;
using CheckList.Core.Tarea.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace CheckList.Core.Compra.DataAccess
{
    public interface ICompraRepository
    {
        Task<List<CompraEntity>> GetByTipoAsync(TipoCompra tipo);
        Task<CompraEntity?> GetByIdAsync(int id);
        Task AddAsync(CompraEntity compra);
        Task UpdateAsync(CompraEntity compra);
        Task DeleteAsync(int id);
        Task ClearByTipoAsync(TipoCompra tipo);
        Task ReorderAsync(List<int> ids);
    }

    public class CompraRepository : ICompraRepository
    {
        private readonly CheckListDbContext _context;

        public CompraRepository(CheckListDbContext context)
        {
            _context = context;
        }

        public async Task<List<CompraEntity>> GetByTipoAsync(TipoCompra tipo)
        {
            return await _context.Compras
                .Where(c => c.Tipo == tipo)
                .OrderBy(c => c.Completada)
                .ThenBy(c => c.Orden)
                .ToListAsync();
        }

        public async Task<CompraEntity?> GetByIdAsync(int id)
        {
            return await _context.Compras.FindAsync(id);
        }

        public async Task AddAsync(CompraEntity compra)
        {
            var maxOrden = await _context.Compras
                .Where(c => c.Tipo == compra.Tipo)
                .MaxAsync(c => (int?)c.Orden) ?? 0;

            compra.Orden = maxOrden + 1;
            compra.FechaCreacion = DateTime.Now;
            _context.Compras.Add(compra);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CompraEntity compra)
        {
            _context.Compras.Update(compra);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var compra = await _context.Compras.FindAsync(id);
            if (compra != null)
            {
                _context.Compras.Remove(compra);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearByTipoAsync(TipoCompra tipo)
        {
            var compras = await _context.Compras
                .Where(c => c.Tipo == tipo)
                .ToListAsync();
            _context.Compras.RemoveRange(compras);
            await _context.SaveChangesAsync();
        }

        public async Task ReorderAsync(List<int> ids)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Compras SET Orden = {0} WHERE Id = {1}", i, ids[i]);
            }
        }
    }
}
