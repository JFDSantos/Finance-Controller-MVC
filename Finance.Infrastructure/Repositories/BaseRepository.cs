using Finance.Application.Interfaces;
using Finance.Domain.Models;
using Finance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Infrastructure.Repositories
{
    public class BaseRepository<T, TId> : IBaseRepository<T, TId>
        where T : class
        where TId : IEquatable<TId>
    {

        protected readonly FinanceContext _context;

        public BaseRepository(FinanceContext context)
        {
            _context = context;
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task DeleteAsync(TId id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
            }
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(TId id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task UpdateAsync(TId id, T entity)
        {
            var entityFind = await _context.Set<T>().FindAsync(id);

            if (entityFind != null)
            {
                _context.Entry(entityFind).CurrentValues.SetValues(entity);
            }

        }

    }
}
