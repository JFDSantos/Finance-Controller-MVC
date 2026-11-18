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
        protected readonly ICurrentUser _currentUser;

        public BaseRepository(FinanceContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task AddAsync(T entity)
        {
            // Only default the owner when none was given explicitly (e.g. seeding while
            // unauthenticated); reading the current user in that case would throw.
            if (entity is IOwnedEntity owned && owned.UserId == 0)
            {
                owned.UserId = _currentUser.UserId;
            }
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task DeleteAsync(TId id)
        {
            var entity = await GetByIdAsync(id);
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
            // FindAsync ignores global query filters, so the owner check is explicit.
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity is IOwnedEntity owned && owned.UserId != _currentUser.UserId)
            {
                return null;
            }
            return entity;
        }

        public async Task UpdateAsync(TId id, T entity)
        {
            var existing = await GetByIdAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException("Entity not found");
            }
            if (!ReferenceEquals(existing, entity))
            {
                throw new InvalidOperationException("UpdateAsync expects the tracked entity returned by GetByIdAsync.");
            }
            _context.Entry(entity).State = EntityState.Modified;
        }

    }
}
