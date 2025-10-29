using Finance.Domain.Models;
using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserSelectDto>> GetAllAsync();
        Task<UserSelectDto> GetByIdAsync(int id);
        Task AddAsync(User dto);
        Task DeleteAsync(int IdTransaction);
        Task<UserSelectDto> UpdateAsync(int IdTransaction, UserCreateDto dto);
        Task<UserSelectDto> ValidLoginUser(string email, string password);
    }
}
