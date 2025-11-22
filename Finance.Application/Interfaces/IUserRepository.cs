using Finance.Domain.Models;
using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetByIdAsync(int id);
        Task AddAsync(User dto);
        Task DeleteAsync(int IdTransaction);
        Task<User> UpdateAsync(int IdTransaction, User dto);
        Task<User> ValidLoginUser(string email, string password);
    }
}
