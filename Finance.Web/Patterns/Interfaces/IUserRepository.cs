using Finance.Web.Models;
using Finance.Web.ViewModel;

namespace Finance.Web.Patterns.Interfaces
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
