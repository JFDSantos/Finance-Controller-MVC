using Finance.Domain.Models;
using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
{
    public interface IUserRepository : IBaseRepository<User, int>
    {
        Task<User?> GetByEmailAsync(string email);
    }
}
