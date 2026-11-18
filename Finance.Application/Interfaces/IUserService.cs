using Finance.Application.ViewModel;
using Finance.Domain.Models;

namespace Finance.Application.Interfaces
{
    public interface IUserService : IBaseService<User, UserCreateDto, UserSelectDto>
    {
        Task<UserSelectDto?> ValidLoginUserAsync(string email, string password);
        Task<UserSelectDto> RegisterAsync(UserCreateDto dto);
        Task DeleteAccountAsync(int userId);
    }
}
