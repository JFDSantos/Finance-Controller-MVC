using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Application.Interfaces
{
    public interface IBaseRepository<T, TId>
        where T : class
        where TId : IEquatable<TId>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(TId id);
        Task AddAsync(T entity);
        Task UpdateAsync(TId id, T entity);
        Task DeleteAsync(TId id);
    }
}
