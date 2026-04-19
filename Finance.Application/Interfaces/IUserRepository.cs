using Finance.Domain.Models;
using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User?>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task AddAsync(User dto);
        Task DeleteAsync(int IdTransaction);
        Task UpdateAsync(int IdTransaction, User dto);
        Task<User?> GetByEmailAsync(string email);
    }
}
